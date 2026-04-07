using Genora.MultiTenancy.AppDtos.AppPaymentConfigurations;
using Genora.MultiTenancy.AppServices.AppPaymentConfigurations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppPaymentConfigurations;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdatePaymentConfigurationPageModel Config { get; set; } = new();

    private readonly IAppPaymentConfigurationService _service;

    public CreateModalModel(IAppPaymentConfigurationService service)
    {
        _service = service;
    }

    public void OnGet()
    {
        Config = new CreateUpdatePaymentConfigurationPageModel
        {
            IsActive = true,
            DisplayOrder = 0
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.CreateAsync(ObjectMapper.Map<CreateUpdatePaymentConfigurationPageModel, CreateUpdatePaymentConfigurationDto>(Config));
        return NoContent();
    }
}
