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
using Genora.MultiTenancy.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
using Volo.Abp.Uow;
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
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["App:AppUrl"] = "https://api.example.test" })
        .Build();
    public HlgDesignContentTests() {
        _tenant.Id.Returns(_tenantId); _tenant.IsAvailable.Returns(true);
        _features.IsEnabledAsync(AppHlgFeatures.Management).Returns(true);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(),Arg.Any<object>(),Arg.Any<string>()).Returns(AuthorizationResult.Success());
        var guid=Substitute.For<IGuidGenerator>();guid.Create().Returns(_=>Guid.NewGuid());
        var clock=Substitute.For<IClock>();clock.Now.Returns(new DateTime(2026,9,19));
        var localizer=Substitute.For<IStringLocalizer>();localizer[Arg.Any<string>()].Returns(c=>new LocalizedString(c.Arg<string>(),c.Arg<string>()));
        var factory=Substitute.For<IStringLocalizerFactory>();factory.Create(Arg.Any<Type>()).Returns(localizer);
        _services.AddSingleton<ICurrentTenant>(_tenant).AddSingleton(_features).AddSingleton<IAuthorizationService>(_auth).AddSingleton(guid).AddSingleton(clock).AddSingleton(factory).AddSingleton(_configuration)
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()));
    }
    private IRepository<T,Guid> Repo<T>(params T[] rows) where T:class,IEntity<Guid> {
        var repo=Substitute.For<IRepository<T,Guid>>();
        repo.GetQueryableAsync().Returns(Task.FromResult(rows.AsQueryable()));
        repo.GetAsync(Arg.Any<Guid>(),Arg.Any<bool>(),Arg.Any<CancellationToken>()).Returns(c=>Task.FromResult(rows.Single(x=>x.Id==c.Arg<Guid>())));
        repo.FirstOrDefaultAsync(Arg.Any<Expression<Func<T,bool>>>(),Arg.Any<CancellationToken>()).Returns(c=>Task.FromResult(rows.FirstOrDefault(c.Arg<Expression<Func<T,bool>>>().Compile())));
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
    public async Task Quiz_List_Uses_Configured_Question_Count_And_Returns_Wrong_Answer_Limit()
    {
        var game=new HlgGame(Guid.NewGuid(),"Quiz",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ongoing,QuestionsPerPlay=2,AllowedWrongAnswers=1,ImageUrl="/uploads/hlg/game.png",BannerUrl="https://cdn.example.test/banner.png"};
        var questions=Enumerable.Range(0,4).Select(index=>new HlgQuestion(Guid.NewGuid(),game.Id,index,$"Question {index}"){TenantId=_tenantId}).ToArray();
        var games=Repo(game);var questionRepo=Repo(questions);var options=Repo<HlgAnswerOption>();var sessions=Repo<HlgGameSession>();var answers=Repo<HlgSessionAnswer>();var customers=Repo<Customer>();
        var service=Bind(new HlgGameAppService(games,questionRepo,options,sessions,answers,customers,_tenant,Substitute.For<IHlgLiveFeedNotifier>(),NullLogger<HlgGameAppService>.Instance,_configuration));

        var result=await service.GetGamesAsync();

        result.Single().TotalQuestions.ShouldBe(2);result.Single().QuestionsPerPlay.ShouldBe(2);result.Single().AllowedWrongAnswers.ShouldBe(1);
        result.Single().ImageUrl.ShouldBe("https://api.example.test/uploads/hlg/game.png");
        result.Single().BannerUrl.ShouldBe("https://cdn.example.test/banner.png");
    }
    [Theory]
    [InlineData(null, 99, false)]
    [InlineData(2, 2, false)]
    [InlineData(2, 3, true)]
    [InlineData(0, 1, true)]
    public void Quiz_Fails_Only_After_Wrong_Answers_Exceed_The_Configured_Maximum(int? allowed, int wrongCount, bool expected)
        => HlgGameAppService.HasExceededWrongAnswerLimit(allowed, wrongCount).ShouldBe(expected);
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
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("/\\foreign.test/image.png")]
    public async Task Product_Rejects_Unsafe_Legacy_Image_Urls(string imageUrl) {
        var category=new HlgKnowledgeCategory(Guid.NewGuid(),"Industry",_tenantId);
        var products=Repo<HlgProduct>();var categories=Repo(category);Repo<HlgBrand>();
        var service=Bind(new HlgProductAdminAppService(products,_tenant,_features,categories));
        await Should.ThrowAsync<UserFriendlyException>(()=>service.CreateAsync(new(){CategoryId=category.Id,Name="Product",ImageUrls=imageUrl}));
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
        var shown=new HlgProduct(Guid.NewGuid(),category.Id,"Visible",_tenantId)
        {
            ThumbnailUrl="/uploads/hlg/thumb.png",
            ImagesJson=JsonSerializer.Serialize(new[]{"/uploads/hlg/gallery.png","https://cdn.example.test/gallery.png"}),
            DetailsJson=JsonSerializer.Serialize(new HlgProductContent { Media=new(){new(){Url="/uploads/hlg/video.mp4",PosterUrl="/uploads/hlg/poster.png"}} })
        };
        Repo(shown,new HlgProduct(Guid.NewGuid(),category.Id,"Hidden brand",_tenantId){BrandId=brand.Id},new HlgProduct(Guid.NewGuid(),category.Id,"Other tenant",Guid.NewGuid()));
        var result=await Bind(new HlgContentAppService(_configuration)).SearchProductsAsync();result.Count.ShouldBe(1);result.Single().Id.ShouldBe(shown.Id);
        result.Single().ThumbnailUrl.ShouldBe("https://api.example.test/uploads/hlg/thumb.png");
        result.Single().Images.ShouldBe(new[]{"https://api.example.test/uploads/hlg/gallery.png","https://cdn.example.test/gallery.png"});
        result.Single().Details.Media.Single().Url.ShouldBe("https://api.example.test/uploads/hlg/video.mp4");
        result.Single().Details.Media.Single().PosterUrl.ShouldBe("https://api.example.test/uploads/hlg/poster.png");
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
        var events=Repo(ev);var sessions=Repo(new HlgGameSession(Guid.NewGuid(),gameId,customerId,_tenantId){IsFinished=true,FinishedAt=start.AddDays(1),Score=50,TotalQuestions=2,CorrectCount=2},new HlgGameSession(Guid.NewGuid(),Guid.NewGuid(),customerId,_tenantId){IsFinished=true,FinishedAt=start.AddDays(1),Score=999,TotalQuestions=2,CorrectCount=2});
        var customers=Repo(new Customer(customerId,"0900000000","Player"){TenantId=_tenantId});
        var result=await Bind(new HlgRankingAppService(events,sessions,customers,Repo<HlgRankingEventGame>(),Repo<HlgGame>(),NullLogger<HlgRankingAppService>.Instance,_configuration,Substitute.For<Genora.MultiTenancy.AppDtos.AppImages.IManageImageService>())).GetEventEntriesAsync(ev.Id);result.Single().Score.ShouldBe(50);
    }
    [Theory]
    [InlineData("ongoing")]      // có game ongoing → chọn ongoing (startAt mới nhất)
    [InlineData("endedOnly")]    // không ongoing → ended mới nhất theo endAt DESC
    [InlineData("upcomingOnly")] // chỉ còn upcoming → không có "game hiện tại" → null
    public async Task Current_Event_Picks_Ongoing_Then_Latest_Ended_Else_Null(string scenario)
    {
        // Clock = 2026-09-19 (xem ctor). Event bao trùm toàn bộ game.
        var ev=new HlgRankingEvent(Guid.NewGuid(),"Dạ Hương",new DateTime(2026,9,1),new DateTime(2026,10,31),_tenantId);
        var endedOld=new HlgGame(Guid.NewGuid(),"Tuần 1",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ended,StartAt=new DateTime(2026,9,1),EndAt=new DateTime(2026,9,8)};
        var endedNew=new HlgGame(Guid.NewGuid(),"Tuần 2",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ended,StartAt=new DateTime(2026,9,9),EndAt=new DateTime(2026,9,16)};
        var ongoing=new HlgGame(Guid.NewGuid(),"Tuần 3",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ongoing,StartAt=new DateTime(2026,9,17),EndAt=new DateTime(2026,9,24)};
        var upcoming=new HlgGame(Guid.NewGuid(),"Tuần 4",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Upcoming,StartAt=new DateTime(2026,9,25),EndAt=new DateTime(2026,10,2)};
        var games=scenario switch {
            "ongoing"=>new[]{endedOld,endedNew,ongoing,upcoming},
            "endedOnly"=>new[]{endedOld,endedNew,upcoming},
            _=>new[]{upcoming}
        };
        var eventRepo=Repo(ev);
        var eventGames=Repo(games.Select(g=>new HlgRankingEventGame(Guid.NewGuid(),ev.Id,g.Id,_tenantId)).ToArray());
        var gameRepo=Repo(games);
        var service=Bind(new HlgRankingAppService(eventRepo,Repo<HlgGameSession>(),Repo<Customer>(),eventGames,gameRepo,NullLogger<HlgRankingAppService>.Instance,_configuration,Substitute.For<Genora.MultiTenancy.AppDtos.AppImages.IManageImageService>()));

        var dto=await service.GetCurrentEventAsync();

        if(scenario=="upcomingOnly"){ dto.ShouldBeNull(); return; }
        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(ev.Id);
        dto.EventStartAt.ShouldBe(ev.StartAt);
        dto.EventEndAt.ShouldBe(ev.EndAt);
        if(scenario=="ongoing"){
            dto.GameId.ShouldBe(ongoing.Id);
            dto.GameName.ShouldBe("Tuần 3");
            dto.GameStatus.ShouldBe("ongoing");
            dto.StartAt.ShouldBe(ongoing.StartAt!.Value);
            dto.EndAt.ShouldBe(ongoing.EndAt!.Value);
        } else {
            dto.GameId.ShouldBe(endedNew.Id); // ended mới nhất theo endAt DESC
            dto.GameStatus.ShouldBe("ended");
        }
    }
    [Fact]
    public async Task Entries_Default_To_Current_Game_And_Respect_Explicit_GameId()
    {
        var ev=new HlgRankingEvent(Guid.NewGuid(),"Dạ Hương",new DateTime(2026,9,1),new DateTime(2026,10,31),_tenantId);
        var endedGame=new HlgGame(Guid.NewGuid(),"Tuần 1",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ended,StartAt=new DateTime(2026,9,1),EndAt=new DateTime(2026,9,10)};
        var ongoingGame=new HlgGame(Guid.NewGuid(),"Tuần 2",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ongoing,StartAt=new DateTime(2026,9,15),EndAt=new DateTime(2026,9,25)};
        var c1=Guid.NewGuid();var c2=Guid.NewGuid();
        var sessions=Repo(
            new HlgGameSession(Guid.NewGuid(),endedGame.Id,c1,_tenantId){IsFinished=true,FinishedAt=new DateTime(2026,9,5),Score=70,TotalQuestions=2,CorrectCount=2},
            new HlgGameSession(Guid.NewGuid(),ongoingGame.Id,c2,_tenantId){IsFinished=true,FinishedAt=new DateTime(2026,9,18),Score=50,TotalQuestions=2,CorrectCount=2});
        var eventRepo=Repo(ev);
        var eventGames=Repo(
            new HlgRankingEventGame(Guid.NewGuid(),ev.Id,endedGame.Id,_tenantId),
            new HlgRankingEventGame(Guid.NewGuid(),ev.Id,ongoingGame.Id,_tenantId));
        var gameRepo=Repo(endedGame,ongoingGame);
        var customers=Repo(new Customer(c1,"0900000001","P1"){TenantId=_tenantId},new Customer(c2,"0900000002","P2"){TenantId=_tenantId});
        var service=Bind(new HlgRankingAppService(eventRepo,sessions,customers,eventGames,gameRepo,NullLogger<HlgRankingAppService>.Instance,_configuration,Substitute.For<Genora.MultiTenancy.AppDtos.AppImages.IManageImageService>()));

        // Mặc định (không truyền gameId) → game hiện tại (ongoing Tuần 2) → chỉ c2.
        (await service.GetEntriesAsync()).Single().UserId.ShouldBe(c2);
        // Truyền gameId Tuần 1 (đã kết thúc) → chỉ c1.
        (await service.GetEntriesAsync(gameId: endedGame.Id)).Single().UserId.ShouldBe(c1);
    }
    [Fact]
    public async Task Ranking_Event_With_GameId_Returns_That_Game_With_Owning_Event()
    {
        // FE mở BXH từ 1 game đã kết thúc → /ranking/event?gameId= phải trả ĐÚNG game đó (không rơi về ongoing).
        var ev=new HlgRankingEvent(Guid.NewGuid(),"Dạ Hương",new DateTime(2026,9,1),new DateTime(2026,10,31),_tenantId);
        var endedGame=new HlgGame(Guid.NewGuid(),"Tuần 2",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ended,StartAt=new DateTime(2026,9,9),EndAt=new DateTime(2026,9,16)};
        var ongoingGame=new HlgGame(Guid.NewGuid(),"Tuần 3",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ongoing,StartAt=new DateTime(2026,9,17),EndAt=new DateTime(2026,9,24)};
        var eventRepo=Repo(ev);
        var eventGames=Repo(
            new HlgRankingEventGame(Guid.NewGuid(),ev.Id,endedGame.Id,_tenantId),
            new HlgRankingEventGame(Guid.NewGuid(),ev.Id,ongoingGame.Id,_tenantId));
        var gameRepo=Repo(endedGame,ongoingGame);
        var service=Bind(new HlgRankingAppService(eventRepo,Repo<HlgGameSession>(),Repo<Customer>(),eventGames,gameRepo,NullLogger<HlgRankingAppService>.Instance,_configuration,Substitute.For<Genora.MultiTenancy.AppDtos.AppImages.IManageImageService>()));

        var dto=await service.GetCurrentEventAsync(endedGame.Id);

        dto.ShouldNotBeNull();
        dto!.GameId.ShouldBe(endedGame.Id);
        dto.GameName.ShouldBe("Tuần 2");
        dto.GameStatus.ShouldBe("ended");
        dto.StartAt.ShouldBe(endedGame.StartAt!.Value);
        dto.EndAt.ShouldBe(endedGame.EndAt!.Value);
        dto.Id.ShouldBe(ev.Id);               // event sở hữu
        dto.Title.ShouldBe("Dạ Hương");
        dto.EventStartAt.ShouldBe(ev.StartAt);
    }
    [Fact]
    public async Task Ranking_Event_With_Unknown_GameId_Returns_Null()
    {
        var ev=new HlgRankingEvent(Guid.NewGuid(),"Dạ Hương",new DateTime(2026,9,1),new DateTime(2026,10,31),_tenantId);
        var service=Bind(new HlgRankingAppService(Repo(ev),Repo<HlgGameSession>(),Repo<Customer>(),Repo<HlgRankingEventGame>(),Repo<HlgGame>(),NullLogger<HlgRankingAppService>.Instance,_configuration,Substitute.For<Genora.MultiTenancy.AppDtos.AppImages.IManageImageService>()));
        (await service.GetCurrentEventAsync(Guid.NewGuid())).ShouldBeNull();
    }
    [Fact]
    public async Task Ranking_Event_With_Orphan_GameId_Returns_Game_With_Empty_Event()
    {
        // Game hợp lệ nhưng không thuộc sự kiện nào (mồ côi) → trả field game, field event để trống.
        var orphan=new HlgGame(Guid.NewGuid(),"Game lẻ",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ended,StartAt=new DateTime(2026,9,1),EndAt=new DateTime(2026,9,8)};
        var service=Bind(new HlgRankingAppService(Repo<HlgRankingEvent>(),Repo<HlgGameSession>(),Repo<Customer>(),Repo<HlgRankingEventGame>(),Repo(orphan),NullLogger<HlgRankingAppService>.Instance,_configuration,Substitute.For<Genora.MultiTenancy.AppDtos.AppImages.IManageImageService>()));

        var dto=await service.GetCurrentEventAsync(orphan.Id);

        dto.ShouldNotBeNull();
        dto!.GameId.ShouldBe(orphan.Id);
        dto.GameName.ShouldBe("Game lẻ");
        dto.GameStatus.ShouldBe("ended");
        dto.Id.ShouldBe(Guid.Empty);          // không có event sở hữu
        dto.Title.ShouldBe(string.Empty);
    }
    [Theory][InlineData(1,2,1,true)][InlineData(2,3,1,true)][InlineData(3,1,1,false)][InlineData(1,4,2,true)][InlineData(1,4,1,false)]
    public void Fulfillment_Transitions_Are_Type_Specific_And_Forward_Only(int from,int to,int type,bool expected) => HlgFulfillmentAdminAppService.CanTransition((HlgRewardHistoryStatus)from,(HlgRewardHistoryStatus)to,(HlgRewardType)type).ShouldBe(expected);
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Shipping_Address_Rejects_Foreign_Or_Unfinished_Session(bool sameOwner,bool finished) {
        var customer=new Customer(Guid.NewGuid(),"0900000000","Player"){TenantId=_tenantId};
        var session=new HlgGameSession(Guid.NewGuid(),Guid.NewGuid(),sameOwner?customer.Id:Guid.NewGuid(),_tenantId){IsFinished=finished};
        var rewards=Repo<HlgReward>();var histories=Repo<HlgRewardHistory>();var addresses=Repo<HlgShippingAddress>();var sessions=Repo(session);var profiles=Repo<HlgUserProfile>();var customers=Repo(customer);
        var service=Bind(new HlgRewardAppService(rewards,histories,addresses,sessions,profiles,customers,_tenant,Substitute.For<IUnitOfWorkManager>(),NullLogger<HlgRewardAppService>.Instance,_configuration));
        await Should.ThrowAsync<UserFriendlyException>(()=>service.SetSessionShippingAddressAsync(session.Id,customer.PhoneNumber!,new(){ReceiverName="Player",Phone="0900000000",Address="Address"}));
        await addresses.DidNotReceive().InsertAsync(Arg.Any<HlgShippingAddress>(),Arg.Any<bool>(),Arg.Any<CancellationToken>());
    }
    [Fact]
    public void Retailer_Is_Additive_Without_Changing_Existing_Customer_Values() {
        ((byte)HlgCustomerType.Pharmacy).ShouldBe((byte)1);((byte)HlgCustomerType.Consumer).ShouldBe((byte)2);((byte)HlgCustomerType.Retailer).ShouldBe((byte)3);
        HlgEnumMapper.CustomerTypeFromString("retailer").ShouldBe(HlgCustomerType.Retailer);HlgEnumMapper.CustomerTypeToString(HlgCustomerType.Retailer).ShouldBe("retailer");
    }
    [Theory][InlineData("valid")][InlineData("tooEarly")][InlineData("full")][InlineData("foreignPrize")][InlineData("otherTenant")][InlineData("noScore")][InlineData("unauthorized")]
    public async Task Winner_Publication_Enforces_Scope_Eligibility_Capacity_And_Permission(string scenario)
    {
        var eventId=Guid.NewGuid();var playerId=Guid.NewGuid();var prizeId=Guid.NewGuid();var gameId=Guid.NewGuid();
        var ev=new HlgRankingEvent(eventId,"Event",new DateTime(2026,9,1),new DateTime(2026,9,18),_tenantId){GameId=gameId};
        var game=new HlgGame(gameId,"Game",default,_tenantId){EndAt=scenario=="tooEarly"?new DateTime(2026,9,20):new DateTime(2026,9,18)};
        Repo(ev);Repo(game);Repo(new Customer(playerId,"0900000000","Player"){TenantId=_tenantId});
        var prize=new HlgRankingPrize(prizeId,scenario=="otherTenant"?Guid.NewGuid():_tenantId){EventId=scenario=="foreignPrize"?Guid.NewGuid():eventId,Quantity=1,IsActive=true};
        Repo(prize);
        var repository=scenario=="full"?Repo(new HlgRankingWinner(Guid.NewGuid(),_tenantId){EventId=eventId,GameId=gameId,PrizeId=prizeId,CustomerId=Guid.NewGuid(),IsActive=true}):Repo<HlgRankingWinner>();
        var ranking=Substitute.For<IHlgRankingAppService>();
        ranking.GetGameEntriesAsync(eventId,gameId,"0900000000",1,Arg.Any<CancellationToken>()).Returns(scenario=="noScore"?new List<RankingEntryDto>():new(){new(){UserId=playerId,Rank=2,Score=120}});
        _services.AddSingleton(ranking);
        if(scenario=="unauthorized")_auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(),Arg.Any<object>(),Arg.Any<string>()).Returns(AuthorizationResult.Failed());
        var service=Bind(new HlgWinnerAdminAppService(repository,_tenant,_features));
        var input=new CreateHlgWinnerInput{EventId=eventId,GameId=gameId,PrizeId=prizeId,CustomerId=playerId,IsActive=true};
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
        var result=await Bind(new HlgContentAppService(_configuration)).SearchProductsAsync(productIds:new(){first.Id,hidden.Id});
        result.Single().Id.ShouldBe(first.Id);
    }
    [Theory]
    [InlineData("endedByStatus", 1)] // game Status=Ended (dù EndAt chưa tới) => trả winners
    [InlineData("endedByTime", 1)]   // game Status=Ongoing nhưng EndAt đã qua => trả winners
    [InlineData("notEnded", 0)]      // game chưa kết thúc (Ongoing + EndAt tương lai) => rỗng
    [InlineData("otherTenant", 0)]   // game thuộc tenant khác => rỗng (không lộ dữ liệu chéo tenant)
    [InlineData("notFound", 0)]      // gameId không tồn tại => rỗng
    public async Task Winners_Are_Returned_Only_For_Ended_Game_Of_Current_Tenant(string scenario, int expectedCount)
    {
        // Clock = 2026-09-19. Mốc "đã kết thúc": Status==Ended HOẶC EndAt < now (khớp HlgContentAppService.GetWinnersAsync).
        var gameId=Guid.NewGuid();var customerId=Guid.NewGuid();
        var gameTenant=scenario=="otherTenant"?Guid.NewGuid():_tenantId;
        var status=scenario=="endedByStatus"?HlgGameStatus.Ended:HlgGameStatus.Ongoing;
        var endAt=scenario=="endedByTime"?new DateTime(2026,9,18):new DateTime(2026,9,20);
        var game=new HlgGame(gameId,"Game",HlgGameType.Quiz,gameTenant){Status=status,EndAt=endAt};
        if(scenario=="notFound")Repo<HlgGame>();else Repo(game);
        Repo(new HlgRankingWinner(Guid.NewGuid(),_tenantId){EventId=Guid.NewGuid(),GameId=gameId,PrizeId=Guid.NewGuid(),CustomerId=customerId,Rank=1,Score=90,IsActive=true});
        Repo(new Customer(customerId,"0900000000","Winner"){TenantId=_tenantId,AvatarUrl="/uploads/hlg/avatar.png"});

        var result=await Bind(new HlgContentAppService(_configuration)).GetWinnersAsync(gameId);

        result.Count.ShouldBe(expectedCount);
        if(expectedCount>0)
        {
            result.Single().UserId.ShouldBe(customerId);
            result.Single().Rank.ShouldBe(1);
            result.Single().Score.ShouldBe(90);
            result.Single().AvatarUrl.ShouldBe("https://api.example.test/uploads/hlg/avatar.png");
        }
    }
    [Fact]
    public async Task Winners_Are_Scoped_To_The_Requested_Game_Not_The_Whole_Event()
    {
        // 1 chiến dịch nhiều game: chỉ trả winner của ĐÚNG game yêu cầu, không trộn winner của game khác cùng event.
        var eventId=Guid.NewGuid();var endedGameId=Guid.NewGuid();var otherGameId=Guid.NewGuid();
        var winnerCustomerId=Guid.NewGuid();var otherCustomerId=Guid.NewGuid();
        Repo(new HlgGame(endedGameId,"Tuần 1",HlgGameType.Quiz,_tenantId){Status=HlgGameStatus.Ended,EndAt=new DateTime(2026,9,18)});
        Repo(
            new HlgRankingWinner(Guid.NewGuid(),_tenantId){EventId=eventId,GameId=endedGameId,PrizeId=Guid.NewGuid(),CustomerId=winnerCustomerId,Rank=1,Score=90,IsActive=true},
            new HlgRankingWinner(Guid.NewGuid(),_tenantId){EventId=eventId,GameId=otherGameId,PrizeId=Guid.NewGuid(),CustomerId=otherCustomerId,Rank=1,Score=80,IsActive=true});
        Repo(
            new Customer(winnerCustomerId,"0900000001","Winner"){TenantId=_tenantId},
            new Customer(otherCustomerId,"0900000002","Other"){TenantId=_tenantId});

        var result=await Bind(new HlgContentAppService(_configuration)).GetWinnersAsync(endedGameId);

        result.Single().UserId.ShouldBe(winnerCustomerId);
    }
    public void Dispose() { foreach(var provider in _providers)provider.Dispose(); }
}
