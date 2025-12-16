using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class AppBookingPlayerDto : EntityDto<Guid>
{
    public Guid BookingId { get; set; }
    public Guid? CustomerId { get; set; }
    public string PlayerName { get; set; }
    public string Notes { get; set; }
}

public class AppBookingDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string BookingCode { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }

    public Guid GolfCourseId { get; set; }
    public string GolfCourseName { get; set; }

    public Guid? CalendarSlotId { get; set; }

    public DateTime PlayDate { get; set; }

    public int NumberOfGolfers { get; set; }

    public decimal? PricePerGolfer { get; set; }
    public decimal TotalAmount { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }
    public BookingStatus Status { get; set; }
    public BookingSource Source { get; set; }

    public List<AppBookingPlayerDto> Players { get; set; } = new();
}