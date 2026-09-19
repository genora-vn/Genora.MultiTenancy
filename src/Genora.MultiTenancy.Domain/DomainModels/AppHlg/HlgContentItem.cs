using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Hlg;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
namespace Genora.MultiTenancy.DomainModels.AppHlg;
public class HlgContentItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public HlgContentSlot Slot { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = "";
    [StringLength(1000)] public string? Summary { get; set; }
    [StringLength(100)] public string? BadgeText { get; set; }
    [StringLength(1000)] public string? ImageUrl { get; set; }
    [StringLength(1000)] public string? TargetUrl { get; set; }
    public Guid? GameId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    protected HlgContentItem() { }
    public HlgContentItem(Guid id, Guid? tenantId) : base(id) { TenantId = tenantId; }
}
