using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Đăng ký/đồng bộ khách hàng Hoa Linh vào dbo.AppCustomers.
/// Idempotent theo số điện thoại (PhoneNumber).
/// Internal service — chỉ controller gọi, không expose auto-API (method có 2 complex-type param).
/// [DisableValidation]: đã validate ở tầng controller; tránh ABP validation interceptor
/// báo lỗi khi hlCustomer = null (trường hợp khách chưa có bên HL DMS).
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlCustomerAppService : ApplicationService, IHlCustomerAppService
{
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<HlCustomerAppService> _logger;

    public HlCustomerAppService(
        IRepository<Customer, Guid> customerRepo,
        ICurrentTenant currentTenant,
        ILogger<HlCustomerAppService> logger)
    {
        _customerRepo = customerRepo;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<HlCustomerDto> UpsertFromHoaLinhAsync(HlCheckCustomerRequest request, HlCustomerDto? hlCustomer = null, CancellationToken ct = default)
    {
        var phone = NormalizePhone(request.PhoneNumber ?? hlCustomer?.Phone ?? hlCustomer?.CustPhone);
        if (string.IsNullOrWhiteSpace(phone))
        {
            _logger.LogWarning("HL upsert customer bỏ qua: thiếu số điện thoại");
            // Vẫn trả về DTO (thông tin từ HL DMS nếu có, hoặc từ Mini App)
            return hlCustomer ?? BuildDtoFromRequest(request, null, CustomerSource.ZaloMiniApp);
        }

        var existing = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct);
        var isFromHl = hlCustomer != null;

        // Tên: ưu tiên tên bên HL DMS, fallback tên từ Mini App
        var name = FirstNonBlank(hlCustomer?.CustName, request.FullName) ?? "Zalo User";

        Customer customer;

        if (existing == null)
        {
            customer = new Customer(GuidGenerator.Create(), phone, name)
            {
                TenantId = _currentTenant.Id,
                AvatarUrl = NullIfBlank(request.AvatarUrl),
                ZaloUserId = NullIfBlank(request.ZaloUserId),
                IsFollower = request.IsFollower ?? false,
                IsActive = true,
                Email = null,
                Address = NullIfBlank(hlCustomer?.Address),
                DateOfBirth = ParseBirthday(hlCustomer?.Birthday),
                CustomerCode = isFromHl
                    ? NullIfBlank(hlCustomer!.CustCode)
                    : await GenerateCustomerCodeAsync(),
                CustomerSource = isFromHl ? CustomerSource.HoaLinh : CustomerSource.ZaloMiniApp,
            };

            customer = await _customerRepo.InsertAsync(customer, autoSave: true, cancellationToken: ct);

            _logger.LogInformation("HL upsert: tạo mới KH {Phone} code={Code} source={Source}",
                phone, customer.CustomerCode, customer.CustomerSource);
        }
        else
        {
            // Cập nhật thông tin, chỉ ghi đè khi có giá trị mới (giữ dữ liệu cũ nếu null)
            existing.FullName = string.IsNullOrWhiteSpace(name) ? existing.FullName : name;
            existing.AvatarUrl = NullIfBlank(request.AvatarUrl) ?? existing.AvatarUrl;
            existing.ZaloUserId = NullIfBlank(request.ZaloUserId) ?? existing.ZaloUserId;
            existing.IsFollower = request.IsFollower ?? existing.IsFollower;
            existing.Address = NullIfBlank(hlCustomer?.Address) ?? existing.Address;
            existing.DateOfBirth = ParseBirthday(hlCustomer?.Birthday) ?? existing.DateOfBirth;

            // Nếu khách đã có bên HL DMS → cập nhật mã KH + nguồn HoaLinh
            if (isFromHl)
            {
                existing.CustomerCode = NullIfBlank(hlCustomer!.CustCode) ?? existing.CustomerCode;
                existing.CustomerSource = CustomerSource.HoaLinh;
            }

            customer = await _customerRepo.UpdateAsync(existing, autoSave: true, cancellationToken: ct);

            _logger.LogInformation("HL upsert: cập nhật KH {Phone} code={Code} source={Source}",
                phone, customer.CustomerCode, customer.CustomerSource);
        }

        // Nếu có dữ liệu HL DMS → trả nguyên thông tin DMS (đầy đủ loyalty/tier...).
        // Nếu không → build DTO từ bản ghi AppCustomers vừa lưu.
        return hlCustomer ?? MapEntityToDto(customer);
    }

    public async Task<List<HlCustomerDto>> GetFromAppCustomersAsync(string phone, CancellationToken ct = default)
    {
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized)) return new List<HlCustomerDto>();

        var queryable = await _customerRepo.GetQueryableAsync();
        var customers = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.PhoneNumber == normalized).OrderBy(x => x.CreationTime), ct);

        var dtos = customers.Select(MapEntityToDto).ToList();
        await EnrichBonusAmountAsync(dtos, ct);
        return dtos;
    }

    public async Task EnrichBonusAmountAsync(List<HlCustomerDto> customers, CancellationToken ct = default)
    {
        if (customers == null || customers.Count == 0) return;

        // Lấy các mã KH đủ điều kiện: có custCode + custChannel = "OTC" + isGkhl = true
        var eligibleCodes = customers
            .Where(c => !string.IsNullOrWhiteSpace(c.CustCode)
                        && string.Equals(c.CustChannel, "OTC", StringComparison.OrdinalIgnoreCase)
                        && c.IsGkhl == true)
            .Select(c => c.CustCode!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Mặc định BonusAmount = 0 cho tất cả
        foreach (var c in customers) c.BonusAmount = 0;

        if (eligibleCodes.Count == 0) return;

        // Tra BonusAmount từ dbo.AppCustomers theo CustomerCode
        var queryable = await _customerRepo.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerCode != null && eligibleCodes.Contains(x.CustomerCode))
                     .Select(x => new { x.CustomerCode, x.BonusAmount }), ct);

        var byCode = rows
            .GroupBy(x => x.CustomerCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.BonusAmount), StringComparer.OrdinalIgnoreCase);

        foreach (var c in customers)
        {
            if (!string.IsNullOrWhiteSpace(c.CustCode)
                && string.Equals(c.CustChannel, "OTC", StringComparison.OrdinalIgnoreCase)
                && c.IsGkhl == true
                && byCode.TryGetValue(c.CustCode!, out var amount))
            {
                c.BonusAmount = amount;
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Map entity Customer (dbo.AppCustomers) → HlCustomerDto. Trường không có → null.</summary>
    private static HlCustomerDto MapEntityToDto(Customer c)
    {
        return new HlCustomerDto
        {
            CustCode = c.CustomerCode,
            CustName = c.FullName,
            CustPhone = c.PhoneNumber,
            Phone = c.PhoneNumber,
            Address = c.Address,
            Birthday = c.DateOfBirth?.ToString("yyyy-MM-dd"),
            IsCustomer = c.CustomerSource == CustomerSource.HoaLinh,
        };
    }

    /// <summary>Build DTO từ request Mini App khi chưa lưu được entity (fallback hiếm gặp).</summary>
    private static HlCustomerDto BuildDtoFromRequest(HlCheckCustomerRequest request, string? custCode, CustomerSource source)
    {
        return new HlCustomerDto
        {
            CustCode = custCode,
            CustName = NullIfBlank(request.FullName),
            CustPhone = NullIfBlank(request.PhoneNumber),
            Phone = NullIfBlank(request.PhoneNumber),
            IsCustomer = source == CustomerSource.HoaLinh,
        };
    }

    private async Task<string> GenerateCustomerCodeAsync()
    {
        const string prefix = "HLKH";
        var queryable = await _customerRepo.GetQueryableAsync();

        var maxNumber = 0;
        foreach (var code in queryable
                     .Where(c => c.CustomerCode != null && c.CustomerCode.StartsWith(prefix))
                     .Select(c => c.CustomerCode!))
        {
            var numberPart = code.Substring(prefix.Length);
            if (int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n > maxNumber)
                maxNumber = n;
        }

        var next = maxNumber + 1;
        var candidate = $"{prefix}{next.ToString("D6", CultureInfo.InvariantCulture)}";

        while (await _customerRepo.AnyAsync(c => c.CustomerCode == candidate))
        {
            next++;
            candidate = $"{prefix}{next.ToString("D6", CultureInfo.InvariantCulture)}";
        }

        return candidate;
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        phone = phone.Trim();
        // DB golf lưu đầu 84 hoặc 0; giữ nguyên như nhập, chỉ bỏ khoảng trắng/dấu
        return System.Text.RegularExpressions.Regex.Replace(phone, @"\s+|-|\.", "");
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? FirstNonBlank(params string?[] values)
    {
        foreach (var v in values)
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        return null;
    }

    /// <summary>Parse birthday từ HL DMS (nhiều format có thể có). Trả null nếu không parse được.</summary>
    private static DateTime? ParseBirthday(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-ddTHH:mm:ss", "dd-MM-yyyy" };
        if (DateTime.TryParseExact(raw.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d.Date;
        if (DateTime.TryParse(raw.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            return d.Date;
        return null;
    }
}
