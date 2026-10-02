using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Immutable event-result row captured on the first Excel export after an event ends.
/// One row represents one customer-game pair from the exported report.
/// </summary>
[Table("AppHlgRankingResultSnapshots", Schema = "HLG")]
public class HlgRankingResultSnapshot : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid EventId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid GameId { get; set; }
    public int EventRank { get; set; }

    [StringLength(100)]
    public string? CustomerCode { get; set; }

    [Required, StringLength(250)]
    public string PlayerName { get; set; } = "";

    [Required, StringLength(30)]
    public string PhoneNumber { get; set; } = "";

    [StringLength(100)]
    public string? ZaloUserId { get; set; }

    [Required, StringLength(250)]
    public string GameName { get; set; } = "";

    public int PlayCount { get; set; }
    public int GameScore { get; set; }
    public int BestScore { get; set; }
    public int CorrectAnswerCount { get; set; }
    public int TotalQuestionCount { get; set; }
    public int EventScore { get; set; }
    public DateTime FirstPlayedAt { get; set; }
    public DateTime LastPlayedAt { get; set; }

    protected HlgRankingResultSnapshot() { }

    public HlgRankingResultSnapshot(Guid id, Guid eventId, Guid customerId, Guid gameId, Guid? tenantId = null)
        : base(id)
    {
        EventId = eventId;
        CustomerId = customerId;
        GameId = gameId;
        TenantId = tenantId;
    }
}
