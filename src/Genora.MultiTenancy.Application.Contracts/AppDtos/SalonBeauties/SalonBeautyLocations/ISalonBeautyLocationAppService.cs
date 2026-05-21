using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;

public interface ISalonBeautyLocationAppService :
    ICrudAppService<
        SalonBeautyLocationDto,
        Guid,
        GetSalonBeautyLocationListInput,
        CreateSalonBeautyLocationDto,
        UpdateSalonBeautyLocationDto>
{
    Task<List<SalonBeautyLocationLookupDto>> GetLookupAsync();
    Task UpdateActiveAsync(Guid id, bool isActive);
    Task UpdateShowOnAppAsync(Guid id, bool isShowOnApp);
}

public class SalonBeautyLocationLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public int SlotDuration { get; set; }
    public int BufferTime { get; set; }
    public int MaxCapacityPerSlot { get; set; }
}

public class GetSalonBeautyLocationListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsShowOnApp { get; set; }
}
