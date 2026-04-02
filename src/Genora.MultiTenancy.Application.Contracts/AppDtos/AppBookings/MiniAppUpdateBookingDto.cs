using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppBookings;
public class MiniAppUpdateBookingDto
{
    [Required]
    public Guid CustomerId { get; set; }

    public DateTime PlayDate { get; set; }

    [Required]
    public Guid CalendarSlotId { get; set; }

    [Range(1, int.MaxValue)]
    public int NumberOfGolfers { get; set; }

    public List<CreateUpdateBookingPlayerDto>? Players { get; set; } = new();

    public decimal? PricePerGolfer { get; set; }

    public List<int>? Utilities { get; set; } = new();

    public short? NumberHoles { get; set; } = 18;

    public bool IsExportInvoice { get; set; }

    public string? CompanyName { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyAddress { get; set; }
    public string? InvoiceEmail { get; set; }
}