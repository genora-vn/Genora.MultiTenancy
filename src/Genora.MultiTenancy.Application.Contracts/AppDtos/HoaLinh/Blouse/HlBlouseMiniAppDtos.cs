using System;
using System.Collections.Generic;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.HoaLinh.Blouse;

// ============================================================================
// MINI APP — Cấu hình & danh mục size (đọc để render form)
// ============================================================================

/// <summary>
/// GET /api/mini-app/hl/blouse/config
/// Cấu hình chương trình + danh mục size (nhóm theo dáng) cho Mini App render form.
/// </summary>
public class HlBlouseConfigDto
{
    public bool IsActive { get; set; }
    public string? ProgramName { get; set; }
    public string? IntroductionHtml { get; set; }

    /// <summary>Số áo tặng miễn phí tối đa (theo suất GKHL)</summary>
    public int FreeShirtLimit { get; set; }

    /// <summary>Số điểm đổi 1 áo thêu tên</summary>
    public int PointsPerShirt { get; set; }

    /// <summary>Số áo đổi tối đa (0 = không giới hạn)</summary>
    public int MaxExchangeShirt { get; set; }

    /// <summary>Ảnh bảng size</summary>
    public string? SizeChartImageUrl { get; set; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    /// <summary>Danh mục size dáng nam</summary>
    public List<HlBlouseSizeDto> MaleSizes { get; set; } = new();

    /// <summary>Danh mục size dáng nữ</summary>
    public List<HlBlouseSizeDto> FemaleSizes { get; set; } = new();
}

/// <summary>DTO 1 size áo (dùng cho dropdown Mini App + Admin)</summary>
public class HlBlouseSizeDto
{
    public Guid Id { get; set; }
    public HlBlouseStyle Style { get; set; }
    public string SizeCode { get; set; } = null!;
    public string? WeightRange { get; set; }
    public int StockQuantity { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Còn hàng để chọn hay không (StockQuantity > 0 và IsActive)</summary>
    public bool InStock { get; set; }
}

// ============================================================================
// MINI APP — Đăng ký (payload động)
// ============================================================================

/// <summary>
/// POST /api/mini-app/hl/blouse/register
/// Payload động: danh sách dòng áo tặng miễn phí + áo đổi điểm mà user chọn.
/// </summary>
public class HlBlouseRegisterRequest
{
    // ===== Thông tin người đăng ký (auto-fill, FE gửi lại) =====
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }

    /// <summary>SĐT — bắt buộc để định danh + đối soát điểm.</summary>
    public string? CustomerPhone { get; set; }

    public string? ZaloUserId { get; set; }

    /// <summary>Đại diện tiếp nhận</summary>
    public string? ReceiverName { get; set; }

    /// <summary>Địa chỉ giao nhận quà</summary>
    public string? DeliveryAddress { get; set; }

    // ===== Thông tin hiển thị trên áo =====
    public HlBlouseBusinessType? BusinessType { get; set; }
    public string? BusinessTypeName { get; set; }
    public string? StoreName { get; set; }
    public string? PrintedName { get; set; }

    /// <summary>Ghi chú thêm</summary>
    public string? Note { get; set; }

    /// <summary>Danh sách dòng áo (động) — cả áo tặng và áo đổi điểm.</summary>
    public List<HlBlouseRegisterItemDto> Items { get; set; } = new();
}

/// <summary>Một dòng áo trong payload đăng ký (động)</summary>
public class HlBlouseRegisterItemDto
{
    /// <summary>Loại: Free (tặng) / Exchange (đổi điểm)</summary>
    public HlBlouseItemType ItemType { get; set; }

    /// <summary>Dáng: Male / Female</summary>
    public HlBlouseStyle Style { get; set; }

    /// <summary>Id size (ưu tiên). Nếu null sẽ tra theo Style + SizeCode.</summary>
    public Guid? SizeId { get; set; }

    /// <summary>Mã size (M/L/XL/2XL) — dùng khi không gửi SizeId.</summary>
    public string? SizeCode { get; set; }

    /// <summary>Số lượng (>0)</summary>
    public int Quantity { get; set; }
}
