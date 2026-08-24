using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Lựa chọn đáp án của câu hỏi. Khớp contract Question.options[] = {key, content}.
/// An toàn để trả về client (không lộ đáp án đúng — đáp án đúng nằm ở HlgQuestion.CorrectKey). Schema: HLG.
/// </summary>
[Table("AppHlgAnswerOptions", Schema = "HLG")]
public class HlgAnswerOption : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid QuestionId { get; set; }
    public virtual HlgQuestion? Question { get; set; }

    /// <summary>Key của lựa chọn (A/B/C/D).</summary>
    public HlgAnswerKey Key { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    protected HlgAnswerOption() { }

    public HlgAnswerOption(Guid id, Guid questionId, HlgAnswerKey key, string content, Guid? tenantId = null) : base(id)
    {
        QuestionId = questionId;
        Key = key;
        Content = content;
        TenantId = tenantId;
    }
}
