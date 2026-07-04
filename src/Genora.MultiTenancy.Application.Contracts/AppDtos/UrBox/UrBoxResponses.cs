using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.UrBox;

/// <summary>
/// Wrapper base cho mọi response từ UrBox.
/// UrBox trả về: { "done": 1, "msg": "success", "status": 200, "data": {...} }
/// </summary>
public class UrBoxResponse<T>
{
    [JsonPropertyName("done")]
    public int Done { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("microtime")]
    public string? Microtime { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

/// <summary>
/// Container phân trang UrBox: { "items": [...], "totalPage": 1, "totalResult": "3" }
/// </summary>
public class UrBoxPagedData<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();

    [JsonPropertyName("totalPage")]
    public int TotalPage { get; set; }

    [JsonPropertyName("totalResult")]
    public string? TotalResult { get; set; }

    /// <summary>Chỉ có ở response brand list</summary>
    [JsonPropertyName("brand_count")]
    public string? BrandCount { get; set; }

    /// <summary>Chỉ có ở response brand list</summary>
    [JsonPropertyName("textTitle")]
    public string? TextTitle { get; set; }
}
