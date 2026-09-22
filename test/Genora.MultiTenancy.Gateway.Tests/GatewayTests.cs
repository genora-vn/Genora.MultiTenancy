using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Genora.MultiTenancy.Gateway;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Genora.MultiTenancy.Gateway.Tests;

public sealed class GatewayTests
{
    [Fact]
    public async Task Budget_Is_Shared_Across_Routes_And_Tenant_Spoofing_Cannot_Create_Another_Bucket()
    {
        await using var fixture = await GatewayFixture.StartAsync(2);
        var paths = new[] { "config", "gifts", "frames/campaigns", "frames/templates", "wheel", "me/frames" };
        var responses = await Task.WhenAll(paths.Select((path, i) => fixture.Client.GetAsync(
            "/api/mini-app/hl25/" + path + "?tenant=" + Guid.NewGuid() + "&__tenant=" + i)));
        Assert.Equal(2, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(4, responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests));
        Assert.Equal(2, fixture.Requests.Count);
        foreach (var response in responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests))
        {
            Assert.NotNull(response.Headers.RetryAfter);
            Assert.True(response.Headers.CacheControl!.NoStore);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Hl25:RateLimitExceeded", body);
            Assert.Contains("\"success\":false", body);
        }
        await Task.Delay(1200);
        using var recovered = await fixture.Client.GetAsync("/api/mini-app/hl25/config");
        Assert.Equal(HttpStatusCode.OK, recovered.StatusCode);
    }

    [Fact]
    public async Task Proxy_Pins_Tenant_Host_Scheme_And_Key_And_Removes_Client_Tenant_And_Auth_Overrides()
    {
        await using var fixture = await GatewayFixture.StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get,
            "/api/mini-app/hl25/frames/templates?campaignId=abc&Tenant=other&tenant=third&__tenant=fake");
        request.Headers.Host = "hl25.example.test"; // Untrusted forwarded host is tested below; unmatched Host must be rejected.
        request.Headers.Add("tenant", "wrong");
        request.Headers.Add("__tenant", "wrong");
        request.Headers.Add("X-Hl25-Gateway-Key", "wrong");
        request.Headers.Add("Authorization", "Bearer wrong");
        request.Headers.Add("Cookie", "tenant=wrong; .Genora.Auth=wrong");
        request.Headers.Add("Forwarded", "host=wrong;proto=http");
        request.Headers.Add("X-Forwarded-Host", "wrong");
        request.Headers.Add("X-Forwarded-Proto", "http");
        using var response = await fixture.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var seen = Assert.Single(fixture.Requests);
        Assert.Equal("hl25.example.test", seen.Host);
        Assert.Equal("hl25.example.test", seen.Headers["X-Forwarded-Host"]);
        Assert.Equal("https", seen.Headers["X-Forwarded-Proto"]);
        Assert.Equal(GatewayFixture.Tenant, seen.Headers["tenant"]);
        Assert.Equal(GatewayFixture.Key, seen.Headers["X-Hl25-Gateway-Key"]);
        Assert.Equal(GatewayFixture.Tenant, seen.Query["tenant"]);
        Assert.Equal("abc", seen.Query["campaignId"]);
        foreach (var name in new[] { "Cookie", "Authorization", "__tenant", "Forwarded", "X-Forwarded-Prefix" })
            Assert.False(seen.Headers.ContainsKey(name), name);
        Assert.False(seen.Query.ContainsKey("__tenant"));
    }

    [Fact]
    public async Task Json_Post_Query_And_Error_Status_Pass_Through_Without_Retry()
    {
        await using var fixture = await GatewayFixture.StartAsync();
        const string body = "{\"zaloUserId\":\"test\",\"value\":123}";
        using var response = await fixture.Client.PostAsync("/api/mini-app/hl25/wheel/spin?status=503",
            new StringContent(body, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var seen = Assert.Single(fixture.Requests);
        Assert.Equal("POST", seen.Method);
        Assert.Equal(body, seen.Body);
        Assert.Contains("test-error", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Multipart_Upload_Is_Forwarded_Intact()
    {
        await using var fixture = await GatewayFixture.StartAsync();
        using var content = new MultipartFormDataContent("test-boundary");
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("test-image-data")), "file", "frame.png");
        using var response = await fixture.Client.PostAsync("/api/mini-app/hl25/upload-image", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var seen = Assert.Single(fixture.Requests);
        Assert.Contains("test-boundary", seen.Headers["Content-Type"]);
        Assert.Contains("frame.png", seen.Body);
        Assert.Contains("test-image-data", seen.Body);
    }

    [Fact]
    public async Task Cors_Works_On_Proxy_And_429_And_Preflight_Does_Not_Reach_Backend_Or_Consume_Budget()
    {
        await using var fixture = await GatewayFixture.StartAsync(1);
        fixture.Client.DefaultRequestHeaders.Add("Origin", GatewayFixture.Origin);
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/mini-app/hl25/config");
        preflight.Headers.Add("Access-Control-Request-Method", "GET");
        preflight.Headers.Add("Access-Control-Request-Headers", "content-type,tenant");
        using var preflightResponse = await fixture.Client.SendAsync(preflight);
        Assert.Equal(HttpStatusCode.NoContent, preflightResponse.StatusCode);
        Assert.Empty(fixture.Requests);
        using var ok = await fixture.Client.GetAsync("/api/mini-app/hl25/config");
        using var rejected = await fixture.Client.GetAsync("/api/mini-app/hl25/gifts");
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        foreach (var response in new[] { preflightResponse, ok, rejected })
            Assert.Equal(GatewayFixture.Origin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Equal("true", Assert.Single(rejected.Headers.GetValues("Access-Control-Allow-Credentials")));
        Assert.Contains("Retry-After", string.Join(",", rejected.Headers.GetValues("Access-Control-Expose-Headers")));
        Assert.Single(fixture.Requests);
    }

    [Fact]
    public async Task Unapproved_Origin_Does_Not_Receive_Cors_Access()
    {
        await using var fixture = await GatewayFixture.StartAsync();
        fixture.Client.DefaultRequestHeaders.Add("Origin", "https://attacker.example");
        using var response = await fixture.Client.GetAsync("/api/mini-app/hl25/config");
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory]
    [InlineData("/api/mini-app/hl25/admin/frame-creations/download-all-images")]
    [InlineData("/api/mini-app/hl25/unknown")]
    [InlineData("/api/mini-app/hlg/config")]
    [InlineData("/identity/connect/token")]
    public async Task Admin_And_Unrelated_Routes_Are_Not_Proxied(string path)
    {
        await using var fixture = await GatewayFixture.StartAsync();
        using var response = await fixture.Client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(fixture.Requests);
    }

    [Fact]
    public async Task Unsupported_Method_Is_Rejected_And_Health_Is_Independent_Of_Budget()
    {
        await using var fixture = await GatewayFixture.StartAsync(1);
        using var delete = await fixture.Client.DeleteAsync("/api/mini-app/hl25/gifts");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, delete.StatusCode);
        using var first = await fixture.Client.GetAsync("/api/mini-app/hl25/gifts");
        using var health = await fixture.Client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Single(fixture.Requests);
    }

    [Theory]
    [InlineData("Hl25Gateway:TenantId", "00000000-0000-0000-0000-000000000000")]
    [InlineData("Hl25Gateway:SharedKey", "short")]
    [InlineData("Hl25Gateway:BackendAddress", "http://localhost/api")]
    [InlineData("Hl25Gateway:BackendAddress", "http://203.0.113.10:8868/")]
    [InlineData("Hl25Gateway:BackendTenantOrigin", "https://user:pass@example.com/")]
    [InlineData("Hl25Gateway:AllowedOrigins:0", "*")]
    [InlineData("Hl25Gateway:PermitLimit", "501")]
    [InlineData("Hl25Gateway:PermitLimit", "0")]
    public void Invalid_Deployment_Settings_Fail_Startup(string key, string value)
    {
        var values = GatewayFixture.Settings("http://127.0.0.1:1", 500);
        values[key] = value;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddHl25Gateway(configuration));
    }
}

internal sealed record SeenRequest(string Method, string Host, Dictionary<string, string> Query,
    Dictionary<string, string> Headers, string Body);

internal sealed class GatewayFixture : IAsyncDisposable
{
    public const string Tenant = "a4183ce7-38f8-44de-b8ab-b506bce448e3";
    public const string Key = "test-only-012345678901234567890123456789";
    public const string Origin = "https://mini.example.test";
    private readonly WebApplication _backend;
    private readonly WebApplication _gateway;
    public HttpClient Client { get; }
    public ConcurrentQueue<SeenRequest> Requests { get; }

    private GatewayFixture(WebApplication backend, WebApplication gateway, ConcurrentQueue<SeenRequest> requests)
    {
        _backend = backend;
        _gateway = gateway;
        Requests = requests;
        Client = gateway.GetTestClient();
        Client.BaseAddress = new Uri("http://hl25.example.test");
    }

    public static Dictionary<string, string?> Settings(string backend, int limit) => new()
    {
        ["Hl25Gateway:TenantId"] = Tenant,
        ["Hl25Gateway:BackendAddress"] = backend,
        ["Hl25Gateway:BackendTenantOrigin"] = "https://hl25.example.test",
        ["Hl25Gateway:SharedKey"] = Key,
        ["Hl25Gateway:AllowedOrigins:0"] = Origin,
        ["Hl25Gateway:PermitLimit"] = limit.ToString()
    };

    public static async Task<GatewayFixture> StartAsync(int limit = 500, Func<string, int, Dictionary<string, string?>>? settingsFactory = null)
    {
        var requests = new ConcurrentQueue<SeenRequest>();
        var backendBuilder = WebApplication.CreateBuilder();
        backendBuilder.Logging.ClearProviders();
        backendBuilder.WebHost.UseUrls("http://127.0.0.1:0");
        var backend = backendBuilder.Build();
        backend.Run(async ctx =>
        {
            using var reader = new StreamReader(ctx.Request.Body);
            requests.Enqueue(new(ctx.Request.Method, ctx.Request.Host.Value ?? "",
                ctx.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase),
                ctx.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase),
                await reader.ReadToEndAsync()));
            ctx.Response.Headers.AccessControlAllowOrigin = "*";
            ctx.Response.StatusCode = ctx.Request.Query["status"] == "503" ? 503 : 200;
            await ctx.Response.WriteAsJsonAsync(new { success = ctx.Response.StatusCode == 200, error = "test-error" });
        });
        await backend.StartAsync();
        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.UseTestServer();
            builder.Configuration.AddInMemoryCollection((settingsFactory ?? Settings)(backend.Urls.Single(), limit));
            builder.Services.AddHl25Gateway(builder.Configuration);
            var gateway = builder.Build();
            gateway.UseHl25Gateway();
            await gateway.StartAsync();
            return new(backend, gateway, requests);
        }
        catch { await backend.DisposeAsync(); throw; }
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _gateway.DisposeAsync();
        await _backend.DisposeAsync();
    }
}
