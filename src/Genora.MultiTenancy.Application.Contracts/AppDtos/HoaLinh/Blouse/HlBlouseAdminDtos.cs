using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.HoaLinh.Blouse;

// ============================================================================
// SHARED — Kết quả trả về sau khi đăng ký / xem chi tiết
// ============================================================================

/// <summary>DTO đơn đăng ký nhận áo Blouse (dùng cho cả Mini App trả về và Admin get)</summary>
public class HlBlouseRegistrationDto
{
    public Guid Id { get; set; }
    public string RegistrationCode { get; set; } = null!;

    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ZaloUserId { get; set; }
    public string? ReceiverName { get; set; }
    public string? DeliveryAddress { get; set; }

    public HlBlouseBusinessType? BusinessType { get; set; }
    public string? BusinessTypeName { get; set; }
    public string? StoreName { get; set; }
    public string? PrintedName { get; set; }
    public string? Note { get; set; }

    public int FreeQuantity { get; set; }
    public int ExchangeQuantity { get; set; }
    public int TotalQuantity { get; set; }
    public int TotalPointsUsed { get; set; }

    public HlBlouseRegistrationStatus Status { get; set; }
    public string? InternalNote { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public List<HlBlouseRegistrationItemResultDto> Items { get; set; } = new();
}

/// <summary>DTO 1 dòng áo trong đơn (trả về)</summary>
public class HlBlouseRegistrationItemResultDto
{
    public Guid Id { get; set; }
    public HlBlouseItemType ItemType { get; set; }
    public HlBlouseStyle Style { get; set; }
    public Guid? SizeId { get; set; }
    public string SizeCode { get; set; } = null!;
    public string? WeightRange { get; set; }
    public int Quantity { get; set; }
    public int PointsPerItem { get; set; }
    public int TotalPoints { get; set; }
}

// ============================================================================
// ADMIN — Filter & list
// ============================================================================

/// <summary>Input filter danh sách đơn đăng ký (Admin)</summary>
public class HlBlouseRegistrationFilterDto
{
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 20;

    /// <summary>Tìm theo mã đơn / mã KH / tên KH / SĐT / tên in trên áo</summary>
    public string? Filter { get; set; }

    public HlBlouseRegistrationStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

/// <summary>DTO cập nhật trạng thái / xử lý đơn (Admin)</summary>
public class HlBlouseUpdateStatusDto
{
    public Guid Id { get; set; }
    public HlBlouseRegistrationStatus Status { get; set; }
    public string? InternalNote { get; set; }
}

// ============================================================================
// ADMIN — Cấu hình chương trình (campaign) CRUD
// ============================================================================

public class HlBlouseCampaignDto
{
    public Guid Id { get; set; }
    public string ProgramName { get; set; } = null!;
    public string? IntroductionHtml { get; set; }
    public int FreeShirtLimit { get; set; }
    public int PointsPerShirt { get; set; }
    public int MaxExchangeShirt { get; set; }
    public string? SizeChartImageUrl { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; }
}

public class HlBlouseCampaignSaveDto
{
    [Required, StringLength(250)]
    public string ProgramName { get; set; } = null!;
    public string? IntroductionHtml { get; set; }
    [Range(0, int.MaxValue)]
    public int FreeShirtLimit { get; set; } = 2;
    [Range(0, int.MaxValue)]
    public int PointsPerShirt { get; set; } = 150;
    [Range(0, int.MaxValue)]
    public int MaxExchangeShirt { get; set; } = 0;
    [StringLength(500)]
    public string? SizeChartImageUrl { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}

// ============================================================================
// ADMIN — Danh mục size CRUD
// ============================================================================

public class HlBlouseSizeSaveDto
{
    public HlBlouseStyle Style { get; set; }
    public string SizeCode { get; set; } = null!;
    public string? WeightRange { get; set; }
    public int StockQuantity { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
