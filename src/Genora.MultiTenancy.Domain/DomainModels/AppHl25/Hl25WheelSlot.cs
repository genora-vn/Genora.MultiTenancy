using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Từng ô trên vòng quay + tỷ lệ trúng. Ràng buộc nghiệp vụ: tổng WinRate các ô = 100 (validate ở AppService).
/// </summary>
[Table("AppHl25WheelSlots", Schema = "hl25")]
public class Hl25WheelSlot : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>FK → Hl25WheelConfig.</summary>
    public Guid WheelConfigId { get; set; }

    /// <summary>FK → Hl25Gift (null = ô "Chúc may mắn").</summary>
    public Guid? GiftId { get; set; }

    /// <summary>Nhãn hiển thị trên ô.</summary>
    [StringLength(256)]
    public string? Label { get; set; }

    /// <summary>Ảnh quà trên ô.</summary>
    [StringLength(1024)]
    public string? SlotImageUrl { get; set; }

    /// <summary>Tỷ lệ trúng (%) của ô.</summary>
    public decimal WinRate { get; set; }

    /// <summary>Vị trí ô.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Màu ô (hex).</summary>
    [StringLength(16)]
    public string? ColorHex { get; set; }

    /// <summary>Navigation tới cấu hình vòng quay.</summary>
    public virtual Hl25WheelConfig? WheelConfig { get; set; }

    protected Hl25WheelSlot() { }

    public Hl25WheelSlot(Guid id, Guid wheelConfigId, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        WheelConfigId = wheelConfigId;
    }
}
