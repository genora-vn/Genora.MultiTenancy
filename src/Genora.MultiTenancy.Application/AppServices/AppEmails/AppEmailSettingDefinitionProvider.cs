using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppEmails;
public class AppEmailSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        // ===== Booking New Request =====
        context.Add(
            new SettingDefinition(
                AppEmailSettingNames.BookingNew_ToEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingNew_CcEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingNew_BccEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingNew_SubjectTemplate,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            )
        );

        // ===== Booking Change Request =====
        context.Add(
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_ToEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_CcEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_BccEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_SubjectTemplate,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            )
        );

        // ===== Booking Cancel Request =====
        context.Add(
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_ToEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_CcEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_BccEmails,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_SubjectTemplate,
                defaultValue: "",
                isVisibleToClients: false
            ).WithProviders(
                GlobalSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            )
        );
    }
}