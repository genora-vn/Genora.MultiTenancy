using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;

public class AppCalendarSlotDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid GolfCourseId { get; set; }

    public string GolfCourseName { get; set; }

    public DateTime ApplyDate { get; set; }

    public TimeSpan TimeFrom { get; set; }

    public TimeSpan TimeTo { get; set; }

    public Guid PromotionTypeId { get; set; }
    public string? PromotionType { get; set; }

    public int MaxSlots { get; set; }

    public int SlotAvailable { get; set; }

    public string InternalNote { get; set; }

    public bool IsActive { get; set; }

    public List<AppCalendarSlotPriceDto> Prices { get; set; } = new();

    // Tính toán giá dựa trên số người chơi
    public decimal CustomerTypePrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal VisitorPrice { get; set; }
    public decimal DiscountPercent { get; set; }

    public string? CustomerTypeCode { get; set; }
    public string? OriginalPriceSource { get; set; }

    public bool IsMemberSupported { get; set; }
    public int? MaxMemberGuest { get; set; }

    /// <summary>Giá Member Guest theo số lỗ (khi khách hàng hiện tại là Member)</summary>
    public decimal? MemberGuestPrice { get; set; }

    /// <summary>Tổng tiền khách hàng phải trả dựa trên số người chơi</summary>
    public decimal CustomerBillTotalPrice { get; set; }

    /// <summary>Tổng tiền theo giá gốc dựa trên số người chơi</summary>
    public decimal OriginalBillTotalPrice { get; set; }

    /// <summary>Tổng tiền được chiết khấu = OriginalBillTotalPrice - CustomerBillTotalPrice</summary>
    public decimal DiscountTotalPrice { get; set; }
}