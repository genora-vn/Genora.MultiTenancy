using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/hlg-winner-excel")]
public class HlgWinnerExcelController : AbpController
{
    private readonly IHlgWinnerAdminAppService _service;

    public HlgWinnerExcelController(IHlgWinnerAdminAppService service)
    {
        _service = service;
    }

    [HttpGet("template")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Template()
    {
        return _service.DownloadImportTemplateAsync();
    }

    [HttpPost("import")]
    [DisableValidation]
    public Task<int> Import([FromForm] ImportHlgWinnerExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}
