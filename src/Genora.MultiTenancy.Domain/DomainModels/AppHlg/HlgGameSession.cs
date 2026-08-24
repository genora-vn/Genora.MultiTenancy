using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Phiên chơi game. Điểm được chấm & tích lũy SERVER-SIDE (BD-2) — Score là nguồn sự thật,
/// KHÔNG tin totalScore client gửi. Khớp contract GameSession. Schema: HLG.
/// </summary>
[Table("AppHlgGameSessions", Schema = "HLG")]
public class HlgGameSession : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid GameId { get; set; }

    /// <summary>Người chơi (dbo.AppCustomers).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Câu hỏi hiện tại (contract: currentIndex).</summary>
    public int CurrentIndex { get; set; }

    /// <summary>Điểm tích lũy server-side (nguồn sự thật). Cộng dồn qua từng /answer.</summary>
    public int Score { get; set; }

    /// <summary>Số câu trả lời đúng (server đếm).</summary>
    public int CorrectCount { get; set; }

    /// <summary>Tổng số câu hỏi của game tại thời điểm bắt đầu (snapshot).</summary>
    public int TotalQuestions { get; set; }

    public DateTime StartedAt { get; set; }

    /// <summary>Đã finish chưa (chống finish/answer nhiều lần).</summary>
    public bool IsFinished { get; set; }

    public DateTime? FinishedAt { get; set; }

    /// <summary>Địa chỉ giao hàng (luồng consumer, Phase 4) — set qua endpoint shipping-address.</summary>
    public Guid? ShippingAddressId { get; set; }

    public virtual ICollection<HlgSessionAnswer> Answers { get; set; } = new List<HlgSessionAnswer>();

    protected HlgGameSession() { }

    public HlgGameSession(Guid id, Guid gameId, Guid customerId, Guid? tenantId = null) : base(id)
    {
        GameId = gameId;
        CustomerId = customerId;
        TenantId = tenantId;
    }
}
