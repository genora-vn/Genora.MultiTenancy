using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class ProBoardItemDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = default!;
    public string BagTag { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public string? CustomerTypeName { get; set; }
    public string? CustomerTypeColorCode { get; set; }
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public DateTime CreationTime { get; set; }
    public ProServiceStatus ServiceStatus { get; set; }
    public ProPaymentStatus PaymentStatus { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string ItemsSummary { get; set; } = default!;
    public string ItemNotesSummary { get; set; } = default!;
    public string? LatestActivityTitle { get; set; }
    public string? LatestActivityDescription { get; set; }
    public List<string> ItemNames { get; set; } = new();
}
