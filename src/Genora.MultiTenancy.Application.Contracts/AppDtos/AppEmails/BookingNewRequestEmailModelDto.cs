using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class BookingNewRequestEmailModelDto
{
    public string BookingCode { get; set; } = default!;
    public string BookerName { get; set; } = default!;
    public string BookerPhone { get; set; } = default!;

    public string GolfCourseName { get; set; } = "";
    public string GolfCourseHotline { get; set; } = "";
    public string GolfCourseAddress { get; set; } = "";

    public DateTime PlayDate { get; set; }
    public string PlayDateText { get; set; } = "";
    public string TeeTime { get; set; } = default!;
    public string TeeTimeFromText { get; set; } = "";
    public string TeeTimeToText { get; set; } = "";

    public int NumberOfGolfers { get; set; }
    public string CustomerTypeSummary { get; set; } = default!;

    public string PlayersText { get; set; } = "";
    public string PromotionText { get; set; } = "";
    public decimal PricePerGolfer { get; set; }
    public string PricePerGolferText { get; set; } = "";

    public decimal TotalAmount { get; set; }
    public string TotalAmountText { get; set; } = "";
    public string PaymentMethod { get; set; } = default!;
    public string OtherRequests { get; set; } = "";

    public bool IsExportInvoice { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyAddress { get; set; }
    public string? InvoiceEmail { get; set; }

    public bool HasPriceBreakdownItems { get; set; }
    public List<BookingPriceBreakdownEmailItemDto> PriceBreakdownItems { get; set; } = new();
}