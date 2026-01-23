using Genora.MultiTenancy.AppDtos.AppBookings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

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
    Task<IRemoteStreamContent> ExportExcelAsync(GetCalendarSlotListInput input);
    Task<IRemoteStreamContent> DownloadTemplateAsync();
    //Task<IRemoteStreamContent> DownloadImportTemplateAsync();
    Task<int> ImportExcelAsync(ImportCalendarExcelInput input);
    Task<IRemoteStreamContent> DownloadImportTemplateAsync(Guid? golfCourseId);
    Task<int> UpdateStatusBulkAsync(UpdateCalendarSlotStatusBulkInput input);
}