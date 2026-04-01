using System;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class BookingCancelRequestEmailModelDto
{
    public string BookingCode { get; set; } = "";

    public string BookerName { get; set; } = "";
    public string BookerPhone { get; set; } = "";

    public string CancelRequesterName { get; set; } = "";
    public string CancelRequesterPhone { get; set; } = "";

    public string GolfCourseName { get; set; } = "";
    public string GolfCourseHotline { get; set; } = "";
    public string GolfCourseAddress { get; set; } = "";

    public DateTime PlayDate { get; set; }
    public string PlayDateText { get; set; } = "";

    public string TeeTimeFromText { get; set; } = "";
    public string TeeTimeToText { get; set; } = "";

    public int NumberOfGolfers { get; set; }
    public string PlayersText { get; set; } = "";

    public string CancelStatusText { get; set; } = "";
}