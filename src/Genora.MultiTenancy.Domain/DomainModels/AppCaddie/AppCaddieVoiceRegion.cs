using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieVoiceRegions")]
public class AppCaddieVoiceRegion : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CaddieId { get; set; }

    public byte VoiceRegion { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public virtual AppCaddie? Caddie { get; set; }

    protected AppCaddieVoiceRegion() { }

    public AppCaddieVoiceRegion(Guid id, Guid caddieId, byte voiceRegion) : base(id)
    {
        CaddieId = caddieId;
        VoiceRegion = voiceRegion;
        CreationTime = DateTime.UtcNow;
    }
}
