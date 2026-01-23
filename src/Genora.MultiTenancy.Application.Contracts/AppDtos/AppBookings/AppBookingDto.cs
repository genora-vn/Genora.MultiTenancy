using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class AppBookingPlayerDto : EntityDto<Guid>
{
    public Guid BookingId { get; set; }
    public Guid? CustomerId { get; set; }
    public string PlayerName { get; set; }
    public string? Notes { get; set; }
    public decimal? PricePerPlayer { get; set; }
    public string? VgaCode { get; set; }
}
public class BookingFilter
{
    public string? filterText { get; set; }
    public int? status { get; set; }
    public int? source { get; set; }
}
public class AppBookingDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string BookingCode { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerType { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }

    public Guid GolfCourseId { get; set; }
    public string GolfCourseName { get; set; }

    public Guid? CalendarSlotId { get; set; }

    public DateTime PlayDate { get; set; }

    public TimeSpan? TimeFrom { get; set; }
    public TimeSpan? TimeTo { get; set; }

    public int NumberOfGolfers { get; set; }

    public decimal? PricePerGolfer { get; set; }
    public decimal TotalAmount { get; set; }
    public string? FrameTimes { get; set; }
    public short? NumberHoles { get; set; }
    public string? Utilities { get; set; }
    public bool IsExportInvoice { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public BookingStatus Status { get; set; }
    public BookingSource Source { get; set; }
    public string VNDayOfWeek { get; set; }
    public List<AppBookingPlayerDto> Players { get; set; } = new();

    public string? CompanyName { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyAddress { get; set; }
    public string? InvoiceEmail { get; set; }
    public List<GolfCourseUtilityDto>? UtilityDto => string.IsNullOrEmpty(Utilities) ? new List<GolfCourseUtilityDto>() : Utilities.Split(",").Select(u => new GolfCourseUtilityDto
    {
        UtilityId = int.Parse(u),
        UtilityName = Enums.UlititiesEnum.From(int.Parse(u)).Name ?? string.Empty,
        IsCheck = true,
    }).ToList();
}