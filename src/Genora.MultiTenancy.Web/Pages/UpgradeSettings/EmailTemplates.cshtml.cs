using Genora.MultiTenancy.AppServices.AppEmails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Layout;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.Web.Pages.UpgradeSettings;

[Authorize]
public class EmailTemplatesModel : AbpPageModel
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPageLayout _pageLayout;

    public EmailTemplatesModel(ISettingProvider settingProvider, ISettingManager settingManager, ICurrentTenant currentTenant, IPageLayout pageLayout)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
        _currentTenant = currentTenant;
        _pageLayout = pageLayout;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class EmailTemplateBlock
    {
        public string? To { get; set; }
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string? SubjectTemplate { get; set; }
    }

    public class InputModel
    {
        public EmailTemplateBlock CommonRecipients { get; set; } = new();

        // Chỉ dùng SubjectTemplate cho từng group
        public EmailTemplateBlock BookingNewRequest { get; set; } = new();
        public EmailTemplateBlock BookingChangeRequest { get; set; } = new();
        public EmailTemplateBlock BookingCancelRequest { get; set; } = new();
        public EmailTemplateBlock OrderProductRequest { get; set; } = new();

        public EmailTemplateBlock FnbOrderNewRequest { get; set; } = new();
        public EmailTemplateBlock ProshopOrderNewRequest { get; set; } = new();
        public EmailTemplateBlock CaddieBookingNewRequest { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
        _pageLayout.Content.Title = L["UpgradeSettings:EmailTemplates:Title"].Value;
        //_pageLayout.Content.BreadCrumb.Add(L["UpgradeSettings:EmailTemplates:Title"].Value);

        // Lấy theo BookingNewRequest làm chuẩn
        Input.CommonRecipients.To = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_ToEmails);
        Input.CommonRecipients.Cc = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_CcEmails);
        Input.CommonRecipients.Bcc = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_BccEmails);

        // Subject riêng từng template
        Input.BookingNewRequest.SubjectTemplate = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_SubjectTemplate);
        Input.BookingChangeRequest.SubjectTemplate = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingChange_SubjectTemplate);
        Input.BookingCancelRequest.SubjectTemplate = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingCancel_SubjectTemplate);
        Input.OrderProductRequest.SubjectTemplate = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.OrderProduct_SubjectTemplate);

        Input.FnbOrderNewRequest.SubjectTemplate = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.FnbOrderNew_SubjectTemplate);
        Input.ProshopOrderNewRequest.SubjectTemplate = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.ProshopOrderNew_SubjectTemplate);
        Input.CaddieBookingNewRequest.SubjectTemplate = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.CaddieBookingNew_SubjectTemplate);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var isTenant = _currentTenant.IsAvailable;

        async Task SetAsync(string name, string? value)
        {
            if (isTenant)
                await _settingManager.SetForCurrentTenantAsync(name, value ?? "");
            else
                await _settingManager.SetGlobalAsync(name, value ?? "");
        }

        var to = Input.CommonRecipients.To;
        var cc = Input.CommonRecipients.Cc;
        var bcc = Input.CommonRecipients.Bcc;

        // Booking New
        await SetAsync(AppEmailSettingNames.BookingNew_ToEmails, to);
        await SetAsync(AppEmailSettingNames.BookingNew_CcEmails, cc);
        await SetAsync(AppEmailSettingNames.BookingNew_BccEmails, bcc);
        await SetAsync(AppEmailSettingNames.BookingNew_SubjectTemplate, Input.BookingNewRequest.SubjectTemplate);

        // Booking Change
        await SetAsync(AppEmailSettingNames.BookingChange_ToEmails, to);
        await SetAsync(AppEmailSettingNames.BookingChange_CcEmails, cc);
        await SetAsync(AppEmailSettingNames.BookingChange_BccEmails, bcc);
        await SetAsync(AppEmailSettingNames.BookingChange_SubjectTemplate, Input.BookingChangeRequest.SubjectTemplate);

        // Booking Cancel
        await SetAsync(AppEmailSettingNames.BookingCancel_ToEmails, to);
        await SetAsync(AppEmailSettingNames.BookingCancel_CcEmails, cc);
        await SetAsync(AppEmailSettingNames.BookingCancel_BccEmails, bcc);
        await SetAsync(AppEmailSettingNames.BookingCancel_SubjectTemplate, Input.BookingCancelRequest.SubjectTemplate);

        // Order Hoa Linh Product
        await SetAsync(AppEmailSettingNames.OrderProduct_ToEmails, to);
        await SetAsync(AppEmailSettingNames.OrderProduct_CcEmails, cc);
        await SetAsync(AppEmailSettingNames.OrderProduct_BccEmails, bcc);
        await SetAsync(AppEmailSettingNames.OrderProduct_SubjectTemplate, Input.OrderProductRequest.SubjectTemplate);

        // Order Fnb
        await SetAsync(AppEmailSettingNames.FnbOrderNew_ToEmails, to);
        await SetAsync(AppEmailSettingNames.FnbOrderNew_CcEmails, cc);
        await SetAsync(AppEmailSettingNames.FnbOrderNew_BccEmails, bcc);
        await SetAsync(AppEmailSettingNames.FnbOrderNew_SubjectTemplate, Input.FnbOrderNewRequest.SubjectTemplate);

        // Order Golf Product
        await SetAsync(AppEmailSettingNames.ProshopOrderNew_ToEmails, to);
        await SetAsync(AppEmailSettingNames.ProshopOrderNew_CcEmails, cc);
        await SetAsync(AppEmailSettingNames.ProshopOrderNew_BccEmails, bcc);
        await SetAsync(AppEmailSettingNames.ProshopOrderNew_SubjectTemplate, Input.ProshopOrderNewRequest.SubjectTemplate);

        // Book Caddie
        await SetAsync(AppEmailSettingNames.CaddieBookingNew_ToEmails, to);
        await SetAsync(AppEmailSettingNames.CaddieBookingNew_CcEmails, cc);
        await SetAsync(AppEmailSettingNames.CaddieBookingNew_BccEmails, bcc);
        await SetAsync(AppEmailSettingNames.CaddieBookingNew_SubjectTemplate, Input.CaddieBookingNewRequest.SubjectTemplate);

        Alerts.Success("Đã lưu cấu hình template Email.");
        return RedirectToPage();
    }
}