using Genora.MultiTenancy.AppServices.AppPayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Layout;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.Web.Pages.UpgradeSettings;

[Authorize]
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

        [Display(Name = "OrderSuccess TemplateId")]
        public string? OrderSuccess { get; set; }

        [Display(Name = "RedeemPoint TemplateId")]
        public string? RedeemPoint { get; set; }

        [Display(Name = "ExchangeGift TemplateId")]
        public string? ExchangeGift { get; set; }

        // ── ZBS Admin Notification Settings ──────────────────────────
        [Display(Name = "FnbOrder TemplateId")]
        public string? FnbOrder { get; set; }

        [Display(Name = "ProshopOrder TemplateId")]
        public string? ProshopOrder { get; set; }

        [Display(Name = "CaddieBooking TemplateId")]
        public string? CaddieBooking { get; set; }

        [Display(Name = "GolfBookingPhoneNumber TemplateId")]
        public string? GolfBookingPhoneNumber { get; set; }

        [Display(Name = "FnbBookingPhoneNumber TemplateId")]
        public string? FnbBookingPhoneNumber { get; set; }

        [Display(Name = "ProshopOrderPhoneNumber TemplateId")]
        public string? ProshopOrderPhoneNumber { get; set; }

        [Display(Name = "CaddieBookingPhoneNumber TemplateId")]
        public string? CaddieBookingPhoneNumber { get; set; }

        // ── Checkout SDK — Payment Config ────────────────────────────
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
        Input.ServiceReview = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsServiceReview);

        Input.OrderSuccess = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsOrderSuccess);
        Input.RedeemPoint = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsRedeemPoint);
        Input.ExchangeGift = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsExchangeGift);

        // ── Load ZBS Admin Notification Settings ──────────────────────
        Input.FnbOrder = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsFnbOrder);
        Input.ProshopOrder = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsProshopOrder);
        Input.CaddieBooking = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsCaddieBooking);
        Input.GolfBookingPhoneNumber = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsGolfBookingPhoneNumber);
        Input.FnbBookingPhoneNumber = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsFnbOrderPhoneNumber);
        Input.ProshopOrderPhoneNumber = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsProshopOrderPhoneNumber);
        Input.CaddieBookingPhoneNumber = await _settingProvider.GetOrNullAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsCaddieBookingPhoneNumber);

        // ── Payment settings ──────────────────────────────────────────
        Input.PaymentPrivateKey = null;
        Input.BankName = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankName);
        Input.BankAccountNumber = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountNumber);
        Input.BankAccountOwner = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountOwner);
        Input.BankBranch = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankBranch);

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
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCreated, Input.BookingCreated);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingCancelled, Input.BookingCancelled);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingReminder, Input.BookingReminder);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsBookingChanged, Input.BookingChanged);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsServiceReview, Input.ServiceReview);

        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsOrderSuccess, Input.OrderSuccess);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsRedeemPoint, Input.RedeemPoint);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsExchangeGift, Input.ExchangeGift);

        // ── Save ZBS Admin Notification Settings ──────────────────────
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsFnbOrder, Input.FnbOrder);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsProshopOrder, Input.ProshopOrder);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsCaddieBooking, Input.CaddieBooking);

        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsGolfBookingPhoneNumber, Input.GolfBookingPhoneNumber);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsFnbOrderPhoneNumber, Input.FnbBookingPhoneNumber);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsProshopOrderPhoneNumber, Input.ProshopOrderPhoneNumber);
        await SetAsync(AppServices.AppZaloAuths.ZaloSettingNames.ZbsCaddieBookingPhoneNumber, Input.CaddieBookingPhoneNumber);

        // ── Payment settings ──────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(Input.PaymentPrivateKey))
            await SetAsync(ZaloPaymentSettingNames.PrivateKey, Input.PaymentPrivateKey);

        await SetAsync(ZaloPaymentSettingNames.BankName, Input.BankName);
        await SetAsync(ZaloPaymentSettingNames.BankAccountNumber, Input.BankAccountNumber);
        await SetAsync(ZaloPaymentSettingNames.BankAccountOwner, Input.BankAccountOwner);
        await SetAsync(ZaloPaymentSettingNames.BankBranch, Input.BankBranch);

        await SetAsync(ZaloPaymentSettingNames.IsPayAtCounterEnabled, Input.IsPayAtCounterEnabled.ToString());
        await SetAsync(ZaloPaymentSettingNames.IsPayBankTransferEnabled, Input.IsPayBankTransferEnabled.ToString());

        Alerts.Success("Đã lưu cấu hình Zalo/ZNS.");
        return RedirectToPage();
    }
}