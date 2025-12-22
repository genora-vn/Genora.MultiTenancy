using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppCustomers;
public class MiniAppCustomerAppService : ApplicationService, IMiniAppCustomerAppService
{
    private readonly IRepository<Customer, Guid> _repo;

    public MiniAppCustomerAppService(IRepository<Customer, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<MiniAppCustomerDto?> GetByPhoneAsync(string phoneNumber)
    {
        if (phoneNumber.IsNullOrWhiteSpace())
            throw new BusinessException("Customer:PhoneRequired");

        var normalized = phoneNumber.Trim();
        var customer = await _repo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized);
        return customer == null ? null : ObjectMapper.Map<Customer, MiniAppCustomerDto>(customer);
    }

    /// <summary>
    /// Nếu tồn tại theo PhoneNumber -> update mapping; chưa có -> tạo mới.
    /// Idempotent theo PhoneNumber.
    /// </summary>
    public async Task<MiniAppCustomerDto> UpsertFromMiniAppAsync(MiniAppUpsertCustomerRequest input)
    {
        if (input.PhoneNumber.IsNullOrWhiteSpace())
            throw new BusinessException("Customer:PhoneRequired");

        var phone = input.PhoneNumber.Trim();
        var name = (input.FullName ?? "").Trim();
        if (name.IsNullOrWhiteSpace())
            name = "Zalo User"; // fallback

        var customer = await _repo.FirstOrDefaultAsync(x => x.PhoneNumber == phone);

        if (customer == null)
        {
            customer = new Customer(GuidGenerator.Create(), phone, name)
            {
                AvatarUrl = input.AvatarUrl,
                ZaloUserId = input.ZaloUserId,
                ZaloFollowerId = input.ZaloFollowerId,
                IsActive = true,
                CustomerCode = await GenerateCustomerCodeNoPermissionAsync()
            };

            customer = await _repo.InsertAsync(customer, autoSave: true);
        }
        else
        {
            // mapping update (chỉ update nếu có dữ liệu mới)
            customer.FullName = name.IsNullOrWhiteSpace() ? customer.FullName : name;
            customer.AvatarUrl = input.AvatarUrl ?? customer.AvatarUrl;
            customer.ZaloUserId = input.ZaloUserId ?? customer.ZaloUserId;
            customer.ZaloFollowerId = input.ZaloFollowerId ?? customer.ZaloFollowerId;

            customer = await _repo.UpdateAsync(customer, autoSave: true);
        }

        //var dto = MapToDto(customer);
        var dto = ObjectMapper.Map<Customer, MiniAppCustomerDto>(customer);
        dto.IsFollower = input.IsFollower;
        dto.IsSensitive = input.IsSensitive;
        return dto;
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
}