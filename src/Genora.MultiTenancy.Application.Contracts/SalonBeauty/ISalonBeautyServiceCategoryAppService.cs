using Volo.Abp.Application.Services;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyServiceCategoryDtos;
using Volo.Abp.Application.Dtos;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using System.Threading.Tasks;
using System;

namespace Genora.MultiTenancy.SalonBeauty;

public interface ISalonBeautyServiceCategoryAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyServiceCategoryDto> GetAsync(Guid id);
    Task<SalonBeautyServiceCategoryDto> CreateAsync(CreateSalonBeautyServiceCategoryDto input);
    Task<SalonBeautyServiceCategoryDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceCategoryDto input);
    Task DeleteAsync(Guid id);
}

public class CreateSalonBeautyServiceCategoryDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public byte Status { get; set; } = 1;
    public string? Note { get; set; }
}

public class UpdateSalonBeautyServiceCategoryDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public byte Status { get; set; }
    public string? Note { get; set; }
}

public class SalonBeautyServiceCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public byte Status { get; set; }
    public string? Note { get; set; }
}
