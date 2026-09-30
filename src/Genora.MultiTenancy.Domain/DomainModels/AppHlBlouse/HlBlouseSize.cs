using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlBlouse;

/// <summary>
/// Danh mục size áo Blouse theo dáng (Nam/Nữ) + tồn kho.
/// Dùng cho dropdown chọn size ở Mini App và validate tồn kho khi đăng ký.
/// </summary>
[Table("AppHlBlouseSizes", Schema = "HL")]
public class HlBlouseSize : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Dáng áo: Nam / Nữ</summary>
    public HlBlouseStyle Style { get; set; }

    /// <summary>Mã size (M, L, XL, 2XL...)</summary>
    [Required]
    [StringLength(20)]
    public string SizeCode { get; set; } = null!;

    /// <summary>Mô tả cân nặng phù hợp (VD: "50-55kg")</summary>
    [StringLength(100)]
    public string? WeightRange { get; set; }

    /// <summary>Tồn kho hiện tại (số áo còn có thể đăng ký). Âm/0 = hết hàng.</summary>
    public int StockQuantity { get; set; }

    /// <summary>Thứ tự hiển thị</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Còn hiển thị/cho chọn hay không</summary>
    public bool IsActive { get; set; } = true;

    protected HlBlouseSize() { }

    public HlBlouseSize(Guid id, HlBlouseStyle style, string sizeCode, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        Style = style;
        SizeCode = sizeCode;
    }
}
