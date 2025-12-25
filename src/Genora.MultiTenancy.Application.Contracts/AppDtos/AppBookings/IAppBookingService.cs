using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public interface IAppBookingService :
        ICrudAppService<
            AppBookingDto,
            Guid,
            GetBookingListInput,
            CreateUpdateAppBookingDto>
{
    Task<IRemoteStreamContent> ExportExcelAsync(GetBookingListInput input);
    Task<IRemoteStreamContent> DownloadTemplateAsync();
    Task<IRemoteStreamContent> DownloadImportTemplateAsync();
    Task ImportExcelAsync(ImportBookingExcelInput input);
}