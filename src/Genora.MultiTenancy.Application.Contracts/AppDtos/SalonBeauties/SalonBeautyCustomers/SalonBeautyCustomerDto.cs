using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

public class SalonBeautyCustomerDto : FullAuditedEntityDto<Guid>
{
    public string CustomerCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? PhoneMasked { get; set; }
    public string? Email { get; set; }
    public SalonBeautyGender? Gender { get; set; }
    public string? GenderText { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Avatar { get; set; }
    public string? ZaloUserId { get; set; }
    public bool IsFollowOa { get; set; }
    public SalonBeautyCustomerSource? Source { get; set; }
    public string? SourceText { get; set; }
    public byte Status { get; set; }
    public string? StatusText { get; set; }
    public string? Note { get; set; }

    public string MembershipLevel { get; set; } = "NEW";

    /// <summary>Tên hiển thị hạng (Mới / Thân thiết / Vàng / Kim cương).</summary>
    public string MembershipLevelLabel { get; set; } = "Mới";

    /// <summary>Ngưỡng chi tiêu cần đạt để lên hạng tiếp theo (VND). 0 nếu đã max hạng.</summary>
    public decimal NextTierThreshold { get; set; }

    /// <summary>Tên hạng tiếp theo, dùng cho label "Ngưỡng hạng VIP/Kim cương".</summary>
    public string? NextTierLabel { get; set; }

    public decimal TotalSpent { get; set; }
    public int TotalBooking { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int LoyaltyPoint { get; set; }
    public DateTime? LastBookingDate { get; set; }

    /// <summary>Tổng tiền đã nạp (VND, gồm các deposit Success).</summary>
    public decimal TotalDeposit { get; set; }

    /// <summary>Tổng tiền nạp tháng hiện tại (VND).</summary>
    public decimal MonthlyDepositCurrent { get; set; }

    /// <summary>Tỉ lệ thay đổi (%) so với tháng trước. Có thể âm.</summary>
    public decimal MonthlyDepositChangePercent { get; set; }

    /// <summary>Trung bình số ngày giữa 2 lần ghé thăm. Hiển thị dạng "TB N tuần/lần" hoặc "TB N tháng/lần".</summary>
    public string VisitFrequencyLabel { get; set; } = "Chưa có dữ liệu";
}

