using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Bảng map Chiến dịch (HlgRankingEvent) ↔ Trò chơi (HlgGame): một chiến dịch gồm nhiều game cố định.
/// Dùng để tính mẫu số "Đã tham gia x/n" và giới hạn phạm vi xếp hạng theo đúng các game của chiến dịch.
/// </summary>
public class HlgRankingEventGame : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid EventId { get; set; }
    public Guid GameId { get; set; }
    public int DisplayOrder { get; set; }

    protected HlgRankingEventGame() { }

    public HlgRankingEventGame(Guid id, Guid eventId, Guid gameId, Guid? tenantId) : base(id)
    {
        EventId = eventId;
        GameId = gameId;
        TenantId = tenantId;
    }
}
