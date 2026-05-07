using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using System;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;

namespace Genora.MultiTenancy.SalonBeauty;

public interface ISalonBeautyServiceAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyServiceDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyServiceDto> GetAsync(Guid id);
    Task<SalonBeautyServiceDto> CreateAsync(CreateSalonBeautyServiceDto input);
    Task<SalonBeautyServiceDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceDto input);
    Task DeleteAsync(Guid id);
}

public class CreateSalonBeautyServiceDto
{
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public byte? ApplicableRole { get; set; }
    public byte? ApplicableLevel { get; set; }
    public byte Status { get; set; } = 1;
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateSalonBeautyServiceDto
{
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public byte? ApplicableRole { get; set; }
    public byte? ApplicableLevel { get; set; }
    public byte Status { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}

public class SalonBeautyServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public byte? ApplicableRole { get; set; }
    public byte? ApplicableLevel { get; set; }
    public byte Status { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
