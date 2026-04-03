using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Đăng ký các setting thanh toán MỚI với ABP Setting System.
/// Lưu ý: AppId (MiniAppId) đã được đăng ký bởi ZaloSettingDefinitionProvider,
/// nên ở đây chỉ đăng ký PrivateKey và Bank settings.
/// </summary>
public class ZaloPaymentSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            // Private Key lưu encrypted — không hiển thị ra client
            new SettingDefinition(
                ZaloPaymentSettingNames.PrivateKey,
                defaultValue: "",
                isVisibleToClients: false,
                isEncrypted: true
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),

            // ── Bank Transfer Config ────────────────────────────────────────
            new SettingDefinition(
                ZaloPaymentSettingNames.BankName,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),

            new SettingDefinition(
                ZaloPaymentSettingNames.BankAccountNumber,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),

            new SettingDefinition(
                ZaloPaymentSettingNames.BankAccountOwner,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),

            new SettingDefinition(
                ZaloPaymentSettingNames.BankBranch,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            )
        );
    }
}

