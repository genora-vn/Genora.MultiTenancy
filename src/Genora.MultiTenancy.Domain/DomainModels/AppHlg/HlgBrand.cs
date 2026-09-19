using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Hlg;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
namespace Genora.MultiTenancy.DomainModels.AppHlg;
public class HlgBrand : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid CategoryId { get; set; }
    [Required, StringLength(250)] public string Name { get; set; } = "";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    protected HlgBrand() { }
    public HlgBrand(Guid id, Guid? tenantId) : base(id) { TenantId = tenantId; }
}
