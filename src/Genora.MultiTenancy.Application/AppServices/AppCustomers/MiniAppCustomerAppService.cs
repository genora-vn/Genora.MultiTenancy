using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppCustomers;
public class MiniAppCustomerAppService : ApplicationService, IMiniAppCustomerAppService
{
    private readonly IRepository<Customer, Guid> _repo;
    private readonly IMiniAppCustomerTypeService _customerTypeService;
    private readonly IZaloZbsClient _zbsClient;

    public MiniAppCustomerAppService(IRepository<Customer, Guid> repo, IMiniAppCustomerTypeService customerTypeService, IZaloZbsClient zbsClient)
    {
        _repo = repo;
        _customerTypeService = customerTypeService;
        _zbsClient = zbsClient;
    }

    public async Task<MiniAppCustomerDto?> GetByPhoneAsync(string phoneNumber, CancellationToken ct)
    {
        if (phoneNumber.IsNullOrWhiteSpace())
            throw new BusinessException("Customer:PhoneRequired");

        var normalized = phoneNumber.Trim();
        var customer = await _repo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized);
        if (customer == null)
            return null;

        // Map từ entity Customer sang CustomerData
        var customerData = ObjectMapper.Map<Customer, CustomerData>(customer);

        return new MiniAppCustomerDto
        {
            Error = 0,
            Message = "Success",
            Data = customerData
        };
    }

    /// <summary>
    /// Nếu tồn tại theo PhoneNumber -> update mapping; chưa có -> tạo mới.
    /// Idempotent theo PhoneNumber.
    /// </summary>
    public async Task<MiniAppCustomerDto> UpsertFromMiniAppAsync(MiniAppUpsertCustomerRequest input, CancellationToken ct)
    {
        if (input.PhoneNumber.IsNullOrWhiteSpace())
            throw new BusinessException("Customer:PhoneRequired");

        var phone = input.PhoneNumber.Trim();
        var name = (input.FullName ?? "").Trim();
        if (name.IsNullOrWhiteSpace())
            name = "Zalo User";

        var customer = await _repo.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
        var customerType = await _customerTypeService.GetCustomerTypeByCode("MEM");

        var isNew = false;

        if (customer == null)
        {
            isNew = true;

            customer = new Customer(GuidGenerator.Create(), phone, name)
            {
                CustomerTypeId = customerType != null ? customerType?.Id : null,
                AvatarUrl = input.AvatarUrl,
                ZaloUserId = input.ZaloUserId,
                ZaloFollowerId = input.ZaloFollowerId,
                IsFollower = input.IsFollower ?? false,
                IsSensitive = input.IsSensitive ?? false,
                IsActive = true,
                CustomerCode = await GenerateCustomerCodeNoPermissionAsync()
            };

            customer = await _repo.InsertAsync(customer, autoSave: true);
        }
        else
        {
            customer.FullName = name.IsNullOrWhiteSpace() ? customer.FullName : name;
            customer.Gender = input.Gender ?? customer.Gender;
            customer.Email = input.Email ?? customer.Email;
            customer.VgaCode = input.VgaCode ?? customer.VgaCode;
            customer.DateOfBirth = input.DateOfBirth ?? customer.DateOfBirth;
            customer.AvatarUrl = input.AvatarUrl ?? customer.AvatarUrl;
            customer.ZaloUserId = input.ZaloUserId ?? customer.ZaloUserId;
            customer.ZaloFollowerId = input.ZaloFollowerId ?? customer.ZaloFollowerId;
            customer.IsFollower = input.IsFollower ?? customer.IsFollower;

            customer = await _repo.UpdateAsync(customer, autoSave: true);
        }

        // ✅ gửi ZNS “Đăng ký thành công” chỉ khi tạo mới
        if (isNew)
        {
            _ = SendRegisterSuccessZnsSafeAsync(customer, ct);
        }

        var result = ObjectMapper.Map<Customer, CustomerData>(customer);
        result.IsFollower = input.IsFollower;
        result.IsSensitive = input.IsSensitive;

        return new MiniAppCustomerDto
        {
            Error = 0,
            Message = "Success",
            Data = result
        };
    }

    // MiniApp service không yêu cầu quyền => generate code bản internal
    private async Task<string> GenerateCustomerCodeNoPermissionAsync()
    {
        const string prefix = "KH";
        var queryable = await _repo.GetQueryableAsync();

        var codes = queryable
            .Where(c => c.CustomerCode != null && c.CustomerCode.StartsWith(prefix))
            .Select(c => c.CustomerCode!)
            .ToList();

        var maxNumber = 0;
        foreach (var code in codes)
        {
            var numberPart = code.Substring(prefix.Length);
            if (int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
                if (n > maxNumber) maxNumber = n;
        }

        var next = maxNumber + 1;
        var candidate = $"{prefix}{next.ToString("D6", CultureInfo.InvariantCulture)}";

        while (await _repo.AnyAsync(c => c.CustomerCode == candidate))
        {
            next++;
            candidate = $"{prefix}{next.ToString("D6", CultureInfo.InvariantCulture)}";
        }

        return candidate;
    }

    private async Task SendRegisterSuccessZnsSafeAsync(Customer customer, CancellationToken ct)
    {
        try
        {
            var templateId = "TEMPLATE_ID"; // Cấu hình template id lấy từ AppSettings

            var req = new ZaloZbsCallRequest
            {
                Api = "zns",
                Method = "POST",
                Path = "/message/template",
                Query = new Dictionary<string, string?>(),
                Body = new
                {
                    phone = customer.PhoneNumber,                 // "8490xxxxxxx" hoặc "090xxxxxxx"
                    template_id = templateId,
                    template_data = new
                    {
                        customer_name = customer.FullName,
                        customer_code = customer.CustomerCode,
                        // thêm field khác
                    },
                    tracking_id = customer.Id.ToString()          // để đối soát
                }
            };

            await _zbsClient.CallAsync(req, ct);
        }
        catch (Exception ex)
        {
            // Không làm fail luồng đăng ký. Log DB đã được BaseZaloClient ghi (SEND_ZNS)
            // Đây là log app-level để truy vết.
            Logger.LogWarning(ex, "Send ZNS register-success failed for customer {CustomerId}", customer.Id);
        }
    }
}