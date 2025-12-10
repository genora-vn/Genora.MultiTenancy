namespace Genora.MultiTenancy.Enums;

public enum BookingStatus : byte
{
    Processing = 0,
    Confirmed = 1,
    Paid = 2,
    Completed = 3,
    CancelledRefund = 4,
    CancelledNoRefund = 5
}