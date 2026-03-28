using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.Realtime;
public class FnbOrderRealtimeDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = default!;
    public string BagTag { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneMasked { get; set; }

    public string? CustomerTypeName { get; set; }
    public string? CustomerTypeColorCode { get; set; }

    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? CancelledAt { get; set; }

    public int ServiceStatus { get; set; }
    public int PaymentStatus { get; set; }

    public string? PrimaryImageUrl { get; set; }
    public string ItemsSummary { get; set; } = default!;
    public string ItemNotesSummary { get; set; } = default!;
    public int TotalQuantity { get; set; }

    public string? LatestActivityTitle { get; set; }
    public string? LatestActivityDescription { get; set; }

    public List<FnbOrderRealtimeActivityDto> RecentActivities { get; set; } = new();
    public List<FnbOrderRealtimeItemDto> Items { get; set; } = new();
}

public class FnbOrderRealtimeItemDto
{
    public Guid? ItemId { get; set; }
    public string ItemName { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
}

public class FnbOrderRealtimeActivityDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime Time { get; set; }
    public bool IsDanger { get; set; }
}