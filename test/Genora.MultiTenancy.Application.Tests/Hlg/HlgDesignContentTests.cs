using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.AppServices.Hlg;
using Genora.MultiTenancy.AppServices.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;
namespace Genora.MultiTenancy.Hlg;
public class HlgDesignContentTests : IDisposable
{
    private readonly Guid _tenantId=Guid.NewGuid();
    private readonly ICurrentTenant _tenant=Substitute.For<ICurrentTenant>();
    private readonly IFeatureChecker _features=Substitute.For<IFeatureChecker>();
    private readonly IAbpAuthorizationService _auth=Substitute.For<IAbpAuthorizationService>();
    private readonly ServiceCollection _services=new();
    private readonly List<ServiceProvider> _providers=new();
    public HlgDesignContentTests() {
        _tenant.Id.Returns(_tenantId); _tenant.IsAvailable.Returns(true);
        _features.IsEnabledAsync(AppHlgFeatures.Management).Returns(true);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(),Arg.Any<object>(),Arg.Any<string>()).Returns(AuthorizationResult.Success());
        var guid=Substitute.For<IGuidGenerator>();guid.Create().Returns(_=>Guid.NewGuid());
        var clock=Substitute.For<IClock>();clock.Now.Returns(new DateTime(2026,9,19));
        var localizer=Substitute.For<IStringLocalizer>();localizer[Arg.Any<string>()].Returns(c=>new LocalizedString(c.Arg<string>(),c.Arg<string>()));
        var factory=Substitute.For<IStringLocalizerFactory>();factory.Create(Arg.Any<Type>()).Returns(localizer);
        _services.AddSingleton<ICurrentTenant>(_tenant).AddSingleton(_features).AddSingleton<IAuthorizationService>(_auth).AddSingleton(guid).AddSingleton(clock).AddSingleton(factory)
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()));
    }
    private IRepository<T,Guid> Repo<T>(params T[] rows) where T:class,IEntity<Guid> {
        var repo=Substitute.For<IRepository<T,Guid>>();
        repo.GetQueryableAsync().Returns(Task.FromResult(rows.AsQueryable()));
        repo.GetAsync(Arg.Any<Guid>(),Arg.Any<bool>(),Arg.Any<CancellationToken>()).Returns(c=>Task.FromResult(rows.Single(x=>x.Id==c.Arg<Guid>())));
        repo.AnyAsync(Arg.Any<Expression<Func<T,bool>>>(),Arg.Any<CancellationToken>()).Returns(c=>rows.Any(c.Arg<Expression<Func<T,bool>>>().Compile()));
        repo.CountAsync(Arg.Any<Expression<Func<T,bool>>>(),Arg.Any<CancellationToken>()).Returns(c=>rows.Count(c.Arg<Expression<Func<T,bool>>>().Compile()));

        repo.InsertAsync(Arg.Any<T>(),Arg.Any<bool>(),Arg.Any<CancellationToken>()).Returns(c=>c.Arg<T>());
        repo.UpdateAsync(Arg.Any<T>(),Arg.Any<bool>(),Arg.Any<CancellationToken>()).Returns(c=>c.Arg<T>());
        repo.ClearReceivedCalls(); _services.AddSingleton(repo);return repo;
    }
    private T Bind<T>(T service) where T:Volo.Abp.Application.Services.ApplicationService {
        var provider=_services.BuildServiceProvider();_providers.Add(provider);service.LazyServiceProvider=new AbpLazyServiceProvider(provider);return service;
    }
    [Theory]
    [InlineData("javascript:alert(1)")][InlineData("data:text/html,unsafe")][InlineData("//foreign.test/a")][InlineData("/\\foreign.test/a")]
    public void Unsafe_Media_And_Cta_Urls_Are_Rejected(string url) => Should.Throw<ValidationException>(()=>HlgContentValidation.Url(url));
    [Theory][InlineData("https://cdn.example.test/a.mp4")][InlineData("/uploads/tenant/a.png")]
    public void Existing_Storage_And_Generic_Video_Urls_Are_Accepted(string url) => HlgContentValidation.Url(url);
    [Fact]
    public void Product_Collections_Reject_Self_Duplicate_And_Empty_Answers() {
        var id=Guid.NewGuid();var details=new HlgProductContent { RelatedProductIds=new(){id} };
        Should.Throw<ValidationException>(()=>HlgContentValidation.Product(details,id));
        details.RelatedProductIds.Add(id);Should.Throw<ValidationException>(()=>HlgContentValidation.Product(details,null));
        details.RelatedProductIds.Clear();details.Knowledge.Add(new(){Title="FAQ",Content=" "});Should.Throw<ValidationException>(()=>HlgContentValidation.Product(details,null));
    }
    [Fact]
    public async Task Product_Save_RoundTrips_Hierarchy_Ordered_Content_And_Tenant() {
        var category=new HlgKnowledgeCategory(Guid.NewGuid(),"Industry",_tenantId);
        var brand=new HlgBrand(Guid.NewGuid(),_tenantId){CategoryId=category.Id,Name="Brand"};
        var related=new HlgProduct(Guid.NewGuid(),category.Id,"Related",_tenantId);
        var products=Repo(related);var categories=Repo(category);Repo(brand);
        var service=Bind(new HlgProductAdminAppService(products,_tenant,_features,categories));
        var input=new CreateHlgProductInput { CategoryId=category.Id,BrandId=brand.Id,Name="Product",Content="<h2>Ingredients</h2>",Details=new(){
            BadgeText="New",RelatedProductIds=new(){related.Id},Knowledge=new(){new(){Title="Question",Content="Answer"}},Media=new(){new(){Kind=HlgMediaKind.Video,Placement=HlgMediaPlacement.Hero,Url="https://example.test/movie.mp4",PosterUrl="/poster.png"},new(){Url="/banner.png",Placement=HlgMediaPlacement.Knowledge}} } };
        var result=await service.CreateAsync(input);result.BrandId.ShouldBe(brand.Id);result.Details.Media[1].Placement.ShouldBe(HlgMediaPlacement.Knowledge);result.Details.Knowledge[0].Content.ShouldBe("Answer");
        await products.Received().InsertAsync(Arg.Is<HlgProduct>(x=>x.TenantId==_tenantId && x.BrandId==brand.Id && x.DetailsJson!.Contains("Question")),true,Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Product_Rejects_Brand_From_Another_Industry() {
        var category=new HlgKnowledgeCategory(Guid.NewGuid(),"Industry",_tenantId);var brand=new HlgBrand(Guid.NewGuid(),_tenantId){CategoryId=Guid.NewGuid()};
        var products=Repo<HlgProduct>();var categories=Repo(category);Repo(brand);
        await Should.ThrowAsync<UserFriendlyException>(()=>Bind(new HlgProductAdminAppService(products,_tenant,_features,categories)).CreateAsync(new(){CategoryId=category.Id,BrandId=brand.Id,Name="Product"}));
        await products.DidNotReceive().InsertAsync(Arg.Any<HlgProduct>(),Arg.Any<bool>(),Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Forged_Cross_Tenant_Category_Is_Rejected_Even_Without_Repository_Filter() {
        var category=new HlgKnowledgeCategory(Guid.NewGuid(),"Other",Guid.NewGuid());var products=Repo<HlgProduct>();var categories=Repo(category);
        await Should.ThrowAsync<AbpAuthorizationException>(()=>Bind(new HlgProductAdminAppService(products,_tenant,_features,categories)).CreateAsync(new(){CategoryId=category.Id,Name="Product"}));
    }
    [Fact]
    public async Task Public_Catalog_Hides_Inactive_Brand_And_Cross_Tenant_Product() {
        var category=new HlgKnowledgeCategory(Guid.NewGuid(),"Industry",_tenantId);Repo(category);
        var brand=new HlgBrand(Guid.NewGuid(),_tenantId){CategoryId=category.Id,Name="Hidden",IsActive=false};Repo(brand);
        var shown=new HlgProduct(Guid.NewGuid(),category.Id,"Visible",_tenantId);
        Repo(shown,new HlgProduct(Guid.NewGuid(),category.Id,"Hidden brand",_tenantId){BrandId=brand.Id},new HlgProduct(Guid.NewGuid(),category.Id,"Other tenant",Guid.NewGuid()));
        var result=await Bind(new HlgContentAppService()).SearchProductsAsync();result.Count.ShouldBe(1);result.Single().Id.ShouldBe(shown.Id);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task Content_Admin_Denies_Missing_Permission_Or_Feature_Before_Query(bool missingPermission) {
        var repo=Repo<HlgContentItem>();if(missingPermission)_auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(),Arg.Any<object>(),Arg.Any<string>()).Returns(AuthorizationResult.Failed());else _features.IsEnabledAsync(AppHlgFeatures.Management).Returns(false);
        await Should.ThrowAsync<AbpAuthorizationException>(()=>Bind(new HlgContentAdminAppService(repo,_tenant,_features)).GetListAsync(new()));await repo.DidNotReceive().GetQueryableAsync();
    }
    [Fact]
    public async Task Content_Admin_Uses_Host_Permission_And_Host_Scope() {
        _tenant.IsAvailable.Returns(false);_tenant.Id.Returns((Guid?)null);
        var repo=Repo(new HlgContentItem(Guid.NewGuid(),null){Title="Host"},new HlgContentItem(Guid.NewGuid(),_tenantId){Title="Tenant"});
        var result=await Bind(new HlgContentAdminAppService(repo,_tenant,_features)).GetListAsync(new());result.Items.Single().Title.ShouldBe("Host");
        await _auth.Received().AuthorizeAsync(Arg.Any<ClaimsPrincipal>(),Arg.Any<object>(),"MultiTenancy.HostAppHlgContent");
    }
    [Fact]
    public async Task Ranking_Selected_Game_Does_Not_Include_Other_Game_Scores() {
        var gameId=Guid.NewGuid();var customerId=Guid.NewGuid();var start=new DateTime(2026,9,1);var ev=new HlgRankingEvent(Guid.NewGuid(),"Event",start,start.AddMonths(1),_tenantId){GameId=gameId};
        var events=Repo(ev);var sessions=Repo(new HlgGameSession(Guid.NewGuid(),gameId,customerId,_tenantId){IsFinished=true,FinishedAt=start.AddDays(1),Score=50},new HlgGameSession(Guid.NewGuid(),Guid.NewGuid(),customerId,_tenantId){IsFinished=true,FinishedAt=start.AddDays(1),Score=999});
        var customers=Repo(new Customer(customerId,"0900000000","Player"){TenantId=_tenantId});
        var result=await Bind(new HlgRankingAppService(events,sessions,customers,NullLogger<HlgRankingAppService>.Instance)).GetEventEntriesAsync(ev.Id);result.Single().Score.ShouldBe(50);
    }
    [Theory][InlineData(1,2,1,true)][InlineData(2,3,1,true)][InlineData(3,1,1,false)][InlineData(1,4,2,true)][InlineData(1,4,1,false)]
    public void Fulfillment_Transitions_Are_Type_Specific_And_Forward_Only(int from,int to,int type,bool expected) => HlgFulfillmentAdminAppService.CanTransition((HlgRewardHistoryStatus)from,(HlgRewardHistoryStatus)to,(HlgRewardType)type).ShouldBe(expected);
    [Fact]
    public void Retailer_Is_Additive_Without_Changing_Existing_Customer_Values() {
        ((byte)HlgCustomerType.Pharmacy).ShouldBe((byte)1);((byte)HlgCustomerType.Consumer).ShouldBe((byte)2);((byte)HlgCustomerType.Retailer).ShouldBe((byte)3);
        HlgEnumMapper.CustomerTypeFromString("retailer").ShouldBe(HlgCustomerType.Retailer);HlgEnumMapper.CustomerTypeToString(HlgCustomerType.Retailer).ShouldBe("retailer");
    }
    [Theory][InlineData("valid")][InlineData("tooEarly")][InlineData("full")][InlineData("foreignPrize")][InlineData("otherTenant")][InlineData("noScore")][InlineData("unauthorized")]
    public async Task Winner_Publication_Enforces_Scope_Eligibility_Capacity_And_Permission(string scenario)
    {
        var eventId=Guid.NewGuid();var playerId=Guid.NewGuid();var prizeId=Guid.NewGuid();
        var ev=new HlgRankingEvent(eventId,"Event",new DateTime(2026,9,1),new DateTime(2026,9,18),_tenantId);
        if(scenario=="tooEarly")ev.EndAt=new DateTime(2026,9,20);
        Repo(ev);Repo(new Customer(playerId,"0900000000","Player"){TenantId=_tenantId});
        var prize=new HlgRankingPrize(prizeId,scenario=="otherTenant"?Guid.NewGuid():_tenantId){EventId=scenario=="foreignPrize"?Guid.NewGuid():eventId,Quantity=1,IsActive=true};
        Repo(prize);
        var repository=scenario=="full"?Repo(new HlgRankingWinner(Guid.NewGuid(),_tenantId){EventId=eventId,PrizeId=prizeId,CustomerId=Guid.NewGuid(),IsActive=true}):Repo<HlgRankingWinner>();
        var ranking=Substitute.For<IHlgRankingAppService>();
        ranking.GetEventEntriesAsync(eventId,"0900000000",1,Arg.Any<CancellationToken>()).Returns(scenario=="noScore"?new List<RankingEntryDto>():new(){new(){UserId=playerId,Rank=2,Score=120}});
        _services.AddSingleton(ranking);
        if(scenario=="unauthorized")_auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(),Arg.Any<object>(),Arg.Any<string>()).Returns(AuthorizationResult.Failed());
        var service=Bind(new HlgWinnerAdminAppService(repository,_tenant,_features));
        var input=new CreateHlgWinnerInput{EventId=eventId,PrizeId=prizeId,CustomerId=playerId,IsActive=true};
        if(scenario=="valid") {
            var result=await service.CreateAsync(input);result.Rank.ShouldBe(2);result.Score.ShouldBe(120);
            await repository.Received().InsertAsync(Arg.Is<HlgRankingWinner>(x=>x.TenantId==_tenantId && x.IsActive),true,Arg.Any<CancellationToken>());
        } else {
            if(scenario=="otherTenant" || scenario=="unauthorized") await Should.ThrowAsync<AbpAuthorizationException>(()=>service.CreateAsync(input));
            else await Should.ThrowAsync<UserFriendlyException>(()=>service.CreateAsync(input));
            await repository.DidNotReceive().InsertAsync(Arg.Any<HlgRankingWinner>(),Arg.Any<bool>(),Arg.Any<CancellationToken>());
        }
    }
    [Fact]
    public async Task Multi_Select_Product_Filter_Does_Not_Expose_Archived_Or_Unselected_Products()
    {
        var category=new HlgKnowledgeCategory(Guid.NewGuid(),"Industry",_tenantId);Repo(category);Repo<HlgBrand>();
        var first=new HlgProduct(Guid.NewGuid(),category.Id,"First",_tenantId);var hidden=new HlgProduct(Guid.NewGuid(),category.Id,"Hidden",_tenantId){IsActive=false};
        Repo(first,hidden,new HlgProduct(Guid.NewGuid(),category.Id,"Unselected",_tenantId));
        var result=await Bind(new HlgContentAppService()).SearchProductsAsync(productIds:new(){first.Id,hidden.Id});
        result.Single().Id.ShouldBe(first.Id);
    }
    public void Dispose() { foreach(var provider in _providers)provider.Dispose(); }
}
