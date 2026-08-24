using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Game Gamification. Cấu hình động theo GameType (BD-1). Khớp contract Game.
/// totalQuestions tính động (đếm câu hỏi active). Schema: HLG.
/// </summary>
[Table("AppHlgGames", Schema = "HLG")]
public class HlgGame : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    public HlgGameType Type { get; set; }

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    /// <summary>Luật chơi (HTML/text).</summary>
    public string? Rules { get; set; }

    /// <summary>Mô tả phần thưởng hiển thị cho người chơi.</summary>
    public string? RewardDescription { get; set; }

    public HlgGameStatus Status { get; set; } = HlgGameStatus.Upcoming;

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    /// <summary>Điểm cơ bản mỗi câu đúng (server dùng chấm điểm, BD-2). Mặc định 100.</summary>
    public int BaseScorePerQuestion { get; set; } = 100;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<HlgQuestion> Questions { get; set; } = new List<HlgQuestion>();

    protected HlgGame() { }

    public HlgGame(Guid id, string name, HlgGameType type, Guid? tenantId = null) : base(id)
    {
        Name = name;
        Type = type;
        TenantId = tenantId;
    }
}
