using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Web.Middlewares;

public sealed class Hl25GatewayGuardOptions
{
    public bool Enabled { get; set; }
    public Guid TenantId { get; set; }
    public string SharedKey { get; set; } = "";
}

// Opt-in during gateway rollout. Runs after tenant resolution, before tenant migration / business UoW.
public sealed class Hl25GatewayGuardMiddleware
{
    public const string KeyHeader = "X-Hl25-Gateway-Key";
    private readonly RequestDelegate _next;
    private readonly Hl25GatewayGuardOptions _options;
    private readonly byte[] _keyHash;

    public Hl25GatewayGuardMiddleware(RequestDelegate next, IOptions<Hl25GatewayGuardOptions> options)
    {
        _next = next;
        _options = options.Value;
        _keyHash = SHA256.HashData(Encoding.UTF8.GetBytes(_options.SharedKey));
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant tenant)
    {
        var path = context.Request.Path;
        if (!_options.Enabled || !path.StartsWithSegments("/api/mini-app/hl25") ||
            path.StartsWithSegments("/api/mini-app/hl25/admin"))
        {
            await _next(context);
            return;
        }

        var hasKey = context.Request.Headers.TryGetValue(KeyHeader, out var key);
        // Other tenants remain unchanged; a gateway-marked request MUST resolve to the configured tenant.
        if (tenant.Id != _options.TenantId && !hasKey)
        {
            await _next(context);
            return;
        }
        var valid = tenant.Id == _options.TenantId && key.Count == 1 &&
            CryptographicOperations.FixedTimeEquals(_keyHash, SHA256.HashData(Encoding.UTF8.GetBytes(key.ToString())));
        context.Request.Headers.Remove(KeyHeader);
        if (!valid)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.Headers.CacheControl = "no-store";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false, data = (object)null, error = "Hl25:GatewayRequired",
                message = "Yêu cầu không hợp lệ. Vui lòng truy cập qua cổng chương trình."
            }, context.RequestAborted);
            return;
        }
        await _next(context);
    }
}
