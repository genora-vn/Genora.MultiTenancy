namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Payload đăng ký/đồng bộ khách hàng Gamification (gọi sau decode-phone).
/// customerType được gán tại đây khi register (quyết định luồng nhận quà).
/// </summary>
public class HlgCustomerUpsertPayloadDto
{
    public string Phone { get; set; } = string.Empty;
    /// <summary>Owner phone. Omit only for owner registration (defaults to Phone).</summary>
    public string? PharmaPhone { get; set; }
    /// <summary>Selected DMS branch's custCode, validated again against DMS on registration.</summary>
    public string? CustomerCode { get; set; }
    public string? FullName { get; set; }
    public string? ZaloUserId { get; set; }
    public string? AvatarUrl { get; set; }
    public bool? IsFollower { get; set; }

    /// <summary>"pharmacy" | "consumer" — gán khi register.</summary>
    public string? CustomerType { get; set; }

    public string? Gender { get; set; }
    public string? Birthday { get; set; }
    public string? Address { get; set; }
    public string? VgaCode { get; set; } // Legacy alias; HLG pharmacy code is not Customer golf VGA.
    public string? PharmacyCode { get; set; }
}
