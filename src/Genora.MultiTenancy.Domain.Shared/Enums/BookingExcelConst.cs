namespace Genora.MultiTenancy.Enums;
public static class BookingExcelConst
{
    public static string PaymentMethodText(PaymentMethod? pm) => pm switch
    {
        PaymentMethod.COD => "COD",
        PaymentMethod.Online => "Online",
        PaymentMethod.BankTransfer => "BankTransfer",
        _ => ""
    };

    public static string StatusText(BookingStatus status) => status switch
    {
        BookingStatus.Processing => "Pending",
        BookingStatus.Confirmed => "Confirmed",
        BookingStatus.Paid => "Paid",
        BookingStatus.Completed => "Completed",
        BookingStatus.CancelledRefund => "CancelledRefund",
        BookingStatus.CancelledNoRefund => "CancelledNoRefund",
        _ => ""
    };

    public static string SourceText(BookingSource source) => source switch
    {
        BookingSource.MiniApp => "MiniApp",
        BookingSource.Hotline => "Hotline",
        BookingSource.Agent => "Agent",
        _ => ""
    };
}