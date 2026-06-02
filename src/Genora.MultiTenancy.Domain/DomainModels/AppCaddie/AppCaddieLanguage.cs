using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieLanguages")]
public class AppCaddieLanguage : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CaddieId { get; set; }

    public Guid LanguageId { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public virtual AppCaddie? Caddie { get; set; }
    public virtual AppLanguage? Language { get; set; }

    protected AppCaddieLanguage() { }

    public AppCaddieLanguage(Guid id, Guid caddieId, Guid languageId) : base(id)
    {
        CaddieId = caddieId;
        LanguageId = languageId;
        CreationTime = DateTime.UtcNow;
    }
}
