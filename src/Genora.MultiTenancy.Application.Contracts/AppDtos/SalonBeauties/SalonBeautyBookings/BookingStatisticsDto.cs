namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class BookingStatisticsDto
{
    public int TotalBookings { get; set; }
    public decimal TotalBookingsChangePercent { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalValueChangePercent { get; set; }
    public decimal CompletionRate { get; set; }
    public string CompletionTrendText { get; set; } = "Ổn định";
    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int ProcessingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int NewUnprocessedCount { get; set; }
}