using System;
using System.Collections.Generic;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO hiển thị đơn hàng Genora (tạo từ Mini App)
/// </summary>
public class HlOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public decimal SubTotal { get; set; }
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SystemDiscount { get; set; }
    public decimal TotalAmount { get; set; }
    public HlOrderDeliveryStatus DeliveryStatus { get; set; }
    public HlOrderPaymentStatus PaymentStatus { get; set; }
    public HlPaymentMethod? PaymentMethod { get; set; }
    public string? Note { get; set; }
    public string? InternalNote { get; set; }
    public string? CancelNote { get; set; }
    public string? ExternalOrderCode { get; set; }
    public bool IsSyncedToHl { get; set; }
    public DateTime? SyncedAt { get; set; }
    public DateTime CreationTime { get; set; }
    public List<HlOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO chi tiết sản phẩm trong đơn hàng Genora
/// </summary>
public class HlOrderItemDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? ProductGroupName { get; set; }
    public string? BrandName { get; set; }
    public string? ProductUnit { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// DTO cập nhật trạng thái đơn hàng
/// </summary>
public class HlOrderUpdateStatusDto
{
    public Guid Id { get; set; }
    public HlOrderDeliveryStatus? DeliveryStatus { get; set; }
    public HlOrderPaymentStatus? PaymentStatus { get; set; }
    public string? InternalNote { get; set; }
}

/// <summary>
/// DTO hủy đơn hàng
/// </summary>
public class HlOrderCancelDto
{
    public Guid Id { get; set; }
    public string? CancelNote { get; set; }
}

/// <summary>
/// Input filter cho danh sách đơn hàng Genora
/// </summary>
public class HlOrderFilterDto
{
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 20;
    public string? Filter { get; set; }
    public HlOrderDeliveryStatus? DeliveryStatus { get; set; }
    public HlOrderPaymentStatus? PaymentStatus { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
