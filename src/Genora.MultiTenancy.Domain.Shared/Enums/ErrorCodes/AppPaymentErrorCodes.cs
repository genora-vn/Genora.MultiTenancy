namespace Genora.MultiTenancy.Enums.ErrorCodes;

public static class AppPaymentErrorCodes
{
    public const string Prefix = "AppPayment:";

    public const string BookingNotFound         = Prefix + "BookingNotFound";
    public const string BookingAlreadyPaid      = Prefix + "BookingAlreadyPaid";
    public const string BookingCancelled        = Prefix + "BookingCancelled";
    public const string PaymentNotConfigured    = Prefix + "PaymentNotConfigured";
    public const string InvalidMac              = Prefix + "InvalidMac";
    public const string InvalidAmount           = Prefix + "InvalidAmount";
    public const string DuplicateTransaction    = Prefix + "DuplicateTransaction";
    public const string AppIdMismatch           = Prefix + "AppIdMismatch";
    public const string OrderNotFound           = Prefix + "OrderNotFound";
    public const string OrderAlreadyPaid = Prefix + "OrderAlreadyPaid";
}
