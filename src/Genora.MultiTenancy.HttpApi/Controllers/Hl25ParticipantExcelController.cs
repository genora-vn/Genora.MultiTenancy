using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

/// <summary>
/// Controller expose endpoint tải Excel danh sách người tham gia Hoa Linh 25 Năm.
/// Dùng cho genora.excel.download (GET + query filter).
/// </summary>
[ApiController]
[Route("api/app/hl25-participant-excel")]
public class Hl25ParticipantExcelController : AbpController
{
    private readonly IHl25ParticipantAppService _service;

    public Hl25ParticipantExcelController(IHl25ParticipantAppService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetHl25ParticipantListInput input)
    {
        return _service.ExportExcelAsync(input);
    }
}
