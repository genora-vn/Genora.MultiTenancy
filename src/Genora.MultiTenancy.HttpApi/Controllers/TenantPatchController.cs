using Genora.MultiTenancy.Tenants;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Controllers;

[Route("api/tenants/patch")]
public class TenantPatchController : MultiTenancyController
{
    private readonly TenantPatchAppService _svc;
    public TenantPatchController(TenantPatchAppService svc) { _svc = svc; }

    [HttpPost("all")]
    public Task<int> PatchAll() => _svc.PatchAllAsync();

    [HttpPost("{name}")]
    public Task PatchOne(string name) => _svc.PatchOneAsync(name);
}