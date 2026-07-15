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
/// Chi nhánh/điểm áp dụng của quà (field "office[]" — chỉ có khi field=office).
/// </summary>
public class UrBoxOfficeDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("brand_id")]
    public string? BrandId { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("address_en")]
    public string? AddressEn { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("city_id")]
    public string? CityId { get; set; }

    [JsonPropertyName("district_id")]
    public string? DistrictId { get; set; }

    [JsonPropertyName("ward_id")]
    public string? WardId { get; set; }

    [JsonPropertyName("street_id")]
    public string? StreetId { get; set; }

    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }

    [JsonPropertyName("isApply")]
    public string? IsApply { get; set; }

    [JsonPropertyName("title_city")]
    public string? TitleCity { get; set; }

    [JsonPropertyName("brand_title")]
    public string? BrandTitle { get; set; }

    [JsonPropertyName("brand_img_src")]
    public string? BrandImgSrc { get; set; }
}

/// <summary>
/// Item trong danh sách quà (API /4.0/gift/lists → data.items[])
/// </summary>
public class UrBoxGiftItemDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("brand_id")]
    public string? BrandId { get; set; }

    [JsonPropertyName("brand_name")]
    public string? BrandName { get; set; }

    [JsonPropertyName("brand_online")]
    public string? BrandOnline { get; set; }

    [JsonPropertyName("gift_id")]
    public string? GiftId { get; set; }

    [JsonPropertyName("cat_id")]
    public string? CatId { get; set; }

    [JsonPropertyName("cat_title")]
    public string? CatTitle { get; set; }

    [JsonPropertyName("parent_cat_id")]
    public string? ParentCatId { get; set; }

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

    [JsonPropertyName("view")]
    public string? View { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("code_quantity")]
    public string? CodeQuantity { get; set; }

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

    [JsonPropertyName("code_display")]
    public string? CodeDisplay { get; set; }

    [JsonPropertyName("code_display_type")]
    public int? CodeDisplayType { get; set; }

    [JsonPropertyName("price_promo")]
    public decimal? PricePromo { get; set; }

    [JsonPropertyName("start_promo")]
    public long? StartPromo { get; set; }

    [JsonPropertyName("end_promo")]
    public long? EndPromo { get; set; }

    [JsonPropertyName("is_promo")]
    public int? IsPromo { get; set; }

    [JsonPropertyName("is_unfix")]
    public string? IsUnfix { get; set; }

    [JsonPropertyName("usage_check")]
    public int? UsageCheck { get; set; }

    [JsonPropertyName("brandImage")]
    public string? BrandImage { get; set; }

    [JsonPropertyName("brandLogoLoyalty")]
    public string? BrandLogoLoyalty { get; set; }

    [JsonPropertyName("office")]
    public List<UrBoxOfficeDto>? Office { get; set; }

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

    [JsonPropertyName("brand_online")]
    public string? BrandOnline { get; set; }

    [JsonPropertyName("cat_id")]
    public string? CatId { get; set; }

    [JsonPropertyName("parent_cat_id")]
    public string? ParentCatId { get; set; }

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

    [JsonPropertyName("weight")]
    public string? Weight { get; set; }

    [JsonPropertyName("justGetOrder")]
    public string? JustGetOrder { get; set; }

    [JsonPropertyName("view")]
    public string? View { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("usage_check")]
    public int? UsageCheck { get; set; }

    [JsonPropertyName("code_display")]
    public string? CodeDisplay { get; set; }

    [JsonPropertyName("code_display_type")]
    public int? CodeDisplayType { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("images")]
    public Dictionary<string, string>? Images { get; set; }

    [JsonPropertyName("images_rectangle")]
    public Dictionary<string, string>? ImagesRectangle { get; set; }

    [JsonPropertyName("expire_duration")]
    public string? ExpireDuration { get; set; }

    [JsonPropertyName("price_promo")]
    public decimal? PricePromo { get; set; }

    [JsonPropertyName("start_promo")]
    public long? StartPromo { get; set; }

    [JsonPropertyName("end_promo")]
    public long? EndPromo { get; set; }

    [JsonPropertyName("is_promo")]
    public int? IsPromo { get; set; }

    [JsonPropertyName("is_unfix")]
    public string? IsUnfix { get; set; }

    [JsonPropertyName("brandImage")]
    public string? BrandImage { get; set; }

    [JsonPropertyName("office")]
    public List<UrBoxOfficeDto>? Office { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}
