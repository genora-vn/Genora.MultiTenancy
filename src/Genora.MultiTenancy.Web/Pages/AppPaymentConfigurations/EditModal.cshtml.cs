using Genora.MultiTenancy.AppDtos.AppPaymentConfigurations;
using Genora.MultiTenancy.AppServices.AppPaymentConfigurations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppPaymentConfigurations;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdatePaymentConfigurationPageModel Config { get; set; } = new();

    private readonly IAppPaymentConfigurationService _service;

    public EditModalModel(IAppPaymentConfigurationService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Config = ObjectMapper.Map<PaymentConfigurationDto, CreateUpdatePaymentConfigurationPageModel>(dto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Id, ObjectMapper.Map<CreateUpdatePaymentConfigurationPageModel, CreateUpdatePaymentConfigurationDto>(Config));
        return NoContent();
    }
}
