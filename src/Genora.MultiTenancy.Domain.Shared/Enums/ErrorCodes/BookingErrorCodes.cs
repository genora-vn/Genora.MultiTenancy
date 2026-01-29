namespace Genora.MultiTenancy.Enums.ErrorCodes;
public static class BookingErrorCodes
{
    public const string Prefix = "Booking:";

    public const string CalendarSlotRequired = Prefix + "CalendarSlotRequired";
    public const string BookingCancelledReadonly = Prefix + "BookingCancelledReadonly";
    public const string BookingNotFound = Prefix + "BookingNotFound";
}