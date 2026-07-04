namespace Genora.MultiTenancy.AppDtos.UrBox;

/// <summary>
/// Cấu hình kết nối hệ thống UrBox — bind từ section "UrBoxSetting" trong appsettings.json.
/// </summary>
public class UrBoxSettings
{
    /// <summary>Tên section trong appsettings.json</summary>
    public const string SectionName = "UrBoxSetting";

    /// <summary>Base URL API UrBox (vd: https://sandapi.urbox.dev)</summary>
    public string UrBoxApiUrl { get; set; } = "https://sandapi.urbox.dev";

    /// <summary>App secret cấp bởi UrBox</summary>
    public string AppSecret { get; set; } = string.Empty;

    /// <summary>App id cấp bởi UrBox</summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>Mã chương trình (campaign) đổi quà</summary>
    public string CampaignCode { get; set; } = string.Empty;

    /// <summary>Có gửi SMS mã quà cho khách hàng không (1=có, 0=không)</summary>
    public int IsSendSms { get; set; } = 0;

    /// <summary>Số điểm tối thiểu để được đổi quà</summary>
    public int MinimumBonusPoint { get; set; } = 0;

    /// <summary>Tỉ lệ quy đổi: 1 điểm = ? đồng (giá UrBox tính theo đồng)</summary>
    public int BonusPointRate { get; set; } = 1;

    /// <summary>Đường dẫn file private key PEM để ký Signature (tương đối thư mục chạy app)</summary>
    public string PrivateKeyPath { get; set; } = "Keys/urbox_private_key.pem";

    /// <summary>Timeout (giây) cho mỗi request tới UrBox</summary>
    public int TimeoutSeconds { get; set; } = 30;

    // ── Đường dẫn API ────────────────────────────────────────────────────────
    public string GiftBrandPath { get; set; } = "/4.0/gift/brand";
    public string GiftListPath { get; set; } = "/4.0/gift/lists";
    public string GiftDetailPath { get; set; } = "/4.0/gift/detail";
    public string CategoryListPath { get; set; } = "/2.0/category/catbyparent";
    public string CartPayVoucherPath { get; set; } = "/2.0/cart/cartPayVoucher";
    public string CartListByUserPath { get; set; } = "/2.0/cart/getlist";
    public string CartByTransactionPath { get; set; } = "/2.0/cart/getByTransaction";
}
