using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieRatingDetails")]
public class AppCaddieRatingDetail : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid RatingId { get; set; }

    public Guid SkillId { get; set; }

    public int Score { get; set; }

    public virtual AppCaddieRating? Rating { get; set; }
    public virtual AppCaddieSkill? Skill { get; set; }

    protected AppCaddieRatingDetail() { }

    public AppCaddieRatingDetail(Guid id, Guid ratingId, Guid skillId, int score) : base(id)
    {
        RatingId = ratingId;
        SkillId = skillId;
        Score = score;
    }
}
