using Genora.MultiTenancy.AppDtos.AppPaymentConfigurations;
using Genora.MultiTenancy.DomainModels.AppPaymentConfigurations;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppPaymentConfigurations;

[Authorize]
public class AppPaymentConfigurationService : ApplicationService, IAppPaymentConfigurationService
{
    private readonly IRepository<PaymentConfiguration, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public AppPaymentConfigurationService(
        IRepository<PaymentConfiguration, Guid> repository,
        ICurrentTenant currentTenant)
    {
        _repository    = repository;
        _currentTenant = currentTenant;
    }

    // Trả về permission đúng theo Tenant/Host context
    private string P(string tenantPermission)
    {
        if (_currentTenant.IsAvailable) return tenantPermission;

        // Map Tenant prefix → Host prefix
        const string tenantRoot = MultiTenancyPermissions.AppPaymentConfigurations.Default;
        const string hostRoot   = MultiTenancyPermissions.HostAppPaymentConfigurations.Default;

        if (tenantPermission.StartsWith(tenantRoot))
            return hostRoot + tenantPermission.Substring(tenantRoot.Length);

        return hostRoot;
    }

    private async Task CheckAsync(string tenantPermission)
        => await AuthorizationService.CheckAsync(P(tenantPermission));

    public async Task<List<PaymentConfigurationDto>> GetListAsync()
    {
        await CheckAsync(MultiTenancyPermissions.AppPaymentConfigurations.Default);

        var list = await _repository.GetListAsync();
        return list
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.PaymentProviderName)
            .Select(x => ObjectMapper.Map<PaymentConfiguration, PaymentConfigurationDto>(x))
            .ToList();
    }

    public async Task<PaymentConfigurationDto> GetAsync(Guid id)
    {
        await CheckAsync(MultiTenancyPermissions.AppPaymentConfigurations.Default);

        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<PaymentConfiguration, PaymentConfigurationDto>(entity);
    }

    public async Task<PaymentConfigurationDto> CreateAsync(CreateUpdatePaymentConfigurationDto input)
    {
        await CheckAsync(MultiTenancyPermissions.AppPaymentConfigurations.Create);

        var entity = new PaymentConfiguration(GuidGenerator.Create(), input.PaymentProviderName)
        {
            BankBin        = input.BankBin,
            AccountNumber  = input.AccountNumber,
            AccountName    = input.AccountName,
            MerchantId     = input.MerchantId,
            ApiKey         = input.ApiKey,
            Description    = input.Description,
            LogoUrl        = input.LogoUrl,
            IsActive       = input.IsActive,
            DisplayOrder   = input.DisplayOrder
        };

        await _repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<PaymentConfiguration, PaymentConfigurationDto>(entity);
    }

    public async Task<PaymentConfigurationDto> UpdateAsync(Guid id, CreateUpdatePaymentConfigurationDto input)
    {
        await CheckAsync(MultiTenancyPermissions.AppPaymentConfigurations.Edit);

        var entity = await _repository.GetAsync(id);

        entity.PaymentProviderName = input.PaymentProviderName;
        entity.BankBin             = input.BankBin;
        entity.AccountNumber       = input.AccountNumber;
        entity.AccountName         = input.AccountName;
        entity.MerchantId          = input.MerchantId;
        entity.ApiKey              = input.ApiKey;
        entity.Description         = input.Description;
        entity.LogoUrl             = input.LogoUrl;
        entity.IsActive            = input.IsActive;
        entity.DisplayOrder        = input.DisplayOrder;

        await _repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<PaymentConfiguration, PaymentConfigurationDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckAsync(MultiTenancyPermissions.AppPaymentConfigurations.Delete);
        await _repository.DeleteAsync(id, autoSave: true);
    }

    [AllowAnonymous]
    public async Task<PaymentConfigurationDto?> GetActiveAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var entity = query
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .FirstOrDefault();

        return entity == null
            ? null
            : ObjectMapper.Map<PaymentConfiguration, PaymentConfigurationDto>(entity);
    }
}
