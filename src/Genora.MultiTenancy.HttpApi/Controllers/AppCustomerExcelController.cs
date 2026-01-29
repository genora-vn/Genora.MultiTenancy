using Genora.MultiTenancy.AppDtos.AppCustomers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/app-customer-excel")]
public class AppCustomerExcelController : AbpController
{
    private readonly IAppCustomerService _service;

    public AppCustomerExcelController(IAppCustomerService service)
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
    public Task<int> Import([FromForm] ImportCustomerExcelInput input)
    {
        return _service.ImportExcelAsync(input);
    }
}