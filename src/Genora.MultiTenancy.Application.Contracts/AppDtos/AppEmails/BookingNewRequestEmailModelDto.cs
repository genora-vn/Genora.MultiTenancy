using System;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class BookingNewRequestEmailModelDto
{
    public string BookingCode { get; set; } = default!;
    public string BookerName { get; set; } = default!;
    public string BookerPhone { get; set; } = default!;
    public DateTime PlayDate { get; set; }
    public string TeeTime { get; set; } = default!;
    public int NumberOfGolfers { get; set; }

    // format: "Member: 1 | Member Guest: 2 | Visitor: 1"
    public string CustomerTypeSummary { get; set; } = default!;

    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = default!;
    public string OtherRequests { get; set; } = "";

    public bool IsExportInvoice { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyAddress { get; set; }
    public string? InvoiceEmail { get; set; }
    public string PlayDateText { get; set; } = "";
    public string TeeTimeFromText { get; set; } = "";
    public string TeeTimeToText { get; set; } = "";
    public string TotalAmountText { get; set; } = "";
}