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

public class Hl25GatewayGuardTests
{
    [Theory]
    [InlineData(true, "target", "valid", "/api/mini-app/hl25/config", true)]
    [InlineData(true, "target", "absent", "/api/mini-app/hl25/config", false)]
    [InlineData(true, "target", "wrong", "/api/mini-app/hl25/wheel/spin", false)]
    [InlineData(true, "target", "duplicate", "/api/mini-app/hl25/config", false)]
    [InlineData(true, "other", "valid", "/api/mini-app/hl25/config", false)]
    [InlineData(true, "host", "valid", "/api/mini-app/hl25/config", false)]
    [InlineData(true, "other", "absent", "/api/mini-app/hl25/config", true)]
    [InlineData(true, "target", "absent", "/api/mini-app/hlg/config", true)]
    [InlineData(true, "target", "absent", "/api/mini-app/hl25/admin/frame-creations/download-all-images", true)]
    [InlineData(false, "target", "absent", "/api/mini-app/hl25/config", true)]
    public async Task Guard_Restricts_Only_Target_Tenant_Public_Hl25_And_Rejects_Wrong_Resolution(
        bool enabled, string resolved, string key, string path, bool expectedNext)
    {
        var id = Guid.NewGuid();
        const string secret = "test-only-012345678901234567890123456789";
        var current = Substitute.For<ICurrentTenant>();
        current.Id.Returns(resolved == "target" ? id : resolved == "other" ? Guid.NewGuid() : (Guid?)null);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = path;
        if (key != "absent")
            context.Request.Headers[Hl25GatewayGuardMiddleware.KeyHeader] = key == "valid" ? secret : "wrong";
        if (key == "duplicate") context.Request.Headers[Hl25GatewayGuardMiddleware.KeyHeader] = new[] { secret, secret };
        var called = false;
        var middleware = new Hl25GatewayGuardMiddleware(_ => { called = true; return Task.CompletedTask; },
            Options.Create(new Hl25GatewayGuardOptions { Enabled = enabled, TenantId = id, SharedKey = secret }));
        await middleware.InvokeAsync(context, current);
        Assert.Equal(expectedNext, called);
        Assert.Equal(expectedNext ? 200 : 403, context.Response.StatusCode);
        if (!expectedNext)
        {
            context.Response.Body.Position = 0;
            Assert.Contains("Hl25:GatewayRequired", await new StreamReader(context.Response.Body).ReadToEndAsync());
        }
        if (enabled && key == "valid" && resolved == "target")
            Assert.False(context.Request.Headers.ContainsKey(Hl25GatewayGuardMiddleware.KeyHeader));
    }
}
