using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Quà tặng trong kho (Kho quà) của vòng quay may mắn.
/// </summary>
[Table("AppHl25Gifts", Schema = "hl25")]
public class Hl25Gift : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Tên quà.</summary>
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    /// <summary>Hình ảnh quà.</summary>
    [StringLength(1024)]
    public string? ImageUrl { get; set; }

    /// <summary>Ảnh sản phẩm hiển thị trên bánh vòng quay; ImageUrl dùng cho thông tin quà trúng.</summary>
    [StringLength(1024)]
    public string? WheelImageUrl { get; set; }

    /// <summary>Mô tả (VD "Combo 04 sản phẩm…").</summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Tổng số lượng.</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Số lượng còn lại (giảm khi trao thưởng).</summary>
    public int RemainingQuantity { get; set; }

    /// <summary>Giá trị quà.</summary>
    public decimal? Value { get; set; }

    /// <summary>Trạng thái quà.</summary>
    public Hl25GiftStatus Status { get; set; } = Hl25GiftStatus.Available;

    protected Hl25Gift() { }

    public Hl25Gift(Guid id, string name, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        Name = name;
    }
}
