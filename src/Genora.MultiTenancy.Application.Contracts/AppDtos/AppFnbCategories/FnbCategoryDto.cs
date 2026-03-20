using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public class FnbCategoryDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}