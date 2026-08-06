using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class CaddieBookingNewRequestEmailModelDto
{
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string GolfCourseName { get; set; } = string.Empty;

    public string BookingDateText { get; set; } = string.Empty;
    public string StartTimeText { get; set; } = string.Empty;
    public int? NumberOfHoles { get; set; }

    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;

    public int PaymentStatus { get; set; }
    public string PaymentStatusText { get; set; } = string.Empty;
    public string PaymentMethodText { get; set; } = string.Empty;

    public int CheckinStatus { get; set; }
    public string CheckinStatusText { get; set; } = string.Empty;
    public string CheckinTimeText { get; set; } = string.Empty;

    public string? Note { get; set; }
    public string? CancelReason { get; set; }

    public string CreationTimeText { get; set; } = string.Empty;
    public string TotalCaddieFeeText { get; set; } = string.Empty;

    public List<CaddieEmailItemDto> Caddies { get; set; } = new List<CaddieEmailItemDto>();
}

public class CaddieEmailItemDto
{
    public string CaddieCode { get; set; } = string.Empty;
    public string CaddieName { get; set; } = string.Empty;
    public string GenderText { get; set; } = string.Empty;
    public string? Note { get; set; }
}