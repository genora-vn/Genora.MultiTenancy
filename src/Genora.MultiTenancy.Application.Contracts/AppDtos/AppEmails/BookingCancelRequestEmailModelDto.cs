using System;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class BookingCancelRequestEmailModelDto
{
    public string BookingCode { get; set; } = "";

    // ✅ giữ nguyên khách hàng (booker)
    public string BookerName { get; set; } = "";
    public string BookerPhone { get; set; } = "";

    // ✅ người thao tác hủy trên admin
    public string CancelRequesterName { get; set; } = "";
    public string CancelRequesterPhone { get; set; } = "";

    public DateTime PlayDate { get; set; }
    public string PlayDateText { get; set; } = "";

    public string TeeTimeFromText { get; set; } = "";
    public string TeeTimeToText { get; set; } = "";

    public int NumberOfGolfers { get; set; }

    public string CancelStatusText { get; set; }
}