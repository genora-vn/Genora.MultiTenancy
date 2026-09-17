using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Câu hỏi của game. Khớp contract Question (KHÔNG bao gồm correctKey — bí mật server-side, BD-2).
/// CorrectKey chỉ dùng server chấm điểm, KHÔNG serialize ra client lúc chơi. Schema: HLG.
/// </summary>
[Table("AppHlgQuestions", Schema = "HLG")]
public class HlgQuestion : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid GameId { get; set; }
    public virtual HlgGame? Game { get; set; }

    /// <summary>Thứ tự câu hỏi trong game (contract: index).</summary>
    public int Index { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    /// <summary>Giới hạn thời gian trả lời (giây).</summary>
    public int TimeLimitSec { get; set; } = 30;

    /// <summary>Hệ số nhân điểm (contract: scoreMultiplier). Server dùng chấm điểm; client chỉ hiển thị.</summary>
    public decimal ScoreMultiplier { get; set; } = 1m;

    /// <summary>⚠️ ĐÁP ÁN ĐÚNG — BÍ MẬT server-side. KHÔNG trả về client lúc chơi (chống gian lận, BD-2).</summary>
    public HlgAnswerKey CorrectKey { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<HlgAnswerOption> Options { get; set; } = new List<HlgAnswerOption>();

    protected HlgQuestion() { }

    public HlgQuestion(Guid id, Guid gameId, int index, string content, Guid? tenantId = null) : base(id)
    {
        GameId = gameId;
        Index = index;
        Content = content;
        TenantId = tenantId;
    }
}
