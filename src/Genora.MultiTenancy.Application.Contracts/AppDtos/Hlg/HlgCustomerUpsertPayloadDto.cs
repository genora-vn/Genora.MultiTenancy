namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Payload đăng ký/đồng bộ khách hàng Gamification (gọi sau decode-phone).
/// customerType được gán tại đây khi register (quyết định luồng nhận quà).
/// </summary>
public class HlgCustomerUpsertPayloadDto
{
    public string Phone { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ZaloUserId { get; set; }
    public string? AvatarUrl { get; set; }
    public bool? IsFollower { get; set; }

    /// <summary>"pharmacy" | "consumer" — gán khi register.</summary>
    public string? CustomerType { get; set; }

    public string? Gender { get; set; }
    public string? Birthday { get; set; }
    public string? Address { get; set; }
}
