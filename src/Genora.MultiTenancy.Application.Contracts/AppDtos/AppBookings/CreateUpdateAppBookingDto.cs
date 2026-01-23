using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class CreateUpdateBookingPlayerDto
{
    public Guid? CustomerId { get; set; }
    [Required]
    [StringLength(200)]
    public string PlayerName { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    public decimal? PricePerPlayer { get; set; }
    public string? VgaCode { get; set; }
}

public class CreateUpdateAppBookingDto
{
    [Required]
    public Guid CustomerId { get; set; }
    public string? CustomerType { get; set; }

    [Required]
    public DateTime PlayDate { get; set; }

    [Required]
    public Guid GolfCourseId { get; set; }

    [Required]
    public Guid? CalendarSlotId { get; set; }

    public TimeSpan? TimeFrom { get; set; }
    public TimeSpan? TimeTo { get; set; }

    [Range(1, 100)]
    public int NumberOfGolfers { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PricePerGolfer { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Required]
    public PaymentMethod? PaymentMethod { get; set; }

    [Required]
    public BookingStatus Status { get; set; }

    [Required]
    public BookingSource Source { get; set; }
    public short? NumberHoles { get; set; }
    public string? Utilities { get; set; }
    public bool IsExportInvoice { get; set; }
    public List<CreateUpdateBookingPlayerDto>? Players { get; set; } = new();
    public List<GolfCourseUtilityDto>? UtilityDto => string.IsNullOrEmpty(Utilities) ? new List<GolfCourseUtilityDto>() : Utilities.Split(",").Select(u => new GolfCourseUtilityDto
    {
        UtilityId = int.Parse(u),
        UtilityName = Enums.UlititiesEnum.From(int.Parse(u)).Name ?? string.Empty,
    }).ToList();
}