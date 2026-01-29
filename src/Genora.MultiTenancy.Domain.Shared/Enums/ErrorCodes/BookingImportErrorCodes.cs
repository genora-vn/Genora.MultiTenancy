namespace Genora.MultiTenancy.Enums.ErrorCodes;
public static class BookingImportErrorCodes
{
    public const string Prefix = "BookingImport:";

    public const string PlayDateInvalidFormat = Prefix + "PlayDateInvalidFormat";
    public const string NumberOfGolfersInvalid = Prefix + "NumberOfGolfersInvalid";
    public const string TotalAmountInvalid = Prefix + "TotalAmountInvalid";
    public const string PaymentMethodRequired = Prefix + "PaymentMethodRequired";
    public const string StatusRequired = Prefix + "StatusRequired";
    public const string SourceRequired = Prefix + "SourceRequired";

    public const string UnknownRowError = Prefix + "UnknownRowError";

    public const string PlayDateRequired = Prefix + "PlayDateRequired";
    public const string StatusInvalid = Prefix + "StatusInvalid";
    public const string PaymentMethodInvalid = Prefix + "PaymentMethodInvalid";
    public const string SourceInvalid = Prefix + "SourceInvalid";
}