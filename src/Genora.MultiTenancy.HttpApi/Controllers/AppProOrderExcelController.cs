using Genora.MultiTenancy.AppDtos.AppProOrders;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-pro-order-excel")]
public class AppProOrderExcelController : AbpController
{
    private readonly IAppProOrderService _service;

    public AppProOrderExcelController(IAppProOrderService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetProOrderListInput input)
        => _service.ExportExcelAsync(input);
}
