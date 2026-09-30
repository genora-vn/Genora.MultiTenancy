using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/hlg-ranking-excel")]
public class HlgRankingExcelController : AbpController
{
    private readonly IHlgRankingAdminAppService _service;

    public HlgRankingExcelController(IHlgRankingAdminAppService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] Guid eventId)
    {
        return _service.ExportResultsAsync(eventId);
    }
}
