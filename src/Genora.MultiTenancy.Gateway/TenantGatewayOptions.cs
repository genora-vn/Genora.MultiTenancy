using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Genora.MultiTenancy.Gateway;

public sealed class TenantGatewayOptions
{
    public Dictionary<string, GatewayTenantOptions> Tenants { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    internal bool LegacyHl25 { get; set; }

    public static TenantGatewayOptions Load(IConfiguration configuration)
    {
        var section = configuration.GetSection("TenantGateway");
        TenantGatewayOptions result;
        if (section.Exists())
        {
            if (Guid.TryParse(configuration["Hl25Gateway:TenantId"], out var oldId) && oldId != Guid.Empty)
                throw new InvalidOperationException("Configure either TenantGateway or legacy Hl25Gateway, not both.");
            result = section.Get<TenantGatewayOptions>() ?? new();
        }
        else
        {
            var old = configuration.GetSection(Hl25GatewayOptions.SectionName).Get<Hl25GatewayOptions>() ?? new();
            old.Validate();
            result = new() { LegacyHl25 = true, Tenants = new()
            {
                ["hl25"] = new()
                {
                    TenantId = old.TenantId, PublicHosts = [new Uri(old.BackendTenantOrigin).Host],
                    BackendAddress = old.BackendAddress, BackendTenantOrigin = old.BackendTenantOrigin,
                    SharedKey = old.SharedKey, AllowedOrigins = old.AllowedOrigins,
                    PermitLimit = old.PermitLimit, ApiProfiles = ["Hl25"]
                }
            }};
        }
        result.Validate();
        return result;
    }

    public void Validate()
    {
        var ids = new HashSet<Guid>();
        var hosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, tenant) in Tenants.Where(t => t.Value.Enabled))
        {
            if (!Regex.IsMatch(name, "^[a-zA-Z0-9-]+$") || !names.Add(name))
                throw new InvalidOperationException("TenantGateway entry names must be unique letters, digits or hyphens.");
            if (tenant.TenantId == Guid.Empty || !ids.Add(tenant.TenantId))
                throw new InvalidOperationException($"TenantGateway:{name}: unique non-empty TenantId required. Put all APIs/aliases of one tenant in one entry.");
            if (tenant.PublicHosts.Length == 0 || tenant.PublicHosts.Any(h =>
                    !Regex.IsMatch(h, "^[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]$") ||
                    Uri.CheckHostName(h) == UriHostNameType.Unknown || !hosts.Add(h)))
                throw new InvalidOperationException($"TenantGateway:{name}: PublicHosts must be exact unique hostnames without scheme, port, wildcard or trailing dot.");
            if (!IsOrigin(tenant.BackendAddress) || !IsOrigin(tenant.BackendTenantOrigin))
                throw new InvalidOperationException($"TenantGateway:{name}: backend address/origin must be HTTP(S) origins without paths, credentials or query.");
            var backend = new Uri(tenant.BackendAddress);
            if (backend.Scheme == "http" && !backend.IsLoopback)
                throw new InvalidOperationException($"TenantGateway:{name}: use HTTPS for non-loopback backends.");
            if (tenant.SharedKey.Length < 32 || tenant.SharedKey.Any(c => c < 33 || c > 126) || !keys.Add(tenant.SharedKey))
                throw new InvalidOperationException($"TenantGateway:{name}: a distinct shared key of at least 32 printable ASCII characters is required.");
            if (tenant.PermitLimit < 1)
                throw new InvalidOperationException($"TenantGateway:{name}: PermitLimit must be positive.");
            if (tenant.AllowedOrigins.Length == 0 || tenant.AllowedOrigins.Any(o => !IsOrigin(o) || o.EndsWith('/')))
                throw new InvalidOperationException($"TenantGateway:{name}: exact CORS origins without trailing slash are required.");
            var endpoints = GatewayApiProfiles.GetEndpoints(tenant);
            if (endpoints.Count == 0)
                throw new InvalidOperationException($"TenantGateway:{name}: at least one API profile or explicit additional route is required.");
            var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var endpoint in endpoints)
            {
                if (!endpoint.Path.StartsWith("/api/mini-app/", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Path.IndexOfAny(['*', '?', '#', '%', '\\']) >= 0 || endpoint.Path.Contains("..") ||
                    endpoint.Path.EndsWith('/') || endpoint.Path.Contains("//") ||
                    endpoint.Path.Split('/').Any(s => s.Equals("admin", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"TenantGateway:{name}: only explicit Mini App routes without admin, catch-all or encoded paths are allowed.");
                RoutePatternFactory.Parse(endpoint.Path);
                if (endpoint.Methods.Length == 0 || endpoint.Methods.Any(m =>
                    !new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD" }.Contains(m)))
                    throw new InvalidOperationException($"TenantGateway:{name}: each route needs explicit supported uppercase HTTP methods.");
                foreach (var method in endpoint.Methods)
                    if (!matches.Add(method + " " + Regex.Replace(endpoint.Path, "\\{[^}]+\\}", "{}")))
                        throw new InvalidOperationException($"TenantGateway:{name}: duplicate or ambiguous route patterns.");
            }
        }
        if (ids.Count == 0) throw new InvalidOperationException("TenantGateway requires at least one enabled tenant.");
    }

    private static bool IsOrigin(string value) => Uri.TryCreate(value, UriKind.Absolute, out var u) &&
        (u.Scheme == "http" || u.Scheme == "https") && !value.Contains('*') &&
        u.AbsolutePath == "/" && u.UserInfo.Length == 0 && u.Query.Length == 0 && u.Fragment.Length == 0;
}

public sealed class GatewayTenantOptions
{
    public bool Enabled { get; set; } = true;
    public Guid TenantId { get; set; }
    public string[] PublicHosts { get; set; } = [];
    public string BackendAddress { get; set; } = "";
    public string BackendTenantOrigin { get; set; } = "";
    public string SharedKey { get; set; } = "";
    public int PermitLimit { get; set; } = 500;
    public string[] AllowedOrigins { get; set; } = [];
    public string[] ApiProfiles { get; set; } = [];
    public List<GatewayApiRoute> AdditionalRoutes { get; set; } = [];
}

public sealed class GatewayApiRoute
{
    public string Path { get; set; } = "";
    public string[] Methods { get; set; } = [];
}
