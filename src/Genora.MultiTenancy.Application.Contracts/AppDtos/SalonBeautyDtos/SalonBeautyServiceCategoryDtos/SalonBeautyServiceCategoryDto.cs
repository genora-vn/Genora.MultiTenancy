using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyServiceCategoryDtos;

public class SalonBeautyServiceCategoryDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public byte Status { get; set; }
    public string? Note { get; set; }
}
