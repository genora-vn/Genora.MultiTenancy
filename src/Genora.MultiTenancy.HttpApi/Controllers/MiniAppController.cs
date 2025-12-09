using Genora.MultiTenancy.Apps.AppSettings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Controllers;


[Route("api/mini-app")]
public class MiniAppController : MultiTenancyController
{
    private readonly IAppSettingService _appSettingService;

    public MiniAppController(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    [HttpGet("settings")]
    public async Task<object> GetSettingAsync(PagedAndSortedResultRequestDto input)
    {
        var cs = await _appSettingService.GetListAsync(input);
        return cs;
    }
}