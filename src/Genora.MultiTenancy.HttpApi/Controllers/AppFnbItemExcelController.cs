using Genora.MultiTenancy.AppDtos.AppFnbItems;
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
[Route("api/app/app-fnb-item-excel")]
public class AppFnbItemExcelController : AbpController
{
    private readonly IAppFnbItemService _service;

    public AppFnbItemExcelController(IAppFnbItemService service)
    {
        _service = service;
    }

    [HttpGet("template")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Template()
    {
        return _service.DownloadImportTemplateAsync();
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetFnbItemListInput input)
    {
        return _service.ExportExcelAsync(input);
    }

    [HttpPost("import")]
    [DisableValidation]
    public Task<int> Import([FromForm] ImportFnbItemExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}