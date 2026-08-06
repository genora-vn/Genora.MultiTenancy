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

        // ===== Order Product Request =====
        context.Add(
           new SettingDefinition(
               AppEmailSettingNames.OrderProduct_ToEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.OrderProduct_CcEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.OrderProduct_BccEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.OrderProduct_SubjectTemplate,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           )
       );

        // ===== Order FnB Request =====
        context.Add(
           new SettingDefinition(
               AppEmailSettingNames.FnbOrderNew_ToEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.FnbOrderNew_CcEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.FnbOrderNew_BccEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.FnbOrderNew_SubjectTemplate,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           )
       );

        // ===== Order Proshop Request =====
        context.Add(
           new SettingDefinition(
               AppEmailSettingNames.ProshopOrderNew_ToEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.ProshopOrderNew_CcEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.ProshopOrderNew_BccEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.ProshopOrderNew_SubjectTemplate,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           )
       );

        // ===== Booking Caddie Request =====
        context.Add(
           new SettingDefinition(
               AppEmailSettingNames.CaddieBookingNew_ToEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.CaddieBookingNew_CcEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.CaddieBookingNew_BccEmails,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           ),
           new SettingDefinition(
               AppEmailSettingNames.CaddieBookingNew_SubjectTemplate,
               defaultValue: "",
               isVisibleToClients: false
           ).WithProviders(
               GlobalSettingValueProvider.ProviderName,
               TenantSettingValueProvider.ProviderName
           )
       );
    }
}