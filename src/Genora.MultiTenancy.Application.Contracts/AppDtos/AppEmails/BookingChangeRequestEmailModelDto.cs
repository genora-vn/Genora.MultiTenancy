using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class BookingChangeRequestEmailModelDto
{
    public string BookingCode { get; set; } = "";
    public string BookerName { get; set; } = "";
    public string BookerPhone { get; set; } = "";

    public string GolfCourseName { get; set; } = "";
    public string GolfCourseHotline { get; set; } = "";
    public string GolfCourseAddress { get; set; } = "";

    public string OldStatusText { get; set; } = "";
    public string OldPaymentMethodText { get; set; } = "";
    public int OldNumberOfGolfers { get; set; }
    public string OldPlayDateText { get; set; } = "";
    public string OldTeeTimeFromText { get; set; } = "";
    public string OldTeeTimeToText { get; set; } = "";
    public string OldCustomerTypeText { get; set; } = "";
    public string OldPromotionText { get; set; } = "";
    public string OldPlayersText { get; set; } = "";
    public string OldUpdatedByText { get; set; } = "";

    public string NewStatusText { get; set; } = "";
    public string NewPaymentMethodText { get; set; } = "";
    public int NewNumberOfGolfers { get; set; }
    public string NewPlayDateText { get; set; } = "";
    public string NewTeeTimeFromText { get; set; } = "";
    public string NewTeeTimeToText { get; set; } = "";
    public string NewCustomerTypeText { get; set; } = "";
    public string NewPromotionText { get; set; } = "";
    public string NewPlayersText { get; set; } = "";
    public string NewUpdatedByText { get; set; } = "";

    public string PricePerGolferText { get; set; } = "";
    public string TotalAmountText { get; set; } = "";
    public string OtherRequestsText { get; set; } = "";
    public string InvoiceInfoText { get; set; } = "";

    public bool HasPlayerChanges { get; set; }
    public bool HasHeaderChanges { get; set; }

    public bool HasPriceBreakdownItems { get; set; }

    public List<BookingPriceBreakdownEmailItemDto> PriceBreakdownItems { get; set; } = new();
}