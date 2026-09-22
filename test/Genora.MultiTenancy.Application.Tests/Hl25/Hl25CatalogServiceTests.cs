using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.AppDtos.Hl25.MiniApp;
using Genora.MultiTenancy.AppServices.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25CatalogServiceTests : IDisposable
{
    private readonly Hl25MiniAppCache _cache = new();
    private readonly IUnitOfWorkManager _uowManager = Substitute.For<IUnitOfWorkManager>();
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IFeatureChecker _features = Substitute.For<IFeatureChecker>();
    private readonly IManageImageService _images = Substitute.For<IManageImageService>();
    private readonly IAbpAuthorizationService _auth = Substitute.For<IAbpAuthorizationService>();
    private readonly IObjectMapper _mapper = Substitute.For<IObjectMapper>();
    private readonly IHttpContextAccessor _http = Substitute.For<IHttpContextAccessor>();
    private readonly ServiceProvider _provider;
    private readonly List<Func<Task>> _commits = new();
    private readonly Hl25MiniAppCacheInvalidator _invalidator;
    private readonly Hl25AppConfig _config = new(Guid.NewGuid()) { ProgramName = "program", IsActive = true };
    private readonly Hl25FrameCampaign _campaign = new(Guid.NewGuid(), "campaign") { Status = Hl25CampaignStatus.Active };
    private readonly Hl25FrameTemplate _template;
    private readonly Hl25Gift _gift = new(Guid.NewGuid(), "gift") { ImageUrl = "/gift.png", RemainingQuantity = 1, TotalQuantity = 1 };
    private readonly Hl25Participant _participant = new(Guid.NewGuid()) { ZaloUserId = "player", RemainingSpinTurns = 1 };
    private readonly IRepository<Hl25AppConfig, Guid> _configs;
    private readonly IRepository<Hl25FrameCampaign, Guid> _campaigns;
    private readonly IRepository<Hl25FrameTemplate, Guid> _templates;
    private readonly IRepository<Hl25Gift, Guid> _gifts;
    private readonly MiniAppHl25Service _mini;

    public Hl25CatalogServiceTests()
    {
        var httpContext = Substitute.For<HttpContext>();
        httpContext.Request.Returns(Substitute.For<HttpRequest>());
        _http.HttpContext.Returns(httpContext);
        _template = new Hl25FrameTemplate(Guid.NewGuid(), _campaign.Id, "template", "/frame.png") { IsActive = true };
        _configs = Repo(_config); _campaigns = Repo(_campaign); _templates = Repo(_template); _gifts = Repo(_gift);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        var uow = Substitute.For<IUnitOfWork>();
        _uowManager.Current.Returns(uow);
        _uowManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(uow);
        uow.When(x => x.OnCompleted(Arg.Any<Func<Task>>())).Do(x => _commits.Add(x.Arg<Func<Task>>()));
        uow.CompleteAsync(Arg.Any<CancellationToken>()).Returns(_ => Commit());
        _provider = new ServiceCollection()
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .AddSingleton(_tenant).AddSingleton<IAuthorizationService>(_auth).AddSingleton(_mapper)
            .AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance).BuildServiceProvider();
        _invalidator = new Hl25MiniAppCacheInvalidator(_cache, _uowManager);
        var wheel = new Hl25WheelConfig(Guid.NewGuid()) { IsActive = true };
        var slot = new Hl25WheelSlot(Guid.NewGuid(), wheel.Id) { GiftId = _gift.Id, WinRate = 100 };
        _mini = new MiniAppHl25Service(_configs, Repo(_participant), _campaigns, _templates,
            Substitute.For<IRepository<Hl25FrameCreation, Guid>>(), Substitute.For<IRepository<Hl25SpinTurnLog, Guid>>(),
            Repo(wheel), Repo(slot), _gifts, Substitute.For<IRepository<Hl25SpinLog, Guid>>(),
            _uowManager, _images, _http, new ConfigurationBuilder().Build(), _cache)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    }

    [Fact]
    public async Task Four_Read_Endpoints_Reuse_Data_And_Keep_Request_Origin_Out_Of_Cache()
    {
        _http.HttpContext!.Request.Scheme.Returns("https");
        _http.HttpContext.Request.Host.Returns(new HostString("one.test"));
        (await _mini.GetGiftsAsync()).Single().ImageUrl.ShouldBe("https://one.test/gift.png");
        _http.HttpContext.Request.Host.Returns(new HostString("two.test"));
        var gifts = await _mini.GetGiftsAsync();
        gifts.Single().ImageUrl.ShouldBe("https://two.test/gift.png");
        JsonSerializer.Serialize(gifts).ShouldNotContain("RemainingQuantity");
        for (var i = 0; i < 2; i++)
        {
            (await _mini.GetConfigAsync()).ProgramName.ShouldBe("program");
            (await _mini.GetFrameCampaignsAsync()).Single().TemplateCount.ShouldBe(1);
            (await _mini.GetFrameTemplatesAsync(_campaign.Id)).Single().Id.ShouldBe(_template.Id);
            (await _mini.GetFrameTemplatesAsync(Guid.Empty)).ShouldBeEmpty();
        }
        await _configs.Received(1).GetQueryableAsync();
        await _campaigns.Received(1).GetQueryableAsync();
        await _templates.Received(3).GetQueryableAsync(); // counts + two distinct campaign variants
        await _gifts.Received(1).GetQueryableAsync();
    }

    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public async Task Gift_Admin_Writes_Invalidate_Only_After_Commit(string operation)
    {
        await _mini.GetGiftsAsync();
        var admin = new Hl25GiftAppService(_gifts, _tenant, _features, _images, _invalidator)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        var input = new CreateUpdateHl25GiftDto { Name = "changed", TotalQuantity = 10, RemainingQuantity = 10 };
        if (operation == "create") await admin.CreateAsync(input);
        else if (operation == "update") await admin.UpdateAsync(_gift.Id, input);
        else await admin.DeleteAsync(_gift.Id);
        await _mini.GetGiftsAsync();
        await _gifts.Received(1).GetQueryableAsync();
        await Commit();
        await _mini.GetGiftsAsync();
        await _gifts.Received(2).GetQueryableAsync();
    }

    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public async Task Template_Admin_Writes_Invalidate_Counts_And_All_Filter_Variants(string operation)
    {
        var oldCampaign = _campaign.Id; var newCampaign = Guid.NewGuid();
        await _mini.GetFrameCampaignsAsync();
        await _mini.GetFrameTemplatesAsync(null);
        await _mini.GetFrameTemplatesAsync(oldCampaign);
        await _mini.GetFrameTemplatesAsync(newCampaign);
        var admin = new Hl25FrameTemplateAppService(_templates, _tenant, _features, _images, _invalidator)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        var input = new CreateUpdateHl25FrameTemplateDto { CampaignId = newCampaign, Name = "moved", ImageUrl = "/new.png" };
        if (operation == "create") await admin.CreateAsync(input);
        else if (operation == "update") await admin.UpdateAsync(_template.Id, input);
        else await admin.DeleteAsync(_template.Id);
        await Commit();
        await _mini.GetFrameCampaignsAsync();
        await _mini.GetFrameTemplatesAsync(null);
        var oldItems = await _mini.GetFrameTemplatesAsync(oldCampaign);
        var newItems = await _mini.GetFrameTemplatesAsync(newCampaign);
        await _campaigns.Received(2).GetQueryableAsync();
        await _templates.Received(8).GetQueryableAsync();
        if (operation == "update") { oldItems.ShouldBeEmpty(); newItems.Single().Name.ShouldBe("moved"); }
    }

    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public async Task Campaign_Admin_Writes_Invalidate_Campaigns_And_Templates(string operation)
    {
        await _mini.GetFrameCampaignsAsync(); await _mini.GetFrameTemplatesAsync(null);
        var admin = new Hl25FrameCampaignAppService(_campaigns, _templates, _tenant, _features, _invalidator)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        var input = new CreateUpdateHl25FrameCampaignDto { Name = "changed", Status = Hl25CampaignStatus.Active };
        if (operation == "create") await admin.CreateAsync(input);
        else if (operation == "update") await admin.UpdateAsync(_campaign.Id, input);
        else await admin.DeleteAsync(_campaign.Id);
        await Commit();
        await _mini.GetFrameCampaignsAsync(); await _mini.GetFrameTemplatesAsync(null);
        await _campaigns.Received(2).GetQueryableAsync();
        await _templates.Received(4).GetQueryableAsync();
    }

    [Fact]
    public async Task Config_Update_Refreshes_Public_Config_After_Commit()
    {
        await _mini.GetConfigAsync();
        var admin = new Hl25AppConfigAppService(_configs, _images, _features, _invalidator)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        await admin.UpdateAsync(new CreateUpdateHl25AppConfigDto());
        _config.ProgramName = "saved";
        (await _mini.GetConfigAsync()).ProgramName.ShouldBe("program");
        await Commit();
        (await _mini.GetConfigAsync()).ProgramName.ShouldBe("saved");
        await _configs.Received(3).GetQueryableAsync(); // initial read + admin lookup + refreshed public read
    }

    [Fact]
    public async Task Config_Auto_Create_Invalidates_A_Previously_Empty_Public_Config()
    {
        _configs.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<Hl25AppConfig>().AsQueryable()));
        (await _mini.GetConfigAsync()).IsActive.ShouldBeFalse();
        var admin = new Hl25AppConfigAppService(_configs, _images, _features, _invalidator)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        await admin.GetAsync();
        await Commit();
        _configs.GetQueryableAsync().Returns(Task.FromResult(new[] { _config }.AsQueryable()));
        (await _mini.GetConfigAsync()).IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Unauthorized_Admin_Cannot_Write_Or_Invalidate()
    {
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Failed());
        var admin = new Hl25GiftAppService(_gifts, _tenant, _features, _images, _invalidator)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        await Should.ThrowAsync<AbpAuthorizationException>(() => admin.DeleteAsync(_gift.Id));
        _commits.ShouldBeEmpty();
        await _gifts.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Disabled_Tenant_Feature_Cannot_Write_Or_Invalidate()
    {
        _tenant.Id.Returns(Guid.NewGuid()); _tenant.IsAvailable.Returns(true);
        _features.IsEnabledAsync(Arg.Any<string>()).Returns(false);
        var admin = new Hl25GiftAppService(_gifts, _tenant, _features, _images, _invalidator)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        await Should.ThrowAsync<AbpAuthorizationException>(() => admin.DeleteAsync(_gift.Id));
        _commits.ShouldBeEmpty();
    }

    [Fact]
    public async Task Spin_Reads_Live_Stock_And_Last_Gift_Refreshes_Public_Status()
    {
        (await _mini.GetGiftsAsync()).Single().Status.ShouldBe(Hl25GiftStatus.Available);
        (await _mini.SpinAsync(new Hl25SpinRequest { ZaloUserId = "player" })).Won.ShouldBeTrue();
        _gift.RemainingQuantity.ShouldBe(0);
        (await _mini.GetGiftsAsync()).Single().Status.ShouldBe(Hl25GiftStatus.OutOfStock);
        await _gifts.Received(1).FindAsync(_gift.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Warm_Catalog_Does_Not_Allow_Winning_A_Gift_With_No_Live_Stock()
    {
        await _mini.GetGiftsAsync();
        _gift.RemainingQuantity = 0;
        (await _mini.SpinAsync(new Hl25SpinRequest { ZaloUserId = "player" })).Won.ShouldBeFalse();
        _gift.RemainingQuantity.ShouldBe(0);
    }

    private async Task Commit()
    {
        foreach (var callback in _commits.ToArray()) await callback();
        _commits.Clear();
    }

    private static IRepository<T, Guid> Repo<T>(T item) where T : class, IEntity<Guid>
    {
        var repo = Substitute.For<IRepository<T, Guid>>();
        repo.GetQueryableAsync().Returns(Task.FromResult(new[] { item }.AsQueryable()));
        repo.GetAsync(item.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(item);
        repo.FindAsync(item.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(item);
        repo.InsertAsync(Arg.Any<T>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(x => x.Arg<T>());
        repo.UpdateAsync(Arg.Any<T>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(x => x.Arg<T>());
        return repo;
    }

    public void Dispose() { _provider.Dispose(); _cache.Dispose(); }
}
