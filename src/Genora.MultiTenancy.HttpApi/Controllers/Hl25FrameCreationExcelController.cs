using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

/// <summary>
/// Controller expose endpoint tải Excel lịch sử tạo ảnh thiệp Hoa Linh 25 Năm.
/// Dùng cho genora.excel.download (GET + query filter).
/// </summary>
[ApiController]
[Route("api/app/hl25-frame-creation-excel")]
public class Hl25FrameCreationExcelController : AbpController
{
    private readonly IHl25FrameCreationAppService _service;

    public Hl25FrameCreationExcelController(IHl25FrameCreationAppService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetHl25FrameCreationListInput input)
    {
        return _service.ExportExcelAsync(input);
    }
}
