using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using System;
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

    public MiniAppCustomerAppService(IRepository<Customer, Guid> repo, IMiniAppCustomerTypeService customerTypeService)
    {
        _repo = repo;
        _customerTypeService = customerTypeService;
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
            name = "Zalo User"; // fallback

        var customer = await _repo.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
        var customerType = await _customerTypeService.GetCustomerTypeByCode("MEM");

        if (customer == null)
        {
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
            // mapping update (chỉ update nếu có dữ liệu mới)
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

        //var dto = MapToDto(customer);
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
}