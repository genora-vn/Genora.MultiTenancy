using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/hl-sales-excel")]
public class HlSalesExcelController : AbpController
{
    private readonly IHlSalesExportAppService _service;
    public HlSalesExcelController(IHlSalesExportAppService service) => _service = service;

    [HttpGet("point-history")]
    public Task<IRemoteStreamContent> PointHistory([FromQuery] HlPointHistoryFilter input, [FromQuery] bool batches = false)
        => _service.ExportPointHistoryAsync(input, batches);

    [HttpGet("gift-exchanges")]
    public Task<IRemoteStreamContent> GiftExchanges([FromQuery] HlGiftExchangeFilterDto input)
        => _service.ExportGiftExchangesAsync(input);

    [HttpGet("orders")]
    public Task<IRemoteStreamContent> Orders([FromQuery] HlSalesOrderFilter input)
        => _service.ExportOrdersAsync(input);
}
