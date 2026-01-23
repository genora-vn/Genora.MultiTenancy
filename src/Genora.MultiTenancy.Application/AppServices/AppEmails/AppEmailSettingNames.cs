namespace Genora.MultiTenancy.AppServices.AppEmails;
public static class AppEmailSettingNames
{
    // Booking New Request
    public const string BookingNew_ToEmails = "Genora.AppEmails.BookingNewRequest.ToEmails";
    public const string BookingNew_CcEmails = "Genora.AppEmails.BookingNewRequest.CcEmails";
    public const string BookingNew_BccEmails = "Genora.AppEmails.BookingNewRequest.BccEmails";
    public const string BookingNew_SubjectTemplate = "Genora.AppEmails.BookingNewRequest.SubjectTemplate";

    // ===== Booking Change Request =====
    public const string BookingChange_ToEmails = "Genora.AppEmails.BookingChangeRequest.ToEmails";
    public const string BookingChange_CcEmails = "Genora.AppEmails.BookingChangeRequest.CcEmails";
    public const string BookingChange_BccEmails = "Genora.AppEmails.BookingChangeRequest.BccEmails";
    public const string BookingChange_SubjectTemplate = "Genora.AppEmails.BookingChangeRequest.SubjectTemplate";

    // ===== Booking Cancel Request =====
    public const string BookingCancel_ToEmails = "Genora.AppEmails.BookingCancelRequest.ToEmails";
    public const string BookingCancel_CcEmails = "Genora.AppEmails.BookingCancelRequest.CcEmails";
    public const string BookingCancel_BccEmails = "Genora.AppEmails.BookingCancelRequest.BccEmails";
    public const string BookingCancel_SubjectTemplate = "Genora.AppEmails.BookingCancelRequest.SubjectTemplate";
}