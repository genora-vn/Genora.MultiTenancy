using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Controllers;

[Area("MultiTenancy")]
[Route("api/host/zalo-zbs")]
//[Authorize(MultiTenancyPermissions.HostAppZaloAuths.Default)]
public class HostZaloZbsController : MultiTenancyController
{
    private readonly IZaloZbsClient _client;

    public HostZaloZbsController(IZaloZbsClient client)
    {
        _client = client;
    }

    /// <summary>
    /// API gọi toàn bộ ZBS API
    /// POST /api/host/zalo-zbs/call
    /// BODY: { api, method, path, query, body }
    /// </summary>
    /// <param name="req">ZaloZbsCallRequest</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns></returns>
    [HttpPost("call")]
    public async Task<IActionResult> CallAsync([FromBody] ZaloZbsCallRequest req, CancellationToken ct)
    {
        var body = await _client.CallAsync(req, ct);

        // nếu response là JSON => trả raw JSON
        return Content(body ?? "", "application/json");
    }
}