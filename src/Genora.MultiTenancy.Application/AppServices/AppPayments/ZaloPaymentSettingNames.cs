using Genora.MultiTenancy.AppServices.AppZaloAuths;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Tên các setting cho Zalo Checkout SDK — lưu per-tenant qua ABP Setting Management.
///
/// AppId của Checkout SDK = MiniAppId đã có sẵn trong ZaloSettingNames,
/// dùng lại để tránh cấu hình trùng lặp.
/// </summary>
public static class ZaloPaymentSettingNames
{
    // ── Zalo Checkout — dùng lại key đã có trong ZaloSettingNames ──────────
    /// <summary>App ID của Mini App (dùng chung với ZaloSettingNames.MiniAppId)</summary>
    public const string AppId = ZaloSettingNames.MiniAppId; // "Genora.Zalo.MiniAppId"

    /// <summary>Private Key HMAC-SHA256 do Zalo cấp (lưu encrypted)</summary>
    public const string PrivateKey = "Genora.Payment.Zalo.PrivateKey";

    // ── Bank Transfer Config ────────────────────────────────────────────────
    public const string BankName          = "Genora.Payment.Bank.BankName";
    public const string BankAccountNumber = "Genora.Payment.Bank.AccountNumber";
    public const string BankAccountOwner  = "Genora.Payment.Bank.AccountOwner";
    public const string BankBranch        = "Genora.Payment.Bank.Branch";

    // ── Payment Method Toggles ──────────────────────────────────────────────
    /// <summary>
    /// Bật/tắt hình thức Thanh toán tại quầy. Default = true (bật).
    /// Mini App đọc cờ này để ẩn/hiện option "Thanh toán tại quầy".
    /// </summary>
    public const string IsPayAtCounterEnabled = "Genora.Payment.IsPayAtCounterEnabled";

    /// <summary>
    /// Bật/tắt hình thức Thanh toán chuyển khoản. Default = true (bật).
    /// Mini App đọc cờ này để ẩn/hiện option "Thanh toán chuyển khoản".
    /// </summary>
    public const string IsPayBankTransferEnabled = "Genora.Payment.IsPayBankTransferEnabled";
}
