using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;

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
    public decimal TotalSpent { get; set; }
    public int TotalBooking { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int LoyaltyPoint { get; set; }
    public DateTime? LastBookingDate { get; set; }
}
