using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.UrBox;

/// <summary>
/// Thương hiệu UrBox (API /4.0/gift/brand → data.items[])
/// </summary>
public class UrBoxBrandDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("cat_id")]
    public string? CatId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("cat_title")]
    public string? CatTitle { get; set; }

    [JsonPropertyName("parent_cat_id")]
    public string? ParentCatId { get; set; }

    [JsonPropertyName("images")]
    public string? Images { get; set; }

    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("gift_count")]
    public int GiftCount { get; set; }
}

/// <summary>
/// Danh mục UrBox (API /2.0/category/catbyparent → data[])
/// </summary>
public class UrBoxCategoryDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("images")]
    public string? Images { get; set; }
}

/// <summary>
/// Item trong danh sách quà (API /4.0/gift/lists → data.items[])
/// </summary>
public class UrBoxGiftItemDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("gift_id")]
    public string? GiftId { get; set; }

    [JsonPropertyName("brand_id")]
    public string? BrandId { get; set; }

    [JsonPropertyName("brand_name")]
    public string? BrandName { get; set; }

    [JsonPropertyName("cat_id")]
    public string? CatId { get; set; }

    [JsonPropertyName("cat_title")]
    public string? CatTitle { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("valuex")]
    public string? Valuex { get; set; }

    [JsonPropertyName("point")]
    public string? Point { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("images")]
    public Dictionary<string, string>? Images { get; set; }

    [JsonPropertyName("images_rectangle")]
    public Dictionary<string, string>? ImagesRectangle { get; set; }

    [JsonPropertyName("expire_duration")]
    public string? ExpireDuration { get; set; }

    [JsonPropertyName("brandImage")]
    public string? BrandImage { get; set; }

    [JsonPropertyName("brandLogoLoyalty")]
    public string? BrandLogoLoyalty { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}

/// <summary>
/// Chi tiết 1 quà tặng (API /4.0/gift/detail → data{})
/// </summary>
public class UrBoxGiftDetailDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("gift_id")]
    public string? GiftId { get; set; }

    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("brand_id")]
    public string? BrandId { get; set; }

    [JsonPropertyName("cat_id")]
    public string? CatId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("point")]
    public string? Point { get; set; }

    [JsonPropertyName("valuex")]
    public string? Valuex { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("images")]
    public Dictionary<string, string>? Images { get; set; }

    [JsonPropertyName("images_rectangle")]
    public Dictionary<string, string>? ImagesRectangle { get; set; }

    [JsonPropertyName("expire_duration")]
    public string? ExpireDuration { get; set; }

    [JsonPropertyName("brandImage")]
    public string? BrandImage { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}
