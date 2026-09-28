using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.SignalR;
using Genora.MultiTenancy.Hlg;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.HttpApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Xunit;
using Xunit.Abstractions;
namespace Genora.MultiTenancy.Hlg;
public class HlgDesignWebTests
{
    private readonly ITestOutputHelper _output;
    public HlgDesignWebTests(ITestOutputHelper output) { _output=output; }
    [Fact]
    public void Live_Feed_Groups_Separate_Host_And_Tenants_For_The_Same_Game()
    {
        var game=Guid.NewGuid();var tenant=Guid.NewGuid();
        HlgLiveFeedHub.GroupName(tenant,game).ShouldNotBe(HlgLiveFeedHub.GroupName(null,game));
        HlgLiveFeedHub.GroupName(tenant,game).ShouldNotBe(HlgLiveFeedHub.GroupName(Guid.NewGuid(),game));
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task Live_Feed_Only_Joins_A_Visible_Game(bool visible)
    {
        var tenant=Substitute.For<ICurrentTenant>();tenant.Id.Returns(Guid.NewGuid());
        var game=Guid.NewGuid();var games=Substitute.For<IHlgGameAppService>();
        games.GetGameAsync(game,Arg.Any<CancellationToken>()).Returns(_=>visible ? Task.FromResult(new GameDetailDto{Id=game}) : Task.FromException<GameDetailDto>(new Volo.Abp.UserFriendlyException("hidden")));
        var groups=Substitute.For<IGroupManager>();var context=Substitute.For<HubCallerContext>();context.ConnectionId.Returns("connection");
        var hub=new HlgLiveFeedHub(tenant,games){Groups=groups,Context=context};
        if (visible) { await hub.JoinGame(game);await groups.Received().AddToGroupAsync("connection",HlgLiveFeedHub.GroupName(tenant.Id,game),Arg.Any<CancellationToken>()); }
        else { await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(()=>hub.JoinGame(game));await groups.DidNotReceive().AddToGroupAsync(Arg.Any<string>(),Arg.Any<string>(),Arg.Any<CancellationToken>()); }
    }
    [Fact]
    public async Task Product_Edit_Loads_And_Saves_All_Content_Collections()
    {
        var id=Guid.NewGuid();var related=Guid.NewGuid();var brand=Guid.NewGuid();
        var service=Substitute.For<IHlgProductAdminAppService>();
        service.GetAsync(id).Returns(new HlgProductAdminDto{Id=id,Name="P",BrandId=brand,Details=new(){Knowledge=new(){new(){Title="Question",Content="Answer"}},Media=new(){new(){Kind=HlgMediaKind.Video,Url="/video.mp4"}},RelatedProductIds=new(){related}}});
        var page=new Genora.MultiTenancy.Web.Pages.Hlg.Products.EditModalModel(service){Id=id};
        await page.OnGetAsync();page.Input.BrandId.ShouldBe(brand);page.Input.Details.RelatedProductIds.Single().ShouldBe(related);
        await page.OnPostAsync();await service.Received().UpdateAsync(id,Arg.Is<UpdateHlgProductInput>(x=>x.Details.Media[0].Url=="/video.mp4" && x.Details.Knowledge[0].Content=="Answer"));
    }
    [Fact]
    public async Task Game_Edit_Loads_Quiz_Play_Configuration()
    {
        var id=Guid.NewGuid();var service=Substitute.For<IHlgGameAdminAppService>();
        service.GetAsync(id).Returns(new HlgGameAdminDto{Id=id,Name="Quiz",Type=Genora.MultiTenancy.Enums.Hlg.HlgGameType.Quiz,QuestionsPerPlay=15,AllowedWrongAnswers=4});
        var page=new Genora.MultiTenancy.Web.Pages.Hlg.Games.EditModel(service){Id=id};

        await page.OnGetAsync();

        page.Input.QuestionsPerPlay.ShouldBe(15);page.Input.AllowedWrongAnswers.ShouldBe(4);
    }
    [Fact]
    public async Task Answer_And_Finish_Apis_Return_Game_Failed_Message()
    {
        var games=Substitute.For<IHlgGameAppService>();
        games.AnswerAsync(Arg.Any<AnswerQuestionPayloadDto>(),Arg.Any<CancellationToken>()).Returns(new AnswerResultDto{GameFailed=true});
        games.FinishAsync(Arg.Any<Guid>(),Arg.Any<FinishGamePayloadDto>(),Arg.Any<CancellationToken>()).Returns(new GameResultDto{GameFailed=true});
        var controller=new HoaLinhGamificationController(
            Substitute.For<IZaloApiClient>(),Substitute.For<IHlgProfileAppService>(),Substitute.For<IHlgKnowledgeAppService>(),games,
            Substitute.For<IHlgRewardAppService>(),Substitute.For<IHlgRankingAppService>(),Substitute.For<IHlgBrandAppService>(),NullLogger<HoaLinhGamificationController>.Instance);

        var answer=(OkObjectResult)await controller.Answer(new AnswerQuestionPayloadDto(),CancellationToken.None);
        var finish=(OkObjectResult)await controller.Finish(Guid.NewGuid(),new FinishGamePayloadDto(),CancellationToken.None);

        ((HlgApiResult<AnswerResultDto>)answer.Value!).Message.ShouldBe("Trò chơi thất bại");
        ((HlgApiResult<GameResultDto>)finish.Value!).Message.ShouldBe("Trò chơi thất bại");
    }
    [Theory]
    [InlineData("Reward")][InlineData("Category")][InlineData("Brand")][InlineData("Product")]
    [InlineData("Ranking")][InlineData("Prize")][InlineData("Winner")][InlineData("Game")]
    [InlineData("Question")][InlineData("User")][InlineData("Content")][InlineData("Lookup")][InlineData("Fulfillment")]
    public void Installed_Abp_Generator_Resolves_The_Actual_Hlg_Service_Type(string service)
    {
        var assembly=typeof(Genora.MultiTenancy.AppServices.Hlg.Admin.HlgRewardAdminAppService).Assembly;
        var type=assembly.GetType("Genora.MultiTenancy.AppServices.Hlg.Admin.Hlg"+service+"AdminAppService",true)!;
        var generator=new Volo.Abp.Http.ProxyScripting.Generators.JQuery.JQueryProxyScriptGenerator(
            Microsoft.Extensions.Options.Options.Create(new Volo.Abp.Http.ProxyScripting.Generators.JQuery.DynamicJavaScriptProxyOptions()));
        var method=generator.GetType().GetMethod("GetNormalizedTypeName",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Instance)!;
        var name=(string)method.Invoke(generator,new object[]{type.FullName!})!;
        name.ShouldBe("genora.multiTenancy.appServices.hlg.admin.hlg"+service+"Admin");
    }
}
