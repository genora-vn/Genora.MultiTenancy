using System;
using System.IO;
using System.Threading.Tasks;
using Genora.MultiTenancy.Web.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class TenantGatewayGuardTests
{
    private static readonly Guid A = Guid.Parse("5c8b8db6-b431-4739-85e7-dc21cd82b3ab");
    private static readonly Guid B = Guid.Parse("ad7e99e0-cf58-498c-b970-3ea5304b0b72");
    private const string KeyA = "test-only-a-012345678901234567890123456";
    private const string KeyB = "test-only-b-012345678901234567890123456";

    private static TenantGatewayGuardOptions Settings() => new()
    {
        Enabled = true, Tenants = new()
        {
            ["a"] = new() { TenantId = A, SharedKey = KeyA, PathPrefixes = new[] { "/api/mini-app/hl25" }, ExcludedPathPrefixes = new[] { "/api/mini-app/hl25/admin" } },
            ["b"] = new() { TenantId = B, SharedKey = KeyB, PathPrefixes = new[] { "/api/mini-app/hlg" } }
        }
    };

    [Theory]
    [InlineData("a", "a", "a", "/api/mini-app/hl25/config", true)]
    [InlineData("b", "b", "b", "/api/mini-app/hlg/games", true)]
    [InlineData("a", "b", "b", "/api/mini-app/hl25/config", false)]
    [InlineData("b", "b", "a", "/api/mini-app/hlg/games", false)]
    [InlineData("a", "none", "none", "/api/mini-app/hl25/config", false)]
    [InlineData("b", "none", "none", "/api/mini-app/hlg/games", false)]
    [InlineData("host", "a", "a", "/api/mini-app/hl25/config", false)]
    [InlineData("other", "a", "a", "/api/mini-app/hl25/config", false)]
    [InlineData("other", "none", "none", "/api/mini-app/hl25/config", true)]
    [InlineData("a", "a", "a", "/api/mini-app/hlg/games", false)]
    [InlineData("a", "none", "none", "/api/mini-app/hl25/admin/export", true)]
    [InlineData("a", "a", "a", "/api/mini-app/hl25/admin/export", false)]
    [InlineData("a", "none", "none", "/Hl25/Participants", true)]
    [InlineData("host", "none", "none", "/api/abp/application-configuration", true)]
    public async Task Enforces_Key_Resolved_Tenant_Claim_And_Configured_Path(string current, string claim, string key, string path, bool allowed)
    {
        var options = Settings(); Assert.True(options.IsValid());
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(current == "a" ? A : current == "b" ? B : current == "host" ? (Guid?)null : Guid.NewGuid());
        var ctx = new DefaultHttpContext(); ctx.Request.Path = path; ctx.Response.Body = new MemoryStream();
        if (claim != "none") ctx.Request.Headers[TenantGatewayGuardMiddleware.TenantHeader] = (claim == "a" ? A : B).ToString();
        if (key != "none") ctx.Request.Headers[TenantGatewayGuardMiddleware.KeyHeader] = key == "a" ? KeyA : KeyB;
        var called = false;
        var middleware = new TenantGatewayGuardMiddleware(_ => { called = true; return Task.CompletedTask; }, Options.Create(options));
        await middleware.InvokeAsync(ctx, tenant);
        Assert.Equal(allowed, called);
        Assert.Equal(allowed ? 200 : 403, ctx.Response.StatusCode);
        Assert.False(ctx.Request.Headers.ContainsKey(TenantGatewayGuardMiddleware.KeyHeader));
        Assert.False(ctx.Request.Headers.ContainsKey(TenantGatewayGuardMiddleware.TenantHeader));
        if (!allowed && path.StartsWith("/api/mini-app/hlg"))
        {
            ctx.Response.Body.Position = 0;
            Assert.Contains("\"error\":403", await new StreamReader(ctx.Response.Body).ReadToEndAsync());
        }
    }

    [Fact]
    public void Invalid_Guard_Configuration_Is_Rejected()
    {
        var o = Settings(); o.Tenants["b"].TenantId = A; Assert.False(o.IsValid());
        o = Settings(); o.Tenants["b"].SharedKey = KeyA; Assert.False(o.IsValid());
        o = Settings(); o.Tenants["b"].PathPrefixes = new[] { "/api/mini-app/hlg/../hl25" }; Assert.False(o.IsValid());
        o = Settings(); o.Tenants["b"].ExcludedPathPrefixes = new[] { "/api/mini-app/hlg" }; Assert.False(o.IsValid());
        o = new(); Assert.True(o.IsValid());
    }

    [Fact]
    public async Task Duplicate_Headers_Are_Rejected_And_Disabled_Guard_Preserves_Existing_Behavior()
    {
        var ctx = new DefaultHttpContext(); ctx.Request.Path = "/api/mini-app/hlg/games"; ctx.Response.Body = new MemoryStream();
        ctx.Request.Headers[TenantGatewayGuardMiddleware.KeyHeader] = new[] { KeyB, KeyB };
        ctx.Request.Headers[TenantGatewayGuardMiddleware.TenantHeader] = B.ToString();
        var tenant = Substitute.For<ICurrentTenant>(); tenant.Id.Returns(B);
        var called = false;
        var middleware = new TenantGatewayGuardMiddleware(_ => { called = true; return Task.CompletedTask; }, Options.Create(Settings()));
        await middleware.InvokeAsync(ctx, tenant); Assert.False(called); Assert.Equal(403, ctx.Response.StatusCode);
        var disabled = Settings(); disabled.Enabled = false;
        middleware = new TenantGatewayGuardMiddleware(_ => { called = true; return Task.CompletedTask; }, Options.Create(disabled));
        await middleware.InvokeAsync(new DefaultHttpContext(), tenant); Assert.True(called);
    }
}
