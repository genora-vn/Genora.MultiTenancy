using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class CreateUpdateBookingPlayerDto
{
    public Guid? CustomerId { get; set; }
    [Required]
    [StringLength(200)]
    public string PlayerName { get; set; }
    [StringLength(500)]
    public string Notes { get; set; }
}

public class CreateUpdateAppBookingDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public DateTime PlayDate { get; set; }

    [Required]
    public Guid GolfCourseId { get; set; }

    [Required]
    public Guid? CalendarSlotId { get; set; }

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

    [StringLength(500)]
    public string Notes { get; set; }

    public List<CreateUpdateBookingPlayerDto> Players { get; set; } = new();
}