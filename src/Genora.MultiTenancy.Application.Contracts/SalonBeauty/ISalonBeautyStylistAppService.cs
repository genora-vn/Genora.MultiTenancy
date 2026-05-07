using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using System;

namespace Genora.MultiTenancy.SalonBeauty;

public interface ISalonBeautyStylistAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyStylistDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyStylistDto> GetAsync(Guid id);
    Task<SalonBeautyStylistDto> CreateAsync(CreateSalonBeautyStylistDto input);
    Task<SalonBeautyStylistDto> UpdateAsync(Guid id, UpdateSalonBeautyStylistDto input);
    Task DeleteAsync(Guid id);
}

public class CreateSalonBeautyStylistDto
{
    public string DisplayName { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public byte? Gender { get; set; }
    public byte? Role { get; set; }
    public byte? Level { get; set; }
    public int ExperienceYear { get; set; }
    public byte Status { get; set; } = 1;
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateSalonBeautyStylistDto
{
    public string DisplayName { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public byte? Gender { get; set; }
    public byte? Role { get; set; }
    public byte? Level { get; set; }
    public int ExperienceYear { get; set; }
    public byte Status { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}

public class SalonBeautyStylistDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public byte? Gender { get; set; }
    public byte? Role { get; set; }
    public byte? Level { get; set; }
    public int ExperienceYear { get; set; }
    public decimal RatingAvg { get; set; }
    public int TotalBooking { get; set; }
    public byte Status { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
