using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Một câu trả lời trong phiên chơi — đã chấm SERVER-SIDE (BD-2).
/// Là nguồn đối soát khi /finish: tổng ScoreGained của các answer = điểm thật, bỏ qua totalScore client.
/// Schema: HLG.
/// </summary>
[Table("AppHlgSessionAnswers", Schema = "HLG")]
public class HlgSessionAnswer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }
    public virtual HlgGameSession? Session { get; set; }

    public Guid QuestionId { get; set; }

    /// <summary>Đáp án người chơi chọn.</summary>
    public HlgAnswerKey SelectedKey { get; set; }

    /// <summary>Server chấm: đúng hay sai.</summary>
    public bool IsCorrect { get; set; }

    /// <summary>Điểm server cộng cho câu này (nguồn sự thật).</summary>
    public int ScoreGained { get; set; }

    /// <summary>Thời gian trả lời (giây) — client gửi, dùng cho hệ số thời gian.</summary>
    public int TimeSpentSec { get; set; }

    protected HlgSessionAnswer() { }

    public HlgSessionAnswer(Guid id, Guid sessionId, Guid questionId, Guid? tenantId = null) : base(id)
    {
        SessionId = sessionId;
        QuestionId = questionId;
        TenantId = tenantId;
    }
}
