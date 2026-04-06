using Genora.MultiTenancy.AppServices.AppFnbOrders;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Realtime;
using Genora.MultiTenancy.SignalR;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Genora.MultiTenancy;

 [DependsOn(
    typeof(MultiTenancyApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule)
    )]
public class MultiTenancyHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureLocalization();
        context.Services.AddTransient<IFnbOrderRealtimeNotifier, FnbOrderRealtimeNotifier>();
        context.Services.AddTransient<IProOrderRealtimeNotifier, ProOrderRealtimeNotifier>();
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<MultiTenancyResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }
}
