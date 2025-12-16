using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;
public interface IAppCalendarSlotService :
        ICrudAppService<
            AppCalendarSlotDto,
            Guid,
            GetCalendarSlotListInput,
            CreateUpdateAppCalendarSlotDto>
{
    /// <summary>
    /// Lấy toàn bộ slot của 1 sân trong 1 ngày (hiển thị trên calendar).
    /// </summary>
    Task<List<AppCalendarSlotDto>> GetByDateAsync(GetCalendarSlotByDateInput input);
}