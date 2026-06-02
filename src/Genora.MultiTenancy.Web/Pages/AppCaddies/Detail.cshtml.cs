using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.AppCaddies;

public class DetailModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public CaddieDto Caddie { get; set; } = null!;

    private readonly CaddieAppService _caddieAppService;

    public DetailModel(CaddieAppService caddieAppService)
    {
        _caddieAppService = caddieAppService;
    }

    public async Task OnGetAsync()
    {
        Caddie = await _caddieAppService.GetAsync(Id);
    }
}
