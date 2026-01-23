using Genora.MultiTenancy.AppDtos.AppBookings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-booking-excel")]
public class AppBookingController : AbpController
{
    private readonly IAppBookingService _service;

    public AppBookingController(IAppBookingService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetBookingListInput input)
    {
        input.FilterText ??= string.Empty;
        return _service.ExportExcelAsync(input);
    }

    [HttpGet("template")]
    public async Task<IRemoteStreamContent> Template()
    {
        return await _service.DownloadImportTemplateAsync();
    }

    [HttpPost("import")]
    [DisableValidation]
    public Task Import([FromForm] ImportBookingExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}
