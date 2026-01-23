namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class BookingChangeRequestEmailModelDto
{
    // Booking info
    public string BookingCode { get; set; } = "";
    public string BookerName { get; set; } = "";
    public string BookerPhone { get; set; } = "";

    // Before change
    public string OldStatusText { get; set; } = "";
    public string OldPaymentMethodText { get; set; } = "";
    public int OldNumberOfGolfers { get; set; }

    // Players before (only display if HasPlayerChanges)
    public string OldPlayersText { get; set; } = "";

    // After change
    public string NewStatusText { get; set; } = "";
    public string NewPaymentMethodText { get; set; } = "";
    public int NewNumberOfGolfers { get; set; }

    // Players after (only display if HasPlayerChanges)
    public string NewPlayersText { get; set; } = "";

    // Flags for template rendering
    public bool HasPlayerChanges { get; set; }
    public bool HasHeaderChanges { get; set; }
}