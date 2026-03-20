using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public class FnbItemDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsAvailable { get; set; }
    public int SortOrder { get; set; }
}