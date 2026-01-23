using Genora.MultiTenancy.AppDtos.AppEmails;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppEmails;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateEmailDto Email { get; set; } = new();

    private readonly IAppEmailService _appEmailService;

    public CreateModalModel(IAppEmailService appEmailService)
    {
        _appEmailService = appEmailService;
    }

    public void OnGet()
    {
        Email.ToEmails ??= string.Empty;
        Email.Subject ??= string.Empty;
        Email.Body ??= string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appEmailService.CreateAsync(Email);
        return NoContent();
    }
}
