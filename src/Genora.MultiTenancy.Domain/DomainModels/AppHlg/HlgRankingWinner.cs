using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Hlg;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
namespace Genora.MultiTenancy.DomainModels.AppHlg;
public class HlgRankingWinner : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid EventId { get; set; }
    public Guid PrizeId { get; set; }
    public Guid CustomerId { get; set; }
    public int Rank { get; set; }
    public int Score { get; set; }
    public bool IsActive { get; set; }
    protected HlgRankingWinner() { }
    public HlgRankingWinner(Guid id, Guid? tenantId) : base(id) { TenantId = tenantId; }
}
