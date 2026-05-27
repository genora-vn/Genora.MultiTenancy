using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public class ZaloSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(ZaloSettingNames.AppId, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.AppSecret, "", isVisibleToClients: true, isEncrypted: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.RedirectUri, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.MiniAppId, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.OaId, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.ZbsEnabled, "true", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.ZbsRegisterSuccess, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.ZbsBookingCreated, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.ZbsBookingCancelled, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.ZbsBookingReminder, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.ZbsBookingChanged, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName),

            new SettingDefinition(ZaloSettingNames.ZbsServiceReview, "", isVisibleToClients: true)
                .WithProviders(GlobalSettingValueProvider.ProviderName, TenantSettingValueProvider.ProviderName)
        );
    }
}