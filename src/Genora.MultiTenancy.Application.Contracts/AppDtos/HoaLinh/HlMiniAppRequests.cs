using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Request tạo đơn hàng từ Mini App
/// </summary>
public class HlCreateOrderRequest
{
    [Required]
    public string CustomerCode { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    [Required]
    public string BranchCode { get; set; } = null!;
    public string? BranchName { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }

    /// <summary>Mã trình dược viên (DSR) phụ trách — map dsrCode trên DMS. Nhận từ body param "receiveCode".</summary>
    public string? ReceiveCode { get; set; }

    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SystemDiscount { get; set; }

    /// <summary>1=Cash, 2=BankTransfer</summary>
    public int PaymentMethod { get; set; } = 1;

    public string? Note { get; set; }

    [Required]
    public List<HlCreateOrderItemRequest> Items { get; set; } = new();
}

public class HlCreateOrderItemRequest
{
    [Required]
    public string ProductCode { get; set; } = null!;
    [Required]
    public string ProductName { get; set; } = null!;
    public string? ProductGroupCode { get; set; }
    public string? ProductGroupName { get; set; }
    public string? BrandName { get; set; }
    public string? ProductUnit { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Request đổi quà từ Mini App
/// </summary>
public class HlCreateGiftExchangeRequest
{
    [Required]
    public string CustomerCode { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    [Required]
    public string GiftName { get; set; } = null!;
    public string? GiftCode { get; set; }
    public string? GiftImageUrl { get; set; }
    public int PointsRequired { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
    public string? DeliveryAddress { get; set; }
}

/// <summary>
/// Request đánh dấu quà đã sử dụng (mini app gọi). customerCode tùy chọn — nếu truyền sẽ guard đúng chủ quà.
/// </summary>
public class HlMarkGiftUsedRequest
{
    public string? CustomerCode { get; set; }
}
