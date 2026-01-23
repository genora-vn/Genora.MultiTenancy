using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-calendar-excel")]
public class AppCalendarSlotController : AbpController
{
    private readonly IAppCalendarSlotService _service;

    public AppCalendarSlotController(IAppCalendarSlotService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetCalendarSlotListInput input)
    {
        return _service.ExportExcelAsync(input);
    }

    [HttpGet("template")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Template([FromQuery] DownloadImportTemplateInput input)
    {
        // Nếu muốn bắt buộc chọn sân thì mở comment dưới
        // if (!input.GolfCourseId.HasValue || input.GolfCourseId.Value == Guid.Empty)
        //     throw new Volo.Abp.UserFriendlyException("Vui lòng chọn sân golf trước khi tải file mẫu.");

        return _service.DownloadImportTemplateAsync(input.GolfCourseId);
    }

    [HttpPost("import")]
    [DisableValidation]
    public Task<int> Import([FromForm] ImportCalendarExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}
