using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Hồ sơ người chơi Gamification. Tái dùng dbo.AppCustomers (zalo/phone/code) qua CustomerId,
/// bổ sung field đặc thù game: CustomerType, IsRegistered, và điểm hiển thị (balance thực nằm ở Customer.BonusPoint).
/// Schema: HLG.
/// </summary>
[Table("AppHlgUserProfiles", Schema = "HLG")]
public class HlgUserProfile : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Liên kết tới dbo.AppCustomers (nguồn zalo/phone/code/points).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Zalo user id (snapshot để query nhanh, nguồn gốc ở Customer.ZaloUserId).</summary>
    [StringLength(100)]
    public string? ZaloId { get; set; }

    /// <summary>Loại khách hàng — quyết định luồng nhận quà (pharmacy | consumer).</summary>
    public HlgCustomerType? CustomerType { get; set; }

    /// <summary>Đã hoàn tất đăng ký hồ sơ gamification chưa.</summary>
    public bool IsRegistered { get; set; }

    protected HlgUserProfile() { }

    public HlgUserProfile(Guid id, Guid customerId, Guid? tenantId = null) : base(id)
    {
        CustomerId = customerId;
        TenantId = tenantId;
    }
}
