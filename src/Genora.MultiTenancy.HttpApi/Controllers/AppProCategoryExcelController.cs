using Genora.MultiTenancy.AppDtos.AppProCategories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-pro-category-excel")]
public class AppProCategoryExcelController : AbpController
{
    private readonly IAppProCategoryService _service;

    public AppProCategoryExcelController(IAppProCategoryService service)
    {
        _service = service;
    }

    [HttpGet("template")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Template()
        => _service.DownloadImportTemplateAsync();

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetProCategoryListInput input)
        => _service.ExportExcelAsync(input);

    [HttpPost("import")]
    [DisableValidation]
    public Task<int> Import([FromForm] ImportProCategoryExcelInput input)
        => _service.ImportExcelAsync(input);

    /// <summary>
    /// Bật/tắt IsActive cho ProCategory.
    /// PUT /api/app/app-pro-category-excel/{id}/set-active
    /// ABP JS proxy sẽ generate: service.setActive(id, { isActive })
    /// </summary>
    [HttpPut("{id}/set-active")]
    public Task<ProCategoryDto> SetActive(Guid id, [FromBody] SetProCategoryActiveDto input)
        => _service.SetActiveAsync(id, input);
}

