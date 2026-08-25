using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Cấu hình UI vòng quay may mắn (singleton theo tenant — đã chốt).
/// </summary>
[Table("AppHl25WheelConfig", Schema = "hl25")]
public class Hl25WheelConfig : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Tiêu đề (VD "Vòng quay may mắn").</summary>
    [StringLength(256)]
    public string? Title { get; set; }

    /// <summary>Phụ đề.</summary>
    [StringLength(512)]
    public string? SubTitle { get; set; }

    /// <summary>Màu chủ đạo (hex).</summary>
    [StringLength(16)]
    public string? PrimaryColor { get; set; }

    /// <summary>Màu phụ (hex).</summary>
    [StringLength(16)]
    public string? SecondaryColor { get; set; }

    /// <summary>Ảnh nền vòng quay.</summary>
    [StringLength(1024)]
    public string? BackgroundImageUrl { get; set; }

    /// <summary>Ảnh kim quay.</summary>
    [StringLength(1024)]
    public string? PointerImageUrl { get; set; }

    /// <summary>Số ô quay.</summary>
    public int SlotCount { get; set; }

    /// <summary>Bật/tắt.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Danh sách ô quay (dùng WithDetailsAsync khi cần).</summary>
    public virtual ICollection<Hl25WheelSlot> Slots { get; set; } = new List<Hl25WheelSlot>();

    protected Hl25WheelConfig() { }

    public Hl25WheelConfig(Guid id, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
    }
}
