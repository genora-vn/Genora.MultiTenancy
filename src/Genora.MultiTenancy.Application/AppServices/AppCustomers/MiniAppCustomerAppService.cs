using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppCustomers;

public class MiniAppCustomerAppService : ApplicationService, IMiniAppCustomerAppService
{
    private readonly IRepository<Customer, Guid> _repo;
    private readonly IMiniAppCustomerTypeService _customerTypeService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    private readonly IBackgroundJobManager _jobManager;
    private readonly ICurrentTenant _currentTenant;

    public MiniAppCustomerAppService(
        IRepository<Customer, Guid> repo,
        IMiniAppCustomerTypeService customerTypeService,
        IBackgroundJobManager jobManager,
        ICurrentTenant currentTenant,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _repo = repo;
        _customerTypeService = customerTypeService;
        _jobManager = jobManager;
        _currentTenant = currentTenant;
        _l = l;
    }

    public async Task<MiniAppCustomerDto?> GetByPhoneAsync(string phoneNumber, CancellationToken ct)
    {
        if (phoneNumber.IsNullOrWhiteSpace())
            throw ErrorHelper.BusinessError(_l, "Customer:PhoneRequired");

        var normalized = phoneNumber.Trim();
        var customer = await _repo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct);
        if (customer == null)
            return null;

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
            throw ErrorHelper.BusinessError(_l, "Customer:PhoneRequired");

        var phone = input.PhoneNumber.Trim();
        var name = (input.FullName ?? "").Trim();
        if (name.IsNullOrWhiteSpace())
            name = "Zalo User";

        var customer = await _repo.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct);
        var customerType = await _customerTypeService.GetCustomerTypeByCode("VIS");

        var isNew = false;

        if (customer == null)
        {
            isNew = true;

            customer = new Customer(GuidGenerator.Create(), phone, name)
            {
                CustomerTypeId = customerType?.Id,
                AvatarUrl = input.AvatarUrl,
                ZaloUserId = input.ZaloUserId,
                ZaloFollowerId = input.ZaloFollowerId,
                IsFollower = input.IsFollower ?? false,
                IsSensitive = input.IsSensitive ?? false,
                IsActive = true,
                CustomerCode = await GenerateCustomerCodeNoPermissionAsync(),
            };

            customer.CustomerSource = CustomerSource.ZaloMiniApp;

            customer = await _repo.InsertAsync(customer, autoSave: true, cancellationToken: ct);
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

            customer.CustomerSource = CustomerSource.ZaloMiniApp;

            customer = await _repo.UpdateAsync(customer, autoSave: true, cancellationToken: ct);
        }

        // ✅ gửi ZBS “Đăng ký thành công” chỉ khi tạo mới
        if (isNew && !string.IsNullOrWhiteSpace(customer.PhoneNumber))
        {
            try
            {
                await _jobManager.EnqueueAsync(
                    new ZbsSendJobArgs
                    {
                        TenantId = _currentTenant.Id,
                        TemplateKey = "RegisterSuccess",
                        Phone = customer.PhoneNumber,
                        TrackingId = customer.Id.ToString(),
                        TemplateData = new
                        {
                            customer_name = customer.FullName,
                            customer_id = customer.CustomerCode
                        }
                    },
                    priority: BackgroundJobPriority.Normal
                );
            }
            catch
            {
                // không throw để không block luồng đăng ký
            }
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