using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/hlg-question-excel")]
public class HlgQuestionExcelController : AbpController
{
    private readonly IHlgQuestionAdminAppService _service;

    public HlgQuestionExcelController(IHlgQuestionAdminAppService service)
    {
        _service = service;
    }

    [HttpGet("template")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Template([FromQuery] Guid gameId)
    {
        return _service.DownloadImportTemplateAsync(gameId);
    }

    [HttpPost("import")]
    [DisableValidation]
    public Task<int> Import([FromForm] ImportHlgQuestionExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}
