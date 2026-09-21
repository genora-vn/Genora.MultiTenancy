using System.Globalization;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Configuration;

namespace Genora.MultiTenancy.Gateway;

public static class TenantGatewayExtensions
{
    public const string KeyHeader = "X-Genora-Gateway-Key";
    public const string TenantHeader = "X-Genora-Gateway-Tenant";

    public static IServiceCollection AddTenantGateway(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = TenantGatewayOptions.Load(configuration);
        var routes = new List<RouteConfig>();
        var clusters = new List<ClusterConfig>();
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.OnRejected = async (context, ct) =>
            {
                var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)) : 1;
                context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                context.HttpContext.Response.Headers.CacheControl = "no-store";
                object body = context.HttpContext.Request.Path.StartsWithSegments("/api/mini-app/hlg")
                    ? new { error = 429, data = (object?)null, message = "Hệ thống đang có nhiều lượt truy cập. Vui lòng thử lại sau ít giây." }
                    : new { success = false, data = (object?)null,
                        error = context.HttpContext.Request.Path.StartsWithSegments("/api/mini-app/hl25") ? "Hl25:RateLimitExceeded" : "Gateway:RateLimitExceeded",
                        message = "Hệ thống đang có nhiều lượt truy cập. Vui lòng thử lại sau ít giây." };
                await context.HttpContext.Response.WriteAsJsonAsync(body, ct);
            };
        });
        foreach (var (name, tenant) in settings.Tenants.Where(t => t.Value.Enabled))
        {
            var tenantId = tenant.TenantId.ToString("D");
            var origin = new Uri(tenant.BackendTenantOrigin);
            var policy = "tenant-" + tenantId;
            var endpoints = GatewayApiProfiles.GetEndpoints(tenant);
            services.AddCors(o => o.AddPolicy(policy, p => p.WithOrigins(tenant.AllowedOrigins)
                .WithMethods(endpoints.SelectMany(e => e.Methods).Append("OPTIONS").Distinct().ToArray())
                .AllowAnyHeader().AllowCredentials().WithExposedHeaders("Retry-After")));
            services.AddRateLimiter(o => o.AddPolicy(policy, _ => RateLimitPartition.Get(tenantId,
                _ => new SlidingWindowRateLimiter(new()
                {
                    PermitLimit = tenant.PermitLimit, Window = TimeSpan.FromSeconds(1), SegmentsPerWindow = 10,
                    QueueLimit = 0, AutoReplenishment = true // Native timer per tenant; no partition heartbeat drift.
                }))));
            IReadOnlyDictionary<string, string>[] transforms =
            [
                // Put YARP's original-Host suppression before the pinned Host transform.
                // Its implicit default otherwise runs last and clears an identical Host.
                new Dictionary<string, string> { ["RequestHeaderOriginalHost"] = "false" },
                new Dictionary<string, string> { ["X-Forwarded"] = "Set" },
                Header("Host", origin.Authority), Header("X-Forwarded-Host", origin.Authority),
                Header("X-Forwarded-Proto", origin.Scheme),
                Remove("Forwarded"), Remove("X-Forwarded-Prefix"), Remove("Cookie"), Remove("Authorization"), Remove("__tenant"),
                Remove(KeyHeader), Remove(TenantHeader), Remove(Hl25GatewayExtensions.GatewayKeyHeader),
                Header("tenant", tenantId),
                Header(settings.LegacyHl25 ? Hl25GatewayExtensions.GatewayKeyHeader : KeyHeader, tenant.SharedKey),
                Header(TenantHeader, tenantId),
                new Dictionary<string, string> { ["QueryRemoveParameter"] = "__tenant" },
                new Dictionary<string, string> { ["QueryValueParameter"] = "tenant", ["Set"] = tenantId },
                new Dictionary<string, string> { ["ResponseHeaderRemove"] = "Access-Control-Allow-Origin" },
                new Dictionary<string, string> { ["ResponseHeaderRemove"] = "Access-Control-Allow-Credentials" },
                new Dictionary<string, string> { ["ResponseHeaderRemove"] = "Access-Control-Expose-Headers" }
            ];
            routes.AddRange(endpoints.Select((e, i) => new RouteConfig
            {
                RouteId = $"{name}-{i}", ClusterId = name, RateLimiterPolicy = policy, CorsPolicy = policy,
                Match = new() { Hosts = tenant.PublicHosts, Path = e.Path, Methods = e.Methods }, Transforms = transforms
            }));
            clusters.Add(new ClusterConfig
            {
                ClusterId = name, HttpRequest = new() { ActivityTimeout = TimeSpan.FromSeconds(60) },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["abp"] = new() { Address = tenant.BackendAddress.TrimEnd('/') + "/" }
                }
            });
        }
        services.AddReverseProxy().LoadFromMemory(routes, clusters);
        return services;
    }

    public static WebApplication UseTenantGateway(this WebApplication app)
    {
        app.UseRouting();
        app.UseCors();
        app.UseRateLimiter();
        app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
        app.MapReverseProxy();
        return app;
    }

    private static Dictionary<string, string> Header(string name, string value) => new() { ["RequestHeader"] = name, ["Set"] = value };
    private static Dictionary<string, string> Remove(string name) => new() { ["RequestHeaderRemove"] = name };
}
