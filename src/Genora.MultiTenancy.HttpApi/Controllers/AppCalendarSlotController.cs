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
    public async Task<IRemoteStreamContent> Template()
    {
        return await _service.DownloadImportTemplateAsync();
    }

    [HttpPost("import")]
    [DisableValidation]
    public Task Import([FromForm] ImportCalendarExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}
