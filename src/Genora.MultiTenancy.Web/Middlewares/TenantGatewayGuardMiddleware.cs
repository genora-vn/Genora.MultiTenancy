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

    public bool IsValid() => GetValidationErrors().Count == 0;

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (!Enabled) return errors;
        var enabled = Tenants?.Where(t => t.Value?.Enabled == true).ToArray();
        if (enabled == null || enabled.Length == 0)
        {
            errors.Add("TenantGatewayGuard:Tenants requires at least one enabled tenant.");
            return errors;
        }
        var ids = new HashSet<Guid>();
        var keys = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, tenant) in enabled)
        {
            var prefix = $"TenantGatewayGuard:Tenants:{name}";
            if (tenant.TenantId == Guid.Empty || !ids.Add(tenant.TenantId))
                errors.Add($"{prefix}:TenantId must be a distinct, non-empty tenant GUID.");
            if (string.IsNullOrEmpty(tenant.SharedKey) || tenant.SharedKey.Length < 32 || tenant.SharedKey.Any(c => c < 33 || c > 126))
                errors.Add($"{prefix}:SharedKey requires at least 32 printable ASCII characters without spaces. Set the same key for this tenant in Gateway.");
            else if (!keys.TryAdd(tenant.SharedKey, name))
                errors.Add($"{prefix}:SharedKey duplicates TenantGatewayGuard:Tenants:{keys[tenant.SharedKey]}:SharedKey. Use a different key per tenant, matching its Gateway configuration.");
            if (tenant.PathPrefixes == null || tenant.PathPrefixes.Length == 0 || !tenant.PathPrefixes.All(IsPrefix))
                errors.Add($"{prefix}:PathPrefixes requires explicit /api/mini-app/... prefixes without trailing slash, wildcard or traversal.");
            if (tenant.ExcludedPathPrefixes == null || !tenant.ExcludedPathPrefixes.All(e => IsPrefix(e) &&
                    tenant.PathPrefixes != null && tenant.PathPrefixes.Any(p => IsPrefix(p) && e.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase))))
                errors.Add($"{prefix}:ExcludedPathPrefixes must be narrower child paths of PathPrefixes; use [] when no exclusions are needed.");
        }
        return errors;
    }
    private static bool IsPrefix(string p) => !string.IsNullOrEmpty(p) && p.StartsWith("/api/mini-app/", StringComparison.OrdinalIgnoreCase) &&
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
