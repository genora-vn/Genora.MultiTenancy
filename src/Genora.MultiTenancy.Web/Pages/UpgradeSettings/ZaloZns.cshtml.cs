using Genora.MultiTenancy.AppServices.AppPayments;
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

        [Display(Name = "ServiceReview TemplateId")]
        public string? ServiceReview { get; set; }

        // ── Checkout SDK — Payment Config ────────────────────────────────────
        /// <summary>
        /// Private Key HMAC SHA256 từ Zalo Developer Portal.
        /// Để trống = không thay đổi giá trị hiện tại.
        /// </summary>
        [Display(Name = "Private Key (Checkout SDK)")]
        public string? PaymentPrivateKey { get; set; }

        [Display(Name = "Tên ngân hàng")]
        public string? BankName { get; set; }

        [Display(Name = "Số tài khoản")]
        public string? BankAccountNumber { get; set; }

        [Display(Name = "Chủ tài khoản")]
        public string? BankAccountOwner { get; set; }

        [Display(Name = "Chi nhánh")]
        public string? BankBranch { get; set; }

        [Display(Name = "Thanh toán tại quầy")]
        public bool IsPayAtCounterEnabled { get; set; } = true;

        [Display(Name = "Thanh toán chuyển khoản")]
        public bool IsPayBankTransferEnabled { get; set; } = true;
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

        Input.RegisterSuccess  = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsRegisterSuccess);
        Input.BookingCreated   = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCreated);
        Input.BookingCancelled = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCancelled);
        Input.BookingReminder  = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingReminder);
        Input.BookingChanged   = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingChanged);
        Input.ServiceReview    = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsServiceReview);

        // ── Payment settings ─────────────────────────────────────────────────
        // Private Key không load ra UI (encrypted) — chỉ hiển thị placeholder
        Input.PaymentPrivateKey  = null;
        Input.BankName           = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankName);
        Input.BankAccountNumber  = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountNumber);
        Input.BankAccountOwner   = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountOwner);
        Input.BankBranch         = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankBranch);

        var payAtCounterRaw = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.IsPayAtCounterEnabled);
        Input.IsPayAtCounterEnabled = string.IsNullOrWhiteSpace(payAtCounterRaw)
            ? true
            : bool.TryParse(payAtCounterRaw, out var pc) ? pc : true;

        var payBankRaw = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.IsPayBankTransferEnabled);
        Input.IsPayBankTransferEnabled = string.IsNullOrWhiteSpace(payBankRaw)
            ? true
            : bool.TryParse(payBankRaw, out var pb) ? pb : true;
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
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCreated,   Input.BookingCreated);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCancelled, Input.BookingCancelled);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingReminder,  Input.BookingReminder);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingChanged,   Input.BookingChanged);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsServiceReview,    Input.ServiceReview);

        // ── Payment settings ─────────────────────────────────────────────────
        // Private Key: chỉ lưu nếu user nhập giá trị mới (không ghi đè bằng chuỗi rỗng)
        if (!string.IsNullOrWhiteSpace(Input.PaymentPrivateKey))
            await SetAsync(ZaloPaymentSettingNames.PrivateKey, Input.PaymentPrivateKey);

        await SetAsync(ZaloPaymentSettingNames.BankName,          Input.BankName);
        await SetAsync(ZaloPaymentSettingNames.BankAccountNumber,  Input.BankAccountNumber);
        await SetAsync(ZaloPaymentSettingNames.BankAccountOwner,   Input.BankAccountOwner);
        await SetAsync(ZaloPaymentSettingNames.BankBranch,         Input.BankBranch);

        await SetAsync(ZaloPaymentSettingNames.IsPayAtCounterEnabled,    Input.IsPayAtCounterEnabled.ToString());
        await SetAsync(ZaloPaymentSettingNames.IsPayBankTransferEnabled, Input.IsPayBankTransferEnabled.ToString());

        Alerts.Success("Đã lưu cấu hình Zalo/ZNS.");
        return RedirectToPage();
    }
}