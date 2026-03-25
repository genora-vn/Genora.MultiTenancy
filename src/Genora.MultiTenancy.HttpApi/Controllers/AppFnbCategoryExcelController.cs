using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-fnb-category-excel")]
public class AppFnbCategoryExcelController : AbpController
{
    private readonly IAppFnbCategoryService _service;

    public AppFnbCategoryExcelController(IAppFnbCategoryService service)
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
    public Task<IRemoteStreamContent> Export([FromQuery] GetFnbCategoryListInput input)
    {
        return _service.ExportExcelAsync(input);
    }

    [HttpPost("import")]
    [DisableValidation]
    public Task<int> Import([FromForm] ImportFnbCategoryExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}
