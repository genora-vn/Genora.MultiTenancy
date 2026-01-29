namespace Genora.MultiTenancy.Enums.ErrorCodes;
public static class CalendarSlotErrorCodes
{
    public const string Prefix = "CalendarSlot:";

    public const string Overlap = Prefix + "Overlap";
    public const string MissingGolfCourse = Prefix + "MissingGolfCourse";
    public const string MaxSlotsInvalid = Prefix + "MaxSlotsInvalid";
    public const string Price18Required = Prefix + "Price18Required";
    public const string TimeRangeInvalid = Prefix + "TimeRangeInvalid";

    // ===== Import Excel =====
    public const string UnknownRowError = Prefix + "UnknownRowError";
    public const string BulkIdsRequired = Prefix + "BulkIdsRequired";
    public const string ImportCalendarSlotError = Prefix + "ImportCalendarSlotError";
    public const string ImportGolfCourseCodeRequired = Prefix + "ImportGolfCourseCodeRequired";
    public const string ImportFromDateRequired = Prefix + "ImportFromDateRequired";
    public const string ImportToDateRequired = Prefix + "ImportToDateRequired";
    public const string ImportStartTimeRequired = Prefix + "ImportStartTimeRequired";
    public const string ImportEndTimeRequired = Prefix + "ImportEndTimeRequired";
    public const string ImportMaxSlotsInvalid = Prefix + "ImportMaxSlotsInvalid";
    public const string ImportGapInvalid = Prefix + "ImportGapInvalid";
    public const string ImportPromotionTypeInvalid = Prefix + "ImportPromotionTypeInvalid";
    public const string ImportGolfCourseNotFound = Prefix + "ImportGolfCourseNotFound";

    public const string ImportDayTypeRequired = Prefix + "ImportDayTypeRequired";
    public const string ImportDayTypeInvalid = Prefix + "ImportDayTypeInvalid";
}