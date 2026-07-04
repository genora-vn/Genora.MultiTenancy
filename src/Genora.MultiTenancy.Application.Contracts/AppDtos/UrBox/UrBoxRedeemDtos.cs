using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.UrBox;

/// <summary>
/// Request từ Mini App gửi lên Genora để đổi quà eVoucher.
/// </summary>
public class UrBoxRedeemInput
{
    /// <summary>Mã khách hàng (UrBox: site_user_id) — định danh người đổi quà</summary>
    [Required]
    public string SiteUserId { get; set; } = null!;

    /// <summary>SĐT nhận mã quà (UrBox: ttphone)</summary>
    public string? Phone { get; set; }

    /// <summary>Tên khách hàng (lưu lịch sử đổi quà)</summary>
    public string? CustomerName { get; set; }

    /// <summary>Danh sách quà cần đổi (priceId + quantity)</summary>
    [Required]
    public List<UrBoxRedeemItem> Items { get; set; } = new();
}

public class UrBoxRedeemItem
{
    /// <summary>Mệnh giá quà (UrBox: priceId = id của item trong gift/lists)</summary>
    [Required]
    public string PriceId { get; set; } = null!;

    /// <summary>Số lượng</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Tên quà (lưu lịch sử — không gửi lên UrBox)</summary>
    public string? GiftName { get; set; }

    /// <summary>Ảnh quà (lưu lịch sử — không gửi lên UrBox)</summary>
    public string? GiftImageUrl { get; set; }

    /// <summary>Số điểm cho 1 quà (lưu lịch sử — không gửi lên UrBox)</summary>
    public int PointsRequired { get; set; }
}

// ── Payload gửi lên UrBox (khớp CURL cartPayVoucher) ─────────────────────────

/// <summary>
/// Body request gửi lên UrBox /2.0/cart/cartPayVoucher.
/// Dùng snake_case field name khớp API UrBox.
/// </summary>
public class UrBoxCartPayVoucherRequest
{
    [JsonPropertyName("app_secret")]
    public string? AppSecret { get; set; }

    [JsonPropertyName("app_id")]
    public string? AppId { get; set; }

    [JsonPropertyName("campaign_code")]
    public string? CampaignCode { get; set; }

    [JsonPropertyName("site_user_id")]
    public string? SiteUserId { get; set; }

    [JsonPropertyName("ttphone")]
    public string? Ttphone { get; set; }

    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("isSendSms")]
    public int IsSendSms { get; set; }

    [JsonPropertyName("dataBuy")]
    public List<UrBoxDataBuy> DataBuy { get; set; } = new();
}

/// <summary>
/// Payload để ký Signature — KHÔNG chứa ttphone (theo code tham khảo).
/// Các field sẽ được sort theo alphabet + compact JSON trước khi ký.
/// </summary>
public class UrBoxSignaturePayload
{
    [JsonPropertyName("app_id")]
    public string? AppId { get; set; }

    [JsonPropertyName("app_secret")]
    public string? AppSecret { get; set; }

    [JsonPropertyName("campaign_code")]
    public string? CampaignCode { get; set; }

    [JsonPropertyName("dataBuy")]
    public List<UrBoxDataBuy> DataBuy { get; set; } = new();

    [JsonPropertyName("isSendSms")]
    public int IsSendSms { get; set; }

    [JsonPropertyName("site_user_id")]
    public string? SiteUserId { get; set; }

    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }
}

public class UrBoxDataBuy
{
    [JsonPropertyName("priceId")]
    public string PriceId { get; set; } = null!;

    [JsonPropertyName("quantity")]
    public string Quantity { get; set; } = "1";
}

// ── Response từ UrBox cartPayVoucher ─────────────────────────────────────────

/// <summary>
/// data{} trong response cartPayVoucher (khi thành công).
/// </summary>
public class UrBoxRedeemData
{
    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("cart")]
    public UrBoxRedeemCart? Cart { get; set; }
}

public class UrBoxRedeemCart
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("code_link_gift")]
    public List<UrBoxCodeLinkGift>? CodeLinkGift { get; set; }
}

public class UrBoxCodeLinkGift
{
    [JsonPropertyName("cart_detail_id")]
    public string? CartDetailId { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("expired")]
    public string? Expired { get; set; }
}
