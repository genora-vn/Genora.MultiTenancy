using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.AppServices.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using NSubstitute;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Xunit;
using ClosedXML.Excel;
using System.IO;
namespace Genora.MultiTenancy.Hlg;

public class HlgAdminTests : IDisposable
{
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IFeatureChecker _features = Substitute.For<IFeatureChecker>();
    private readonly IAbpAuthorizationService _auth = Substitute.For<IAbpAuthorizationService>();
    private readonly IRepository<HlgQuestion, Guid> _questions = Substitute.For<IRepository<HlgQuestion, Guid>>();
    private readonly IRepository<HlgGame, Guid> _games = Substitute.For<IRepository<HlgGame, Guid>>();
    private readonly IRepository<HlgAnswerOption, Guid> _options = Substitute.For<IRepository<HlgAnswerOption, Guid>>();
    private readonly IRepository<HlgGameSession, Guid> _sessions = Substitute.For<IRepository<HlgGameSession, Guid>>();
    private readonly ServiceProvider _provider;
    private readonly Guid _gameId = Guid.NewGuid();
    public HlgAdminTests()
    {
        _tenant.IsAvailable.Returns(true); _tenant.Id.Returns(Guid.NewGuid());
        _features.IsEnabledAsync(AppHlgFeatures.Management).Returns(true);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        var guid = Substitute.For<IGuidGenerator>(); guid.Create().Returns(_ => Guid.NewGuid());
        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(c => new LocalizedString(c.Arg<string>(), c.Arg<string>()));
        var factory = Substitute.For<IStringLocalizerFactory>(); factory.Create(Arg.Any<Type>()).Returns(localizer);
        _provider = new ServiceCollection().AddSingleton(factory).AddSingleton<IAuthorizationService>(_auth).AddSingleton<ICurrentTenant>(_tenant)
            .AddSingleton(guid).AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>())).BuildServiceProvider();
        _questions.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgQuestion>().AsQueryable()));
        _options.GetListAsync(Arg.Any<Expression<Func<HlgAnswerOption, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new List<HlgAnswerOption>());
        _games.GetAsync(_gameId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new HlgGame(_gameId, "Quiz", HlgGameType.Quiz));
        _games.InsertAsync(Arg.Any<HlgGame>(), true, Arg.Any<CancellationToken>()).Returns(c => c.Arg<HlgGame>());
        _games.UpdateAsync(Arg.Any<HlgGame>(), true, Arg.Any<CancellationToken>()).Returns(c => c.Arg<HlgGame>());
    }
    private HlgQuestionAdminAppService Service() => new(_questions, _tenant, _features, _games, _options, _sessions, new HlgQuestionExcelTemplateGenerator(), new HlgQuestionExcelImporter()) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    private HlgGameAdminAppService GameService() => new(_games, _tenant, _features, _questions, _sessions) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    private CreateHlgQuestionInput Input() => new() { GameId = _gameId, Content = "Question", OptionA = "A", OptionB = "B", CorrectKey = HlgAnswerKey.B };

    [Fact]
    public void Excel_Importer_Reads_Data_After_Blank_Rows()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Questions");
        worksheet.Cell(3, 1).Value = 1;
        worksheet.Cell(3, 2).Value = "Question one";
        worksheet.Cell(5, 1).Value = 2;
        worksheet.Cell(5, 2).Value = "Question two";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var rows = new HlgQuestionExcelImporter().Read(stream);

        rows.Count.ShouldBe(2);
        rows[0].RowNumber.ShouldBe(3);
        rows[1].RowNumber.ShouldBe(5);
    }

    [Fact]
    public async Task Disabled_Feature_Stops_Before_Query()
    {
        _features.IsEnabledAsync(AppHlgFeatures.Management).Returns(false);
        await Should.ThrowAsync<AbpAuthorizationException>(() => Service().GetListAsync(new()));
        await _questions.DidNotReceive().GetQueryableAsync();
    }
    [Fact]
    public async Task Host_Uses_Host_Permission_And_Does_Not_Check_Tenant_Feature()
    {
        _tenant.IsAvailable.Returns(false); _tenant.Id.Returns((Guid?)null);
        await Service().GetListAsync(new());
        await _auth.Received().AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), MultiTenancyPermissions.HostAppHlgGames.Default);
        await _features.DidNotReceive().IsEnabledAsync(Arg.Any<string>());
    }
    [Fact]
    public async Task Editor_Requires_Edit_Permission_Before_Loading_Secret()
    {
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), MultiTenancyPermissions.AppHlgGames.Edit).Returns(AuthorizationResult.Failed());
        await Should.ThrowAsync<AbpAuthorizationException>(() => Service().GetEditorAsync(Guid.NewGuid()));
        await _questions.DidNotReceive().GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task List_Filters_Parent_Status_Keyword_And_Paginates_Without_Secret()
    {
        var rows = new[] {
            new HlgQuestion(Guid.NewGuid(), _gameId, 0, "Match one") { CorrectKey = HlgAnswerKey.D },
            new HlgQuestion(Guid.NewGuid(), _gameId, 1, "Match two"),
            new HlgQuestion(Guid.NewGuid(), _gameId, 2, "Match inactive") { IsActive = false },
            new HlgQuestion(Guid.NewGuid(), Guid.NewGuid(), 0, "Match other") };
        _questions.GetQueryableAsync().Returns(Task.FromResult(rows.AsQueryable()));
        var result = await Service().GetListAsync(new() { ParentId = _gameId, IsActive = true, FilterText = "Match", SkipCount = 1, MaxResultCount = 1 });
        result.TotalCount.ShouldBe(2); result.Items.Single().Content.ShouldBe("Match two");
        JsonSerializer.Serialize(result).ShouldNotContain("CorrectKey");
        typeof(HlgQuestionAdminDto).GetProperty("CorrectKey").ShouldBeNull();
    }
    [Fact]
    public async Task Create_Saves_Parent_Before_Options_And_Preserves_Tenant()
    {
        var sequence = new List<string>();
        _questions.InsertAsync(Arg.Any<HlgQuestion>(), true, Arg.Any<CancellationToken>()).Returns(c => { sequence.Add("question"); return c.Arg<HlgQuestion>(); });
        _options.InsertAsync(Arg.Any<HlgAnswerOption>(), true, Arg.Any<CancellationToken>()).Returns(c => { sequence.Add("option"); return c.Arg<HlgAnswerOption>(); });
        var result = await Service().CreateAsync(Input());
        sequence.ShouldBe(new[] { "question", "option", "option" });
        await _questions.Received().InsertAsync(Arg.Is<HlgQuestion>(x => x.TenantId == _tenant.Id && x.CorrectKey == HlgAnswerKey.B), true, Arg.Any<CancellationToken>());
        await _options.Received(2).InsertAsync(Arg.Is<HlgAnswerOption>(x => x.TenantId == _tenant.Id && x.QuestionId == result.Id), true, Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Missing_Correct_Option_Is_Rejected_Before_Write()
    {
        var input = Input(); input.CorrectKey = HlgAnswerKey.D;
        await Should.ThrowAsync<ValidationException>(() => Service().CreateAsync(input));
        await _questions.DidNotReceive().InsertAsync(Arg.Any<HlgQuestion>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
    [Theory]
    [InlineData(-1)] [InlineData(0)]
    public void Ranking_Rejects_Reversed_Or_Empty_Interval(int days)
    {
        var input = new HlgRankingInput { Title = "Event", StartAt = DateTime.Today, EndAt = DateTime.Today.AddDays(days) };
        Should.Throw<ValidationException>(() => Validator.ValidateObject(input, new ValidationContext(input), true));
    }
    [Fact]
    public void Reward_Rejects_Negative_Stock()
    {
        var input = new CreateHlgRewardDto { Name = "Gift", StockQuantity = -1 };
        Should.Throw<ValidationException>(() => Validator.ValidateObject(input, new ValidationContext(input), true));
    }
    [Theory]
    [InlineData(null, 1)]
    [InlineData(10, null)]
    [InlineData(10, 11)]
    public void Quiz_Requires_Valid_Play_Configuration(int? questionsPerPlay, int? allowedWrongAnswers)
    {
        var input = new CreateHlgGameInput
        {
            Name = "Quiz",
            Type = HlgGameType.Quiz,
            QuestionsPerPlay = questionsPerPlay,
            AllowedWrongAnswers = allowedWrongAnswers
        };

        Should.Throw<ValidationException>(() => Validator.ValidateObject(input, new ValidationContext(input), true));
    }
    [Fact]
    public async Task Game_Create_Persists_Quiz_Play_Configuration()
    {
        var result = await GameService().CreateAsync(new CreateHlgGameInput
        {
            Name = "Quiz",
            Type = HlgGameType.Quiz,
            QuestionsPerPlay = 12,
            AllowedWrongAnswers = 3
        });

        result.QuestionsPerPlay.ShouldBe(12);
        result.AllowedWrongAnswers.ShouldBe(3);
        await _games.Received().InsertAsync(
            Arg.Is<HlgGame>(x => x.QuestionsPerPlay == 12 && x.AllowedWrongAnswers == 3),
            true,
            Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Existing_Quiz_With_Sessions_Can_Initialize_Pre_Migration_Null_Configuration()
    {
        _sessions.AnyAsync(Arg.Any<Expression<Func<HlgGameSession, bool>>>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await GameService().UpdateAsync(_gameId, new UpdateHlgGameInput
        {
            Name = "Quiz",
            Type = HlgGameType.Quiz,
            QuestionsPerPlay = 10,
            AllowedWrongAnswers = 2
        });

        result.QuestionsPerPlay.ShouldBe(10);
        result.AllowedWrongAnswers.ShouldBe(2);
    }
    [Fact]
    public async Task Product_Allows_Empty_Optional_Content_And_Stores_Image_Array()
    {
        var products = Substitute.For<IRepository<HlgProduct, Guid>>();
        var categories = Substitute.For<IRepository<HlgKnowledgeCategory, Guid>>();
        products.InsertAsync(Arg.Any<HlgProduct>(), true, Arg.Any<CancellationToken>()).Returns(c => c.Arg<HlgProduct>());
        categories.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(c => new HlgKnowledgeCategory(c.Arg<Guid>(), "Category", _tenant.Id));
        var service = new HlgProductAdminAppService(products, _tenant, _features, categories) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        var result = await service.CreateAsync(new() { CategoryId = Guid.NewGuid(), Name = "Lesson", Content = null, ImageUrls = " /one.png\r\n\r\n/two.png " });
        result.Content.ShouldBeNull(); result.ImageUrls.ShouldBe("/one.png\n/two.png");
        await products.Received().InsertAsync(Arg.Is<HlgProduct>(x => x.ImagesJson == "[\"/one.png\",\"/two.png\"]"), true, Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Edit_Updates_Existing_Options_Adds_New_And_Removes_Blank_Options()
    {
        var question = new HlgQuestion(Guid.NewGuid(), _gameId, 0, "Before");
        var a = new HlgAnswerOption(Guid.NewGuid(), question.Id, HlgAnswerKey.A, "Old A");
        var d = new HlgAnswerOption(Guid.NewGuid(), question.Id, HlgAnswerKey.D, "Old D");
        _questions.GetAsync(question.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(question);
        _options.GetListAsync(Arg.Any<Expression<Func<HlgAnswerOption, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new List<HlgAnswerOption> { a, d });
        await Service().UpdateAsync(question.Id, new() { GameId = _gameId, Content = "After", OptionA = "New A", OptionB = "New B", CorrectKey = HlgAnswerKey.B });
        question.Content.ShouldBe("After"); question.CorrectKey.ShouldBe(HlgAnswerKey.B); a.Content.ShouldBe("New A");
        await _options.Received().UpdateAsync(a, true, Arg.Any<CancellationToken>());
        await _options.Received().DeleteAsync(d, true, Arg.Any<CancellationToken>());
        await _options.Received().InsertAsync(Arg.Is<HlgAnswerOption>(x => x.Key == HlgAnswerKey.B && x.QuestionId == question.Id), true, Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Game_With_Sessions_Rejects_Question_Changes()
    {
        _sessions.AnyAsync(Arg.Any<Expression<Func<HlgGameSession, bool>>>(), Arg.Any<CancellationToken>()).Returns(true);
        var error = await Should.ThrowAsync<UserFriendlyException>(() => Service().CreateAsync(Input()));
        error.Message.ShouldBe("Hlg:GameHasSessions");
        await _questions.DidNotReceive().InsertAsync(Arg.Any<HlgQuestion>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Duplicate_Question_Index_Is_Rejected()
    {
        _questions.AnyAsync(Arg.Any<Expression<Func<HlgQuestion, bool>>>(), Arg.Any<CancellationToken>()).Returns(true);
        var error = await Should.ThrowAsync<UserFriendlyException>(() => Service().CreateAsync(Input()));
        error.Message.ShouldBe("Hlg:QuestionIndexExists");
    }
    public void Dispose() => _provider.Dispose();
}
