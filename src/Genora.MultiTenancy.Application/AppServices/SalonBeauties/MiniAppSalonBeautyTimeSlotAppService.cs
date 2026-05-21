using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyTimeSlotAppService : ApplicationService, IMiniAppSalonBeautyTimeSlotAppService
{
    private readonly IRepository<SalonBeautyTimeSlot, Guid> _repository;

    public MiniAppSalonBeautyTimeSlotAppService(IRepository<SalonBeautyTimeSlot, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<List<MiniAppSalonBeautyTimeSlotDto>> GetListAsyncTimeSlots(GetMiniAppTimeSlotListInput input)
    {
        var query = await _repository.GetQueryableAsync();

        query = query.Where(x => x.IsShowOnApp);
        query = query.WhereIf(input.LocationId.HasValue, x => x.LocationId == input.LocationId!.Value);
        query = query.WhereIf(input.StylistId.HasValue, x => x.StylistId == input.StylistId!.Value);
        query = query.WhereIf(input.Date.HasValue, x => x.WorkDate.Date == input.Date!.Value.Date);

        query = query.Where(x => x.Status != SalonBeautyTimeSlotStatus.Off);

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.WorkDate).ThenBy(x => x.StartTime));

        return items.Select(x => new MiniAppSalonBeautyTimeSlotDto
        {
            TimeSlotId = x.Id,
            WorkDate = x.WorkDate,
            StartTime = x.StartTime.ToString(@"hh\:mm"),
            EndTime = x.EndTime.ToString(@"hh\:mm"),
            Status = x.Status,
            IsShowOnApp = x.IsShowOnApp,
            BookedCount = x.BookedCount,
            Capacity = x.Capacity
        }).ToList();
    }
}
