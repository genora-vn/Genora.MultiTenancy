using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.HttpApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Genora.MultiTenancy.Hlg;

// Real MVC routes / query / JSON binding, without booting production Web startup or migrations.
public class HlgRegistrationApiTests : IDisposable
{
    private readonly IHlgProfileAppService _profiles = Substitute.For<IHlgProfileAppService>();
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public HlgRegistrationApiTests()
    {
        _server = new TestServer(new WebHostBuilder().ConfigureServices(s =>
        {
            s.AddLogging();
            s.AddControllers().AddApplicationPart(typeof(HoaLinhGamificationController).Assembly);
            s.AddSingleton(_profiles);
            s.AddSingleton(Substitute.For<IZaloApiClient>());
            s.AddSingleton(Substitute.For<IHlgKnowledgeAppService>());
            s.AddSingleton(Substitute.For<IHlgGameAppService>());
            s.AddSingleton(Substitute.For<IHlgRewardAppService>());
            s.AddSingleton(Substitute.For<IHlgRankingAppService>());
            s.AddSingleton(Substitute.For<IHlgBrandAppService>());
        }).Configure(app => { app.UseRouting(); app.UseEndpoints(e => e.MapControllers()); }));
        _client = _server.CreateClient();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0900000001")]
    public async Task Check_Endpoint_Binds_Employee_Phone_And_Optional_Owner_Phone(string? owner)
    {
        _profiles.CheckCustomerAsync("0900000002", owner, Arg.Any<CancellationToken>())
            .Returns(new HlgCustomerCheckDto { Phone = "0900000002", PharmaPhone = owner ?? "0900000002", CanRegister = true });
        var response = await _client.GetAsync("/api/mini-app/hlg/auth/0900000002" + (owner == null ? "" : "?pharmaPhone=" + owner));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("canRegister").GetBoolean().ShouldBeTrue();
        body.GetProperty("data").GetProperty("maxLinkedAccounts").GetInt32().ShouldBe(5);
        await _profiles.Received(1).CheckCustomerAsync("0900000002", owner, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("DmsCustomerNotFound", 404, HlgRegistrationRules.DmsCustomerNotFound)]
    [InlineData("OwnerRequired", 403, HlgRegistrationRules.OwnerRequired)]
    [InlineData("AccountLimitReached", 409, HlgRegistrationRules.AccountLimitReached)]
    [InlineData("DmsUnavailable", 503, "DMS unavailable")]
    public async Task Both_Endpoints_Return_Expected_Hlg_Error_Envelope(string code, int error, string message)
    {
        _profiles.CheckCustomerAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<HlgCustomerCheckDto>(new UserFriendlyException(message, "HlgRegistration:" + code)));
        _profiles.UpsertCustomerAsync(Arg.Any<HlgCustomerUpsertPayloadDto>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<GamificationUserDto>(new UserFriendlyException(message, "HlgRegistration:" + code)));
        foreach (var response in new[] {
            await _client.GetAsync("/api/mini-app/hlg/auth/0900000002?pharmaPhone=0900000001"),
            await _client.PostAsJsonAsync("/api/mini-app/hlg/customer/upsert", new { phone = "0900000002", pharmaPhone = "0900000001", customerCode = "DMS02" }) })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("error").GetInt32().ShouldBe(error);
            body.GetProperty("message").GetString().ShouldBe(message);
        }
    }

    [Fact]
    public async Task Upsert_Binds_New_Fields_And_Returns_Distinct_Local_And_Dms_Codes()
    {
        _profiles.UpsertCustomerAsync(Arg.Any<HlgCustomerUpsertPayloadDto>(), Arg.Any<CancellationToken>())
            .Returns(new GamificationUserDto { CustomerCode = "HLGKH000001", DmsCustomerCode = "DMS02", PharmaPhone = "0900000001" });
        var response = await _client.PostAsJsonAsync("/api/mini-app/hlg/customer/upsert", new
        { phone = "0900000002", pharmaPhone = "0900000001", customerCode = "DMS02", address = "Branch address", customerType = "pharmacy" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("customerCode").GetString().ShouldBe("HLGKH000001");
        body.GetProperty("data").GetProperty("dmsCustomerCode").GetString().ShouldBe("DMS02");
        await _profiles.Received(1).UpsertCustomerAsync(Arg.Is<HlgCustomerUpsertPayloadDto>(x =>
            x.PharmaPhone == "0900000001" && x.CustomerCode == "DMS02" && x.Address == "Branch address"), Arg.Any<CancellationToken>());
    }

    public void Dispose() { _client.Dispose(); _server.Dispose(); }
}
