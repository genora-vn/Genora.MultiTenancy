using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlBlouse;

/// <summary>
/// Chi tiết một dòng áo trong đơn đăng ký nhận áo Blouse (payload động).
/// Mỗi dòng = 1 dáng + 1 size + số lượng, thuộc loại Tặng miễn phí hoặc Đổi bằng điểm.
/// </summary>
[Table("AppHlBlouseRegistrationItems", Schema = "HL")]
public class HlBlouseRegistrationItem : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    public Guid RegistrationId { get; set; }
    public virtual HlBlouseRegistration Registration { get; set; } = null!;

    /// <summary>Loại dòng: Tặng miễn phí / Đổi bằng điểm</summary>
    public HlBlouseItemType ItemType { get; set; }

    /// <summary>Dáng áo: Nam / Nữ</summary>
    public HlBlouseStyle Style { get; set; }

    /// <summary>Id size (tham chiếu HlBlouseSize) — nullable để không chặn khi size bị xóa</summary>
    public Guid? SizeId { get; set; }

    /// <summary>Mã size snapshot (M, L, XL, 2XL...)</summary>
    [Required]
    [StringLength(20)]
    public string SizeCode { get; set; } = null!;

    /// <summary>Mô tả cân nặng snapshot (VD: "50-55kg")</summary>
    [StringLength(100)]
    public string? WeightRange { get; set; }

    /// <summary>Số lượng áo của dòng này</summary>
    public int Quantity { get; set; }

    /// <summary>Số điểm cần cho 1 áo (0 với áo tặng miễn phí)</summary>
    public int PointsPerItem { get; set; }

    /// <summary>Tổng điểm dòng = PointsPerItem * Quantity (0 với áo tặng)</summary>
    public int TotalPoints { get; set; }

    protected HlBlouseRegistrationItem() { }

    public HlBlouseRegistrationItem(
        Guid id,
        Guid registrationId,
        HlBlouseItemType itemType,
        HlBlouseStyle style,
        string sizeCode,
        int quantity,
        int pointsPerItem = 0,
        Guid? tenantId = null) : base(id)
    {
        RegistrationId = registrationId;
        ItemType = itemType;
        Style = style;
        SizeCode = sizeCode;
        Quantity = quantity;
        PointsPerItem = pointsPerItem;
        TotalPoints = itemType == HlBlouseItemType.Exchange ? pointsPerItem * quantity : 0;
        TenantId = tenantId;
    }
}
