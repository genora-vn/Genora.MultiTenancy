using Genora.MultiTenancy.AppDtos.AppProItems;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-pro-item-excel")]
public class AppProItemExcelController : AbpController
{
    private readonly IAppProItemService _service;

    public AppProItemExcelController(IAppProItemService service)
    {
        _service = service;
    }

    [HttpGet("template")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Template()
        => _service.DownloadImportTemplateAsync();

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetProItemListInput input)
        => _service.ExportExcelAsync(input);

    [HttpPost("import")]
    [DisableValidation]
    public Task<int> Import([FromForm] ImportProItemExcelInput input)
        => _service.ImportExcelAsync(input);

    /// <summary>
    /// Cập nhật IsActive / IsAvailable cho ProItem.
    /// PUT /api/app/app-pro-item-excel/{id}/set-state
    /// ABP JS proxy sẽ generate: service.setState(id, { isActive, isAvailable })
    /// </summary>
    [HttpPut("{id}/set-state")]
    public Task<ProItemDto> SetState(Guid id, [FromBody] SetProItemStateDto input)
        => _service.SetStateAsync(id, input);
}

