using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-fnb-order-excel")]
public class AppFnbOrderExcelController : AbpController
{
    private readonly IAppFnbOrderService _service;

    public AppFnbOrderExcelController(IAppFnbOrderService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] GetFnbOrderListInput input)
        => _service.ExportExcelAsync(input);
}
