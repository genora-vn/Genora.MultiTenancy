using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyaltyConfigs;
using Genora.MultiTenancy.AppServices.SalonBeauty;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyLoyaltyConfigAppService : ApplicationService, ISalonBeautyLoyaltyConfigAppService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;
    private readonly ICurrentTenant _currentTenant;

    public SalonBeautyLoyaltyConfigAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        ICurrentTenant currentTenant)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
        _currentTenant = currentTenant;
    }

    public async Task<SalonBeautyLoyaltyConfigDto> GetAsync()
    {
        await CheckPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default,
            MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default);

        var raw = await _settingProvider.GetOrNullAsync(SalonBeautyLoyaltySettingNames.ExchangeRate);
        decimal rate = 1000m;
        if (decimal.TryParse(raw, out var parsed) && parsed > 0)
            rate = parsed;

        return new SalonBeautyLoyaltyConfigDto { ExchangeRate = rate };
    }

    public async Task<SalonBeautyLoyaltyConfigDto> UpdateAsync(SalonBeautyLoyaltyConfigDto input)
    {
        await CheckPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Edit,
            MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Edit);

        if (input.ExchangeRate <= 0)
            throw new UserFriendlyException("Tỷ lệ quy đổi phải > 0.");

        if (_currentTenant.IsAvailable)
        {
            await _settingManager.SetForCurrentTenantAsync(
                SalonBeautyLoyaltySettingNames.ExchangeRate,
                input.ExchangeRate.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else
        {
            await _settingManager.SetGlobalAsync(
                SalonBeautyLoyaltySettingNames.ExchangeRate,
                input.ExchangeRate.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return new SalonBeautyLoyaltyConfigDto { ExchangeRate = input.ExchangeRate };
    }

    private async Task CheckPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = _currentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (string.IsNullOrWhiteSpace(permission))
            throw new AbpAuthorizationException("Missing loyalty config permission.");
        await AuthorizationService.CheckAsync(permission);
    }
}
