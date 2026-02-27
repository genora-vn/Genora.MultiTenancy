using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Layout;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.Web.Pages.UpgradeSettings;

[Authorize] // nếu muốn chặt hơn: [Authorize("SettingManagement.Settings")]
public class ZaloZnsModel : AbpPageModel
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPageLayout _pageLayout;

    public ZaloZnsModel(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        ICurrentTenant currentTenant,
        IPageLayout pageLayout)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
        _currentTenant = currentTenant;
        _pageLayout = pageLayout;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Display(Name = "AppId")]
        public string? AppId { get; set; }

        [Display(Name = "AppSecret")]
        public string? AppSecret { get; set; }

        [Display(Name = "RedirectUri")]
        public string? RedirectUri { get; set; }

        [Display(Name = "MiniAppId")]
        public string? MiniAppId { get; set; }

        [Display(Name = "OaId")]
        public string? OaId { get; set; }

        [Display(Name = "Bật gửi ZNS/ZBS")]
        public bool ZbsEnabled { get; set; } = true;

        [Display(Name = "RegisterSuccess TemplateId")]
        public string? RegisterSuccess { get; set; }

        [Display(Name = "BookingCreated TemplateId")]
        public string? BookingCreated { get; set; }

        [Display(Name = "BookingCancelled TemplateId")]
        public string? BookingCancelled { get; set; }

        [Display(Name = "BookingReminder TemplateId")]
        public string? BookingReminder { get; set; }

        [Display(Name = "BookingChanged TemplateId")]
        public string? BookingChanged { get; set; }
    }

    public async Task OnGetAsync()
    {
        _pageLayout.Content.Title = L["UpgradeSettings:ZaloZns:Title"].Value;
        //_pageLayout.Content.BreadCrumb.Add(L["UpgradeSettings:EmailTemplates:Title"].Value);

        Input.AppId = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.AppId);
        Input.AppSecret = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.AppSecret);
        Input.RedirectUri = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.RedirectUri);
        Input.MiniAppId = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.MiniAppId);
        Input.OaId = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.OaId);

        var enabledRaw = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsEnabled);
        Input.ZbsEnabled = string.IsNullOrWhiteSpace(enabledRaw) ? true : bool.TryParse(enabledRaw, out var b) ? b : true;

        Input.RegisterSuccess = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsRegisterSuccess);
        Input.BookingCreated = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCreated);
        Input.BookingCancelled = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCancelled);
        Input.BookingReminder = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingReminder);
        Input.BookingChanged = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingChanged);
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

        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.AppId, Input.AppId);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.AppSecret, Input.AppSecret);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.RedirectUri, Input.RedirectUri);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.MiniAppId, Input.MiniAppId);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.OaId, Input.OaId);

        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsEnabled, Input.ZbsEnabled.ToString());

        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsRegisterSuccess, Input.RegisterSuccess);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCreated, Input.BookingCreated);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCancelled, Input.BookingCancelled);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingReminder, Input.BookingReminder);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingChanged, Input.BookingChanged);

        Alerts.Success("Đã lưu cấu hình Zalo/ZNS.");
        return RedirectToPage();
    }
}