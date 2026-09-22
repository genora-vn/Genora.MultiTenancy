using System.Text.RegularExpressions;
using System.Xml.Linq;
using Genora.MultiTenancy.Gateway;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Genora.MultiTenancy.Gateway.Tests;

public class DeploymentConfigurationTests
{
    [Fact]
    public void Missing_And_Duplicate_Keys_Have_Actionable_Errors_Without_Secret_Values()
    {
        var settings = MultiTenantGatewayTests.Settings("http://127.0.0.1:8868", 500);
        settings["TenantGateway:Tenants:hlg:SharedKey"] = GatewayFixture.Key;
        var error = Assert.Throws<InvalidOperationException>(() => TenantGatewayOptions.Load(new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
        Assert.Contains("hlg:SharedKey duplicates TenantGateway:Tenants:hl25:SharedKey", error.Message);
        Assert.DoesNotContain(GatewayFixture.Key, error.Message);
        settings.Remove("TenantGateway:Tenants:hl25:SharedKey");
        error = Assert.Throws<InvalidOperationException>(() => TenantGatewayOptions.Load(new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
        Assert.Contains("hl25:SharedKey", error.Message);
        Assert.Contains("not loaded automatically", error.Message);
    }

    [Theory]
    [InlineData("Staging", "duocphamhoalinh-staging.genora.vn", "api/mini-app/hl25/config", "5088")]
    [InlineData("Staging", "hoalinh-staging.genora.vn", "api/mini-app/hlg/games", "5088")]
    [InlineData("Staging", "hoalinh-staging.genora.vn", "api/mini-app/hl25/config", "404")]
    [InlineData("Staging", "duocphamhoalinh-staging.genora.vn", "api/mini-app/hlg/games", "404")]
    [InlineData("Staging", "staging.genora.vn", "api/mini-app/hl25/config", "404")]
    [InlineData("Staging", "staging.genora.vn", "", "8868")]
    [InlineData("Staging", "staging.genora.vn", "api/abp/application-configuration", "8868")]
    [InlineData("Staging", "duocphamhoalinh-staging.genora.vn", "api/mini-app/hl25/admin", "8868")]
    [InlineData("Staging", "duocphamhoalinh-staging.genora.vn", "api/mini-app/hl25/admin/frame-creations/download-all-images", "8868")]
    [InlineData("Staging", "duocphamhoalinh-staging.genora.vn", "api/mini-app/hl25/admin-other", "5088")]
    [InlineData("Staging", "HOALINH-STAGING.GENORA.VN:443", "api/mini-app/hlg", "5088")]
    [InlineData("Staging", "hoalinh-staging.genora.vn", "signalr-hubs/hlg-live-feed", "8868")]
    [InlineData("Staging", "duocphamhoalinh-staging.genora.vn", "uploads/frame.png", "8868")]
    [InlineData("Staging", "unknown.example.test", "", "404")]
    [InlineData("Production", "duocpham-hoalinh.genora.vn", "api/mini-app/hl25/config", "5088")]
    [InlineData("Production", "hoalinh.genora.vn", "api/mini-app/hlg/games", "5088")]
    [InlineData("Production", "production.genora.vn", "api/mini-app/hlg/games", "404")]
    public void Ingress_Examples_Route_Only_The_Correct_Host_Profile_And_Preserve_Admin(string environment, string host, string path, string expected)
    {
        // Evaluate the deployed rule expressions/order. This does not substitute for real ARR/IIS UAT.
        var root = FindRepositoryRoot();
        var xml = XDocument.Load(Path.Combine(root, "docs", "tenant-gateway", "iis", $"ingress.{environment}.web.config"));
        var rule = xml.Descendants("rule").First(r => Regex.IsMatch(path, r.Element("match")!.Attribute("url")!.Value, RegexOptions.IgnoreCase) &&
            r.Descendants("add").All(c => Regex.IsMatch(host, c.Attribute("pattern")!.Value, RegexOptions.IgnoreCase)));
        var action = rule.Element("action")!;
        if (expected == "404") Assert.Equal("404", action.Attribute("statusCode")!.Value);
        else
        {
            Assert.Contains(":" + expected + "/", action.Attribute("url")!.Value);
            Assert.Equal("true", action.Attribute("appendQueryString")!.Value);
            Assert.Contains(rule.Descendants("set"), x => x.Attribute("name")?.Value == "HTTP_X_FORWARDED_HOST");
        }
        Assert.Equal("PassThrough", xml.Descendants("httpErrors").Single().Attribute("existingResponse")!.Value);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Genora.MultiTenancy.sln"))) return directory.FullName;
        throw new InvalidOperationException("Repository root is required to verify deployment templates.");
    }
}
