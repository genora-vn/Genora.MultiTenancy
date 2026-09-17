using System;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Người dùng Gamification. Khớp contract frontend GamificationUser.
/// gender/customerType để kiểu string để khớp chính xác "male|female|other", "pharmacy|consumer".
/// </summary>
public class GamificationUserDto
{
    public Guid Id { get; set; }
    public string? ZaloId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? Birthday { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CustomerType { get; set; }
    public int Points { get; set; }
    public bool IsRegistered { get; set; }
    public DateTime CreatedAt { get; set; }
}
