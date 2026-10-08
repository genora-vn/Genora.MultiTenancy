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
    public string? PharmaPhone { get; set; }
    /// <summary>Customer identity code in AppCustomers (HLGKH... for a newly registered employee).</summary>
    public string? CustomerCode { get; set; }
    /// <summary>Selected DMS branch; shared across members without violating AppCustomers uniqueness.</summary>
    public string? DmsCustomerCode { get; set; }
    public string? Gender { get; set; }
    public string? Birthday { get; set; }
    public string? Address { get; set; }
    /// <summary>Mã nhà thuốc hiển thị cho nhóm người dùng nhà thuốc.</summary>
    public string? VgaCode { get; set; } // Legacy alias; HLG pharmacy code is not Customer golf VGA.
    public string? PharmacyCode { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CustomerType { get; set; }
    public int Points { get; set; }
    public bool IsRegistered { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// True nếu truyền gameId và người chơi đã HOÀN THÀNH (thắng) game đó ở lần chơi trước.
    /// FE dùng cờ này để hiển thị modal "đã hoàn thành" ngay trước khi bấm "Chơi ngay".
    /// </summary>
    public bool AlreadyCompleted { get; set; }
    /// <summary>Thông báo hiển thị khi AlreadyCompleted = true (null nếu chưa hoàn thành).</summary>
    public string? AlreadyCompletedMessage { get; set; }
}
