using Genora.MultiTenancy.AppDtos.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Controllers;

[Authorize]
[Route("api/master-data")]
public class MasterDataController : AbpController
{
    private readonly IProvinceLookupAppService _provinceService;

    public MasterDataController(IProvinceLookupAppService provinceService)
    {
        _provinceService = provinceService;
    }

    [HttpGet("provinces")]
    public Task<List<ProvinceLookupDto>> GetProvincesAsync([FromQuery] bool forceRefresh = false)
    {
        return _provinceService.GetProvincesAsync(forceRefresh);
    }
}
