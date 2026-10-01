using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Genora.MultiTenancy.HttpApi.Controllers;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Route("api/mini-app/hl/gift-receipts")]
public class HoaLinhGiftReceiptMiniAppController : MultiTenancyController
{
    private readonly IMiniAppHlGiftReceiptService _service;
    public HoaLinhGiftReceiptMiniAppController(IMiniAppHlGiftReceiptService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Confirm([FromBody] HlConfirmGiftInput input)
    {
        if (input == null) return BadRequest(HlApiResult<object>.Fail("HlGiftReceipt:InvalidData", "Thiếu dữ liệu nhận quà."));
        try { return Ok(HlApiResult<HlGiftReceiptDto>.Ok(await _service.ConfirmAsync(input), "Đã xác nhận yêu cầu nhận quà.")); }
        catch (UserFriendlyException ex) { return BadRequest(HlApiResult<object>.Fail(ex.Code ?? "HlGiftReceipt:InvalidData", ex.Message)); }
    }

    [HttpGet]
    public async Task<IActionResult> History([FromQuery] HlGiftReceiptHistoryInput input)
    {
        try { return Ok(HlApiResult<Volo.Abp.Application.Dtos.PagedResultDto<HlGiftReceiptDto>>.Ok(await _service.GetHistoryAsync(input))); }
        catch (UserFriendlyException ex) { return BadRequest(HlApiResult<object>.Fail(ex.Code ?? "HlGiftReceipt:InvalidData", ex.Message)); }
    }
}
