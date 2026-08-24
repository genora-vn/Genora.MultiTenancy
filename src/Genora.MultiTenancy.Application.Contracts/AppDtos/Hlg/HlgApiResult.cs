using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Response envelope chuẩn cho Hoa Linh Gamification Mini App.
/// Khớp CHÍNH XÁC contract frontend: { error?: number, message?: string, data: <payload> }.
/// KHÔNG thêm field thừa (không có "success"). Serialize camelCase mặc định của ABP.
/// </summary>
public class HlgApiResult<T>
{
    /// <summary>Mã lỗi. Bỏ qua khi thành công (null → không serialize).</summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Error { get; set; }

    /// <summary>Thông điệp kèm theo (tùy chọn).</summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    /// <summary>Payload dữ liệu.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public static HlgApiResult<T> Ok(T data, string? message = null)
        => new() { Data = data, Message = message };

    public static HlgApiResult<T> Fail(int error, string? message = null)
        => new() { Error = error, Message = message };
}
