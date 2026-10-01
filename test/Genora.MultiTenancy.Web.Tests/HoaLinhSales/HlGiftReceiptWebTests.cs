using System;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppHoaLinhFeatures;
using Genora.MultiTenancy.HttpApi.Controllers;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.Web.Pages.HoaLinh.GiftReceipts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Genora.MultiTenancy.HoaLinhSales;

public class HlGiftReceiptWebTests
{
    private sealed class ReceiptControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            => feature.Controllers.Add(typeof(HoaLinhGiftReceiptMiniAppController).GetTypeInfo());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Local_Http_Confirm_Retry_And_History_Work_In_Host_And_Tenant_Context(bool host)
    {
        // Exercises actual MVC routing + controller + business service locally; DMS/storage are isolated fakes.
        // No real gift confirmation or database mutation is performed.
        using var fixture = new HlGiftReceiptTests();
        if (host) fixture.UseHost();
        using var server = new TestServer(new WebHostBuilder().ConfigureServices(services =>
        {
            services.AddSingleton(fixture.MiniAppService);
            services.AddControllers().ConfigureApplicationPartManager(parts =>
            {
                parts.ApplicationParts.Clear(); parts.FeatureProviders.Clear();
                parts.FeatureProviders.Add(new ReceiptControllerFeatureProvider());
            });
        }).Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }));
        using var client = server.CreateClient();
        const string route = "/api/mini-app/hl/gift-receipts";
        var input = new { phoneNumber = "0900000001", custCode = "CUST01", campaignCode = "GIFT25NAM", campaignPeriod = 1, voucherCode = "QT34" };
        using var confirmed = await client.PostAsJsonAsync(route, input);
        confirmed.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await confirmed.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().ShouldBeTrue();
        body.GetProperty("data").GetProperty("isConfirmed").GetBoolean().ShouldBeTrue();
        var id = body.GetProperty("data").GetProperty("id").GetString();

        using var retry = await client.PostAsJsonAsync(route, input);
        retry.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await retry.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetString().ShouldBe(id);
        using var history = await client.GetAsync(route + "?phoneNumber=0900000001&custCode=CUST01");
        history.StatusCode.ShouldBe(HttpStatusCode.OK);
        var data = (await history.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
        data.GetProperty("totalCount").GetInt32().ShouldBe(1);
        data.GetProperty("items")[0].GetProperty("id").GetString().ShouldBe(id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Granted_Host_Or_Tenant_Can_Open_Page_With_Excel(bool host)
    {
        var tenant = Substitute.For<ICurrentTenant>(); tenant.IsAvailable.Returns(!host);
        var features = Substitute.For<IFeatureChecker>(); features.IsEnabledAsync(Arg.Any<string>()).Returns(!host);
        var auth = Substitute.For<IAuthorizationService>();
        var root = host ? MultiTenancyPermissions.HostAppHlGiftReceipts.Default : MultiTenancyPermissions.AppHlGiftReceipts.Default;
        auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), root).Returns(AuthorizationResult.Success());
        auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), root + ".Export").Returns(AuthorizationResult.Success());
        var page = new IndexModel(tenant, auth, features)
            { PageContext = new PageContext { HttpContext = new DefaultHttpContext() } };
        (await page.OnGetAsync()).ShouldBeOfType<PageResult>(); page.CanExport.ShouldBeTrue();
        if (host) await features.DidNotReceive().IsEnabledAsync(Arg.Any<string>());
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, false, false)]
    public async Task Page_Enforces_Tenant_Feature_And_Correct_Host_Or_Tenant_Permission(
        bool isTenant, bool enabled, bool granted, bool allowed)
    {
        var tenant = Substitute.For<ICurrentTenant>(); tenant.IsAvailable.Returns(isTenant);
        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(Arg.Any<string>()).Returns(enabled);
        var auth = Substitute.For<IAuthorizationService>();
        var root = isTenant ? MultiTenancyPermissions.AppHlGiftReceipts.Default : MultiTenancyPermissions.HostAppHlGiftReceipts.Default;
        auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Failed());
        auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), root)
            .Returns(granted ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        var page = new IndexModel(tenant, auth, features)
            { PageContext = new PageContext { HttpContext = new DefaultHttpContext() } };
        var result = await page.OnGetAsync();
        if (allowed) result.ShouldBeOfType<PageResult>(); else result.ShouldBeOfType<ForbidResult>();
        page.CanExport.ShouldBeFalse(); // Read permission alone must not enable Excel.
    }

    [Fact]
    public async Task MiniApp_Confirmation_And_History_Expose_Confirmed_State_In_Existing_Envelope()
    {
        var service = Substitute.For<IMiniAppHlGiftReceiptService>();
        var receipt = new HlGiftReceiptDto { Id = Guid.NewGuid(), Status = HlGiftReceiptStatus.Confirmed };
        service.ConfirmAsync(Arg.Any<HlConfirmGiftInput>()).Returns(receipt);
        service.GetHistoryAsync(Arg.Any<HlGiftReceiptHistoryInput>()).Returns(new PagedResultDto<HlGiftReceiptDto>(1, new[] { receipt }));
        var controller = new HoaLinhGiftReceiptMiniAppController(service);
        var confirmed = (await controller.Confirm(new HlConfirmGiftInput())).ShouldBeOfType<OkObjectResult>()
            .Value.ShouldBeOfType<HlApiResult<HlGiftReceiptDto>>();
        confirmed.Success.ShouldBeTrue(); confirmed.Data!.IsConfirmed.ShouldBeTrue();
        var history = (await controller.History(new HlGiftReceiptHistoryInput())).ShouldBeOfType<OkObjectResult>()
            .Value.ShouldBeOfType<HlApiResult<PagedResultDto<HlGiftReceiptDto>>>();
        history.Data!.Items[0].Id.ShouldBe(receipt.Id);
    }

    [Fact]
    public async Task MiniApp_Business_Failure_Returns_Code_And_Does_Not_Confirm()
    {
        var service = Substitute.For<IMiniAppHlGiftReceiptService>();
        service.ConfirmAsync(Arg.Any<HlConfirmGiftInput>()).Returns(_ =>
            Task.FromException<HlGiftReceiptDto>(new UserFriendlyException("Sai chi nhánh", "HlGiftReceipt:BranchNotFound")));
        var controller = new HoaLinhGiftReceiptMiniAppController(service);
        var response = (await controller.Confirm(new HlConfirmGiftInput())).ShouldBeOfType<BadRequestObjectResult>()
            .Value.ShouldBeOfType<HlApiResult<object>>();
        response.Success.ShouldBeFalse(); response.Error.ShouldBe("HlGiftReceipt:BranchNotFound");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public void Installed_Abp_Generator_Resolves_Admin_Proxy_Used_By_Page()
    {
        var generator = new Volo.Abp.Http.ProxyScripting.Generators.JQuery.JQueryProxyScriptGenerator(
            Microsoft.Extensions.Options.Options.Create(new Volo.Abp.Http.ProxyScripting.Generators.JQuery.DynamicJavaScriptProxyOptions()));
        var method = generator.GetType().GetMethod("GetNormalizedTypeName",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)!;
        ((string)method.Invoke(generator, new object[] { typeof(HlGiftReceiptAdminAppService).FullName! })!)
            .ShouldBe("genora.multiTenancy.appServices.hoaLinh.hlGiftReceiptAdmin");
    }
}
