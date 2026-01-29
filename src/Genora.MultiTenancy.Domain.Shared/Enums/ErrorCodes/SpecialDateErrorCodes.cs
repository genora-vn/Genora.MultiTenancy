namespace Genora.MultiTenancy.Enums.ErrorCodes;
public static class SpecialDateErrorCodes
{
    public const string Prefix = "SpecialDate:";

    public const string InvalidInput = Prefix + "InvalidInput";
    public const string NameInvalid = Prefix + "NameInvalid";
    public const string HolidayDatesRequired = Prefix + "HolidayDatesRequired";
}
