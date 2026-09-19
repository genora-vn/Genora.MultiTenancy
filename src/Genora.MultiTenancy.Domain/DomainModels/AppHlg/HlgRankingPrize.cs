using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Hlg;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
namespace Genora.MultiTenancy.DomainModels.AppHlg;
public class HlgRankingPrize : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid EventId { get; set; }
    public Guid RewardId { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = "";
    public int Quantity { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    protected HlgRankingPrize() { }
    public HlgRankingPrize(Guid id, Guid? tenantId) : base(id) { TenantId = tenantId; }
}
