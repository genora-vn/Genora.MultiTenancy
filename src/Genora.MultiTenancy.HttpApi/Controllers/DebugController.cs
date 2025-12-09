using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Controllers;

[Route("api/debug")]
public class DebugController : MultiTenancyController
{
    private readonly IConnectionStringResolver _resolver;
    private readonly ICurrentTenant _current;

    public DebugController(IConnectionStringResolver resolver, ICurrentTenant current)
    {
        _resolver = resolver;
        _current = current;
    }

    [HttpGet("cs")]
    public async Task<object> GetCsAsync()
    {
        var cs = await _resolver.ResolveAsync("Default");
        return new { _current.Id, _current.Name, ConnectionString = cs };
    }
}