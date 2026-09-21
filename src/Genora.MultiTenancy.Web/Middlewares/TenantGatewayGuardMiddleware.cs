using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Web.Middlewares;

public sealed class TenantGatewayGuardOptions
{
    public bool Enabled { get; set; }
    public Dictionary<string, TenantGatewayGuardTenant> Tenants { get; set; } = new();

    public bool IsValid()
    {
        if (!Enabled) return true;
        var tenants = Tenants.Values.Where(t => t.Enabled).ToArray();
        return tenants.Length > 0 && tenants.Select(t => t.TenantId).Distinct().Count() == tenants.Length &&
            tenants.Select(t => t.SharedKey).Distinct().Count() == tenants.Length && tenants.All(t =>
                t.TenantId != Guid.Empty && t.SharedKey.Length >= 32 && t.SharedKey.All(c => c >= 33 && c <= 126) &&
                t.PathPrefixes.Length > 0 && t.PathPrefixes.All(IsPrefix) &&
                t.ExcludedPathPrefixes.All(e => IsPrefix(e) && t.PathPrefixes.Any(p =>
                    e.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase))));
    }
    private static bool IsPrefix(string p) => p.StartsWith("/api/mini-app/", StringComparison.OrdinalIgnoreCase) &&
        !p.EndsWith('/') && !p.Contains("//") && !p.Contains("..") && p.IndexOfAny(new[] { '*', '?', '#', '%', '{', '}', '\\' }) < 0;
}

public sealed class TenantGatewayGuardTenant
{
    public bool Enabled { get; set; } = true;
    public Guid TenantId { get; set; }
    public string SharedKey { get; set; } = "";
    public string[] PathPrefixes { get; set; } = Array.Empty<string>();
    public string[] ExcludedPathPrefixes { get; set; } = Array.Empty<string>();
}

public sealed class TenantGatewayGuardMiddleware
{
    public const string KeyHeader = "X-Genora-Gateway-Key";
    public const string TenantHeader = "X-Genora-Gateway-Tenant";
    private readonly RequestDelegate _next;
    private readonly TenantGatewayGuardOptions _options;
    private readonly Dictionary<Guid, byte[]> _hashes;

    public TenantGatewayGuardMiddleware(RequestDelegate next, IOptions<TenantGatewayGuardOptions> options)
    {
        _next = next;
        _options = options.Value;
        _hashes = _options.Enabled ? _options.Tenants.Values.Where(t => t.Enabled).ToDictionary(t => t.TenantId,
            t => SHA256.HashData(Encoding.UTF8.GetBytes(t.SharedKey))) : new();
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant)
    {
        if (!_options.Enabled) { await _next(context); return; }
        var path = context.Request.Path;
        var protectedTenant = _options.Tenants.Values.FirstOrDefault(t => t.Enabled && t.TenantId == currentTenant.Id &&
            t.PathPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)) &&
            !t.ExcludedPathPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)));
        var marked = context.Request.Headers.ContainsKey(KeyHeader) || context.Request.Headers.ContainsKey(TenantHeader) ||
            context.Request.Headers.ContainsKey(Hl25GatewayGuardMiddleware.KeyHeader);
        if (protectedTenant == null && !marked) { await _next(context); return; }
        var key = context.Request.Headers[KeyHeader];
        var claim = context.Request.Headers[TenantHeader];
        var valid = protectedTenant != null && key.Count == 1 && claim.Count == 1 &&
            Guid.TryParse(claim[0], out var tenantId) && tenantId == protectedTenant.TenantId &&
            CryptographicOperations.FixedTimeEquals(_hashes[tenantId], SHA256.HashData(Encoding.UTF8.GetBytes(key.ToString())));
        context.Request.Headers.Remove(KeyHeader);
        context.Request.Headers.Remove(TenantHeader);
        context.Request.Headers.Remove(Hl25GatewayGuardMiddleware.KeyHeader);
        if (valid) { await _next(context); return; }
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.Headers.CacheControl = "no-store";
        object body = path.StartsWithSegments("/api/mini-app/hlg")
            ? new { error = 403, data = (object)null, message = "Yêu cầu không hợp lệ. Vui lòng truy cập qua cổng chương trình." }
            : new { success = false, data = (object)null,
                error = path.StartsWithSegments("/api/mini-app/hl25") ? "Hl25:GatewayRequired" : "Gateway:GatewayRequired",
                message = "Yêu cầu không hợp lệ. Vui lòng truy cập qua cổng chương trình." };
        await context.Response.WriteAsJsonAsync(body, context.RequestAborted);
    }
}
