using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppCustomers;

[Authorize]
public class AppCustomerService :
     FeatureProtectedCrudAppService<
         Customer,
         AppCustomerDto,
         Guid,
         GetCustomerListInput,
         CreateUpdateAppCustomerDto>,
     IAppCustomerService
{
    protected override string FeatureName => AppCustomerFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppCustomers.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppCustomers.Default;

    public AppCustomerService(
        IRepository<Customer, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppCustomers.Default;
        GetListPolicyName = MultiTenancyPermissions.AppCustomers.Default;
        CreatePolicyName = MultiTenancyPermissions.AppCustomers.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppCustomers.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppCustomers.Delete;
    }

    public async Task<string> GenerateCustomerCodeAsync()
    {
        await CheckCreatePolicyAsync(); // chỉ ai có quyền Create mới được lấy mã

        const string prefix = "KH";

        var queryable = await Repository.GetQueryableAsync();

        var codes = await AsyncExecuter.ToListAsync(
            queryable.Where(c => c.CustomerCode != null && c.CustomerCode.StartsWith(prefix))
        );

        var maxNumber = 0;

        foreach (var code in codes.Select(c => c.CustomerCode))
        {
            var numberPart = code.Substring(prefix.Length);
            if (int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
            {
                if (n > maxNumber)
                {
                    maxNumber = n;
                }
            }
        }

        // Sinh CustomerCode tiếp theo
        var nextNumber = maxNumber + 1;
        var candidate = $"{prefix}{nextNumber.ToString("D6", CultureInfo.InvariantCulture)}";

        while (await Repository.AnyAsync(c => c.CustomerCode == candidate))
        {
            nextNumber++;
            candidate = $"{prefix}{nextNumber.ToString("D6", CultureInfo.InvariantCulture)}";
        }

        return candidate;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<AppCustomerDto>> GetListAsync(GetCustomerListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        // ========== FILTER ==========
        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(c =>
                c.FullName.Contains(filter) ||
                c.PhoneNumber.Contains(filter) ||
                c.CustomerCode.Contains(filter)
            );
        }

        if (!input.PhoneNumber.IsNullOrWhiteSpace())
        {
            var phone = input.PhoneNumber.Trim();
            query = query.Where(c => c.PhoneNumber.Contains(phone));
        }

        if (!input.FullName.IsNullOrWhiteSpace())
        {
            var name = input.FullName.Trim();
            query = query.Where(c => c.FullName.Contains(name));
        }

        if (input.CustomerTypeId.HasValue)
        {
            query = query.Where(c => c.CustomerTypeId == input.CustomerTypeId);
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == input.IsActive.Value);
        }

        if (input.BirthDateFrom.HasValue)
        {
            query = query.Where(c => c.DateOfBirth >= input.BirthDateFrom.Value);
        }

        if (input.BirthDateTo.HasValue)
        {
            query = query.Where(c => c.DateOfBirth <= input.BirthDateTo.Value);
        }

        // ========== SORT ==========
        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(Customer.CreationTime) + " DESC"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        return new PagedResultDto<AppCustomerDto>(
            totalCount,
            ObjectMapper.Map<List<Customer>, List<AppCustomerDto>>(items)
        );
    }

    // ============= UNIQUE PHONE CHECK =============
    public override async Task<AppCustomerDto> CreateAsync(CreateUpdateAppCustomerDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsurePhoneUniqueAsync(input.PhoneNumber, null);

        var entity = ObjectMapper.Map<CreateUpdateAppCustomerDto, Customer>(input);
        entity = await Repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<Customer, AppCustomerDto>(entity);
    }

    public override async Task<AppCustomerDto> UpdateAsync(Guid id, CreateUpdateAppCustomerDto input)
    {
        await CheckUpdatePolicyAsync();
        await EnsurePhoneUniqueAsync(input.PhoneNumber, id);

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);
        entity = await Repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<Customer, AppCustomerDto>(entity);
    }

    private async Task EnsurePhoneUniqueAsync(string phoneNumber, Guid? currentId)
    {
        if (phoneNumber.IsNullOrWhiteSpace())
        {
            throw new BusinessException("Customer:PhoneRequired");
        }

        var normalized = phoneNumber.Trim();

        var exists = await Repository.AnyAsync(c =>
            c.PhoneNumber == normalized &&
            (!currentId.HasValue || c.Id != currentId.Value)
        );

        if (exists)
        {
            throw new BusinessException("Customer:PhoneAlreadyExists")
                .WithData("PhoneNumber", normalized);
        }
    }

    // ============= GET BY PHONE =============
    public async Task<AppCustomerDto> GetByPhoneAsync(string phoneNumber)
    {
        await CheckGetPolicyAsync();

        if (phoneNumber.IsNullOrWhiteSpace())
        {
            throw new BusinessException("Customer:PhoneRequired");
        }

        var normalized = phoneNumber.Trim();

        var customer = await Repository.FirstOrDefaultAsync(c => c.PhoneNumber == normalized);

        if (customer == null)
        {
            return null;
        }

        return ObjectMapper.Map<Customer, AppCustomerDto>(customer);
    }
}