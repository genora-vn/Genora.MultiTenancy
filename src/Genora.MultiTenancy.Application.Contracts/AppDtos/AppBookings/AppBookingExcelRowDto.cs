using System;
namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class AppBookingExcelRowDto
{
    public string? BookingCode { get; set; }
    public string? Customer { get; set; }
    public DateTime PlayDate { get; set; }
    public string? PlayTime { get; set; }
    public int NumberOfGolfers { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsExportInvoice { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyAddress { get; set; }
    public string? InvoiceEmail { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
}