using System.Net;
using System.Text.Json;
using Genora.MultiTenancy.Gateway;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Genora.MultiTenancy.Gateway.Tests;

public class MultiTenantGatewayTests
{
    internal const string TenantB = "0cc49c56-7717-4f9d-ab7d-7461e3834747";
    internal const string KeyB = "test-hlg-012345678901234567890123456789";
    private const string Prefix = "TenantGateway:Tenants:";

    internal static Dictionary<string, string?> Settings(string backend, int limit)
    {
        var result = new Dictionary<string, string?>();
        foreach (var name in new[] { "hl25", "hlg" })
        {
            var p = Prefix + name + ":";
            result[p + "TenantId"] = name == "hl25" ? GatewayFixture.Tenant : TenantB;
            result[p + "PublicHosts:0"] = name + ".example.test";
            result[p + "BackendAddress"] = backend;
            result[p + "BackendTenantOrigin"] = "https://" + name + ".example.test";
            result[p + "SharedKey"] = name == "hl25" ? GatewayFixture.Key : KeyB;
            result[p + "AllowedOrigins:0"] = name == "hl25" ? GatewayFixture.Origin : "https://hlg-mini.example.test";
            result[p + "PermitLimit"] = (name == "hl25" ? limit : limit + 1).ToString();
            result[p + "ApiProfiles:0"] = name == "hl25" ? "Hl25" : "Hlg";
        }
        result[Prefix + "hl25:PublicHosts:1"] = "hl25-alias.example.test";
        return result;
    }

