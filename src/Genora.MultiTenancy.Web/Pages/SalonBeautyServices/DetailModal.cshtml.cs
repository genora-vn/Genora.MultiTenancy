using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyServices;

public class DetailModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SalonBeautyServiceDto Service { get; set; } = new();

    private readonly ISalonBeautyServiceAppService _serviceAppService;

    public DetailModalModel(ISalonBeautyServiceAppService serviceAppService)
    {
        _serviceAppService = serviceAppService;
    }

    public async Task OnGetAsync()
    {
        Service = await _serviceAppService.GetAsync(Id);
    }
}
