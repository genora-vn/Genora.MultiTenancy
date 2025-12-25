using System;
namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class AppBookingExcelRowDto
{
    public string BookingCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? GolfCourseName { get; set; }
    public DateTime PlayDate { get; set; }
    public int NumberOfGolfers { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
}