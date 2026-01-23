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
                defaultValue: "sales@montgomerielinks.com;fo.mlv@montgomerielinks.com",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingNew_CcEmails,
                defaultValue: "",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingNew_BccEmails,
                defaultValue: "",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingNew_SubjectTemplate,
                defaultValue: "[ZALO MINI APP] YÊU CẦU ĐẶT CHỖ MỚI – {BookingCode}",
                isVisibleToClients: false
            )
        );

        // ===== Booking Change Request =====
        context.Add(
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_ToEmails,
                defaultValue: "sales@montgomerielinks.com;fo.mlv@montgomerielinks.com",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_CcEmails,
                defaultValue: "",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_BccEmails,
                defaultValue: "",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingChange_SubjectTemplate,
                defaultValue: "[ZALO MINI APP] YÊU CẦU THAY ĐỔI ĐẶT CHỖ – {BookingCode}",
                isVisibleToClients: false
            )
        );

        // ===== Booking Cancel Request =====
        context.Add(
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_ToEmails,
                defaultValue: "sales@montgomerielinks.com;fo.mlv@montgomerielinks.com",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_CcEmails,
                defaultValue: "",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_BccEmails,
                defaultValue: "",
                isVisibleToClients: false
            ),
            new SettingDefinition(
                AppEmailSettingNames.BookingCancel_SubjectTemplate,
                defaultValue: "[ZALO MINI APP] YÊU CẦU HỦY ĐẶT CHỖ – {BookingCode}",
                isVisibleToClients: false
            )
        );
    }
}