    [Fact]
    public async Task Different_Tenants_Have_Independent_Budgets_And_Response_Contracts()
    {
        await using var f = await GatewayFixture.StartAsync(2, Settings);
        var paths = new[] { "http://hl25.example.test/api/mini-app/hl25/config", "http://hl25.example.test/api/mini-app/hl25/gifts",
            "http://hl25.example.test/api/mini-app/hl25/frames/campaigns", "http://hlg.example.test/api/mini-app/hlg/games",
            "http://hlg.example.test/api/mini-app/hlg/rewards", "http://hlg.example.test/api/mini-app/hlg/ranking/event",
            "http://hlg.example.test/api/mini-app/hlg/knowledge/categories" };
        var responses = await Task.WhenAll(paths.Select(p => f.Client.GetAsync(p)));
        Assert.Equal(2, responses.Take(3).Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(3, responses.Skip(3).Count(r => r.StatusCode == HttpStatusCode.OK));
        var a = Assert.Single(responses.Take(3), r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var b = Assert.Single(responses.Skip(3), r => r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.Contains("Hl25:RateLimitExceeded", await a.Content.ReadAsStringAsync());
        using var hlg = JsonDocument.Parse(await b.Content.ReadAsStringAsync());
        Assert.Equal(429, hlg.RootElement.GetProperty("error").GetInt32());
        Assert.False(hlg.RootElement.TryGetProperty("success", out _));
        Assert.Equal(5, f.Requests.Count);
    }

    [Fact]
    public async Task Aliases_And_Multiple_Profiles_Of_One_Tenant_Share_One_Budget()
    {
        await using var f = await GatewayFixture.StartAsync(2, (backend, limit) =>
        {
            var s = Settings(backend, limit); s[Prefix + "hl25:ApiProfiles:1"] = "Hlg"; return s;
        });
        using var first = await f.Client.GetAsync("/api/mini-app/hl25/config");
        using var second = await f.Client.GetAsync("http://hl25-alias.example.test/api/mini-app/hlg/games");
        using var blocked = await f.Client.GetAsync("/api/mini-app/hl25/gifts");
        using var separate = await f.Client.GetAsync("http://hlg.example.test/api/mini-app/hlg/games");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal(HttpStatusCode.OK, separate.StatusCode);
        Assert.Equal(2, f.Requests.Count(r => r.Headers["tenant"] == GatewayFixture.Tenant));
        Assert.All(f.Requests.Where(r => r.Headers["tenant"] == GatewayFixture.Tenant), r => Assert.Equal("hl25.example.test", r.Host));
    }

    [Fact]
    public async Task Same_Module_On_Two_Tenants_Is_Selected_By_Host_And_Uses_Separate_Quota()
    {
        await using var f = await GatewayFixture.StartAsync(1, (backend, limit) =>
        {
            var s = Settings(backend, limit); s[Prefix + "hlg:ApiProfiles:0"] = "Hl25"; return s;
        });
        using var a = await f.Client.GetAsync("/api/mini-app/hl25/config");
        using var blocked = await f.Client.GetAsync("/api/mini-app/hl25/config");
        using var b = await f.Client.GetAsync("http://hlg.example.test/api/mini-app/hl25/config");
        Assert.Equal(HttpStatusCode.OK, a.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal(HttpStatusCode.OK, b.StatusCode);
        Assert.Contains(f.Requests, r => r.Headers["tenant"] == TenantB && r.Host == "hlg.example.test");
    }

    [Theory]
    [InlineData("host.example.test", "/api/mini-app/hl25/config")]
    [InlineData("unknown.example.test", "/api/mini-app/hlg/games")]
    [InlineData("hl25.example.test", "/api/mini-app/hlg/games")]
    [InlineData("hlg.example.test", "/api/mini-app/hlg/admin/rewards")]
    public async Task Unconfigured_Host_Or_Module_Is_Not_Proxied_Even_With_Forged_Tenant_Headers(string host, string path)
    {
        await using var f = await GatewayFixture.StartAsync(500, Settings);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://" + host + path);
        request.Headers.Add("X-Forwarded-Host", "hl25.example.test");
        request.Headers.Add("tenant", GatewayFixture.Tenant);
        using var response = await f.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(f.Requests);
    }

    [Fact]
    public async Task Forged_Headers_Cannot_Change_The_Routed_Tenant_Or_Key()
    {
        await using var f = await GatewayFixture.StartAsync(500, Settings);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/mini-app/hl25/config?tenant=" + TenantB);
        request.Headers.Add(TenantGatewayExtensions.TenantHeader, TenantB);
        request.Headers.Add(TenantGatewayExtensions.KeyHeader, KeyB);
        request.Headers.Add(Hl25GatewayExtensions.GatewayKeyHeader, "wrong");
        using var response = await f.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var seen = Assert.Single(f.Requests);
        Assert.Equal(GatewayFixture.Tenant, seen.Headers[TenantGatewayExtensions.TenantHeader]);
        Assert.Equal(GatewayFixture.Tenant, seen.Query["tenant"]);
        Assert.Equal(GatewayFixture.Key, seen.Headers[TenantGatewayExtensions.KeyHeader]);
        Assert.False(seen.Headers.ContainsKey(Hl25GatewayExtensions.GatewayKeyHeader));
    }

    [Fact]
    public async Task Each_Tenant_Uses_Its_Configured_Backend_Destination()
    {
        var builder = WebApplication.CreateBuilder(); builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var backendB = builder.Build();
        backendB.Run(ctx => ctx.Response.WriteAsync("backend-b"));
        await backendB.StartAsync();
        await using var f = await GatewayFixture.StartAsync(500, (backend, limit) =>
        {
            var s = Settings(backend, limit); s[Prefix + "hlg:BackendAddress"] = backendB.Urls.Single(); return s;
        });
        using var response = await f.Client.GetAsync("http://hlg.example.test/api/mini-app/hlg/games");
        Assert.Equal("backend-b", await response.Content.ReadAsStringAsync());
        Assert.Empty(f.Requests);
    }

    [Fact]
    public async Task Cors_Is_Isolated_And_Works_On_Hlg_429()
    {
        await using var f = await GatewayFixture.StartAsync(1, Settings);
        f.Client.DefaultRequestHeaders.Add("Origin", GatewayFixture.Origin);
        using var wrongOrigin = await f.Client.GetAsync("http://hlg.example.test/api/mini-app/hlg/games");
        Assert.False(wrongOrigin.Headers.Contains("Access-Control-Allow-Origin"));
        f.Client.DefaultRequestHeaders.Remove("Origin");
        f.Client.DefaultRequestHeaders.Add("Origin", "https://hlg-mini.example.test");
        using var accepted = await f.Client.GetAsync("http://hlg.example.test/api/mini-app/hlg/games");
        using var rejected = await f.Client.GetAsync("http://hlg.example.test/api/mini-app/hlg/rewards");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal("https://hlg-mini.example.test", Assert.Single(rejected.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.NotNull(rejected.Headers.RetryAfter);
    }

    [Fact]
    public async Task Additional_Explicit_MiniApp_Routes_Need_No_Code_Change()
    {
        await using var f = await GatewayFixture.StartAsync(500, (backend, limit) =>
        {
            var s = Settings(backend, limit);
            s[Prefix + "hlg:AdditionalRoutes:0:Path"] = "/api/mini-app/other/items/{id:guid}";
            s[Prefix + "hlg:AdditionalRoutes:0:Methods:0"] = "GET"; return s;
        });
        using var response = await f.Client.GetAsync("http://hlg.example.test/api/mini-app/other/items/" + Guid.NewGuid());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TenantB, Assert.Single(f.Requests).Headers["tenant"]);
    }

    [Theory]
    [InlineData("hlg:TenantId", GatewayFixture.Tenant)]
    [InlineData("hlg:PublicHosts:0", "hl25.example.test")]
    [InlineData("hlg:PublicHosts:0", "*.example.test")]
    [InlineData("hlg:PublicHosts:0", "https://hlg.example.test")]
    [InlineData("hlg:SharedKey", GatewayFixture.Key)]
    [InlineData("hlg:PermitLimit", "0")]
    [InlineData("hlg:ApiProfiles:0", "unknown")]
    [InlineData("hlg:AdditionalRoutes:0:Path", "/api/mini-app/hlg/{**all}")]
    [InlineData("hlg:AdditionalRoutes:0:Path", "/api/mini-app/hlg/admin")]
    [InlineData("hlg:AdditionalRoutes:0:Path", "/api/mini-app/hlg/games")]
    public void Invalid_Or_Ambiguous_Tenant_Routing_Fails_Startup(string key, string value)
    {
        var s = Settings("http://127.0.0.1:1", 500); s[Prefix + key] = value;
        if (key.Contains("AdditionalRoutes")) s[Prefix + "hlg:AdditionalRoutes:0:Methods:0"] = "GET";
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddTenantGateway(new ConfigurationBuilder().AddInMemoryCollection(s).Build()));
    }

    [Fact]
    public void Explicit_Tenant_Quotas_Allow_500_300_And_Other_Positive_Values()
    {
        var s = Settings("http://127.0.0.1:1", 500); s[Prefix + "hlg:PermitLimit"] = "300";
        var options = TenantGatewayOptions.Load(new ConfigurationBuilder().AddInMemoryCollection(s).Build());
        Assert.Equal(500, options.Tenants["hl25"].PermitLimit);
        Assert.Equal(300, options.Tenants["hlg"].PermitLimit);
        options.Tenants["hlg"].PermitLimit = 750;
        options.Validate();
    }

    [Fact]
    public void Mixed_Legacy_And_MultiTenant_Config_Fails_Instead_Of_Silently_Ignoring_A_Tenant()
    {
        var s = Settings("http://127.0.0.1:1", 500); s["Hl25Gateway:TenantId"] = GatewayFixture.Tenant;
        Assert.Throws<InvalidOperationException>(() => TenantGatewayOptions.Load(new ConfigurationBuilder().AddInMemoryCollection(s).Build()));
    }
}
