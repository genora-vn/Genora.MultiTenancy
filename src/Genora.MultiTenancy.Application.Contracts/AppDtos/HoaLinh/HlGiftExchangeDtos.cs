using System;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO hiển thị yêu cầu đổi quà
/// </summary>
public class HlGiftExchangeDto
{
    public Guid Id { get; set; }
    public string ExchangeCode { get; set; } = null!;
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string GiftName { get; set; } = null!;
    public string? GiftCode { get; set; }
    public string? GiftImageUrl { get; set; }
    public int PointsRequired { get; set; }
    public int Quantity { get; set; }
    public int TotalPointsUsed { get; set; }
    public HlGiftExchangeStatus Status { get; set; }
    public string? Note { get; set; }
    public string? InternalNote { get; set; }
    public string? UrBoxVoucherCode { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Response gốc từ UrBox (JSON) — modal parse để hiển thị voucher/QR/hạn dùng/link nhận quà.</summary>
    public string? UrBoxResponse { get; set; }
}

/// <summary>
/// Input filter cho danh sách đổi quà
/// </summary>
public class HlGiftExchangeFilterDto
{
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 20;
    public string? Filter { get; set; }
    public HlGiftExchangeStatus? Status { get; set; }
}

/// <summary>
/// DTO duyệt/từ chối đổi quà
/// </summary>
public class HlGiftExchangeApproveDto
{
    public Guid Id { get; set; }
    public bool IsApproved { get; set; }
    public string? InternalNote { get; set; }
}
