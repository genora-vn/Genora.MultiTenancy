using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppProCategories;

public class ProCategoryDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
