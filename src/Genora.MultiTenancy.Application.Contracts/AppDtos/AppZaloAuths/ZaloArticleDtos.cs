using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;

// ── Danh sách bài viết (GET /v2.0/article/getslice) ──────────────────────────

/// <summary>
/// Response danh sách bài viết Zalo OA. Kế thừa error/message chuẩn.
/// </summary>
public class ZaloArticleListResponse : ZaloBaseResponse
{
    [JsonPropertyName("data")]
    public ZaloArticleListData? Data { get; set; }
}

public class ZaloArticleListData
{
    [JsonPropertyName("medias")]
    public List<ZaloArticleItem> Medias { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

/// <summary>Bài viết rút gọn trong danh sách</summary>
public class ZaloArticleItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("total_view")]
    public long TotalView { get; set; }

    [JsonPropertyName("total_share")]
    public long TotalShare { get; set; }

    [JsonPropertyName("total_like")]
    public long TotalLike { get; set; }

    [JsonPropertyName("total_comment")]
    public long TotalComment { get; set; }

    [JsonPropertyName("create_date")]
    public long CreateDate { get; set; }

    [JsonPropertyName("update_date")]
    public long UpdateDate { get; set; }

    [JsonPropertyName("thumb")]
    public string? Thumb { get; set; }

    [JsonPropertyName("link_view")]
    public string? LinkView { get; set; }
}

// ── Chi tiết bài viết (GET /v2.0/article/getdetail) ──────────────────────────

/// <summary>
/// Response chi tiết 1 bài viết Zalo OA.
/// </summary>
public class ZaloArticleDetailResponse : ZaloBaseResponse
{
    [JsonPropertyName("data")]
    public ZaloArticleDetail? Data { get; set; }
}

public class ZaloArticleDetail
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("total_view")]
    public long TotalView { get; set; }

    [JsonPropertyName("total_share")]
    public long TotalShare { get; set; }

    [JsonPropertyName("total_like")]
    public long TotalLike { get; set; }

    [JsonPropertyName("total_comment")]
    public long TotalComment { get; set; }

    [JsonPropertyName("cover")]
    public ZaloArticleCover? Cover { get; set; }

    [JsonPropertyName("body")]
    public List<ZaloArticleBodyBlock> Body { get; set; } = new();

    [JsonPropertyName("related_medias")]
    public List<object> RelatedMedias { get; set; } = new();

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("link_view")]
    public string? LinkView { get; set; }
}

public class ZaloArticleCover
{
    [JsonPropertyName("cover_type")]
    public string? CoverType { get; set; }

    [JsonPropertyName("photo_url")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>Khối nội dung bài viết (type=text/image/...)</summary>
public class ZaloArticleBodyBlock
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
