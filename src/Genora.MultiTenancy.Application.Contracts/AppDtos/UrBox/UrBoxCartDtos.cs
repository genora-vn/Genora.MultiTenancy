using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.UrBox;

/// <summary>
/// Cart trong danh sách lịch sử đổi quà theo user (API /2.0/cart/getlist → data[])
/// </summary>
public class UrBoxCartDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("linkCart")]
    public string? LinkCart { get; set; }

    [JsonPropertyName("campaign_code")]
    public string? CampaignCode { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("created_timestamp")]
    public string? CreatedTimestamp { get; set; }

    [JsonPropertyName("pay_time")]
    public string? PayTime { get; set; }

    [JsonPropertyName("pay_status")]
    public string? PayStatus { get; set; }

    [JsonPropertyName("pay_status_code")]
    public int PayStatusCode { get; set; }

    [JsonPropertyName("item_quantity")]
    public int ItemQuantity { get; set; }

    [JsonPropertyName("detail")]
    public List<UrBoxCartItemDto> Detail { get; set; } = new();
}

/// <summary>
/// Chi tiết quà trong 1 cart (field "detail[]")
/// </summary>
public class UrBoxCartItemDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("gift_id")]
    public string? GiftId { get; set; }

    [JsonPropertyName("gift_detail_id")]
    public string? GiftDetailId { get; set; }

    [JsonPropertyName("gift_title")]
    public string? GiftTitle { get; set; }

    [JsonPropertyName("gift_detail_title")]
    public string? GiftDetailTitle { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("serial")]
    public string? Serial { get; set; }

    [JsonPropertyName("code_image")]
    public string? CodeImage { get; set; }

    [JsonPropertyName("code_display")]
    public string? CodeDisplay { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("valuex")]
    public string? Valuex { get; set; }

    [JsonPropertyName("expired")]
    public string? Expired { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("brandId")]
    public string? BrandId { get; set; }

    [JsonPropertyName("brandTitle")]
    public string? BrandTitle { get; set; }

    [JsonPropertyName("brandImage")]
    public string? BrandImage { get; set; }
}

/// <summary>
/// Chi tiết đơn hàng theo transaction (API /2.0/cart/getByTransaction → data{})
/// </summary>
public class UrBoxCartByTransactionDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("campaign_code")]
    public string? CampaignCode { get; set; }

    [JsonPropertyName("linkCart")]
    public string? LinkCart { get; set; }

    [JsonPropertyName("money_ship")]
    public string? MoneyShip { get; set; }

    [JsonPropertyName("money_total")]
    public string? MoneyTotal { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("created_timestamp")]
    public string? CreatedTimestamp { get; set; }

    [JsonPropertyName("pay_time")]
    public string? PayTime { get; set; }

    [JsonPropertyName("pay_status")]
    public string? PayStatus { get; set; }

    [JsonPropertyName("pay_status_code")]
    public int PayStatusCode { get; set; }

    [JsonPropertyName("item_quantity")]
    public int ItemQuantity { get; set; }

    [JsonPropertyName("detail")]
    public List<UrBoxCartItemDto> Detail { get; set; } = new();
}
