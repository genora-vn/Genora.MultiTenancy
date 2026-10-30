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
using Genora.MultiTenancy.DomainModels.AppCustomers;
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
using Volo.Abp.Timing;
using Xunit;
using ClosedXML.Excel;
using System.IO;
using System.IO.Compression;
namespace Genora.MultiTenancy.Hlg;

public class HlgAdminTests : IDisposable
{
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IFeatureChecker _features = Substitute.For<IFeatureChecker>();
    private readonly IAbpAuthorizationService _auth = Substitute.For<IAbpAuthorizationService>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IRepository<HlgQuestion, Guid> _questions = Substitute.For<IRepository<HlgQuestion, Guid>>();
    private readonly IRepository<HlgGame, Guid> _games = Substitute.For<IRepository<HlgGame, Guid>>();
    private readonly IRepository<HlgAnswerOption, Guid> _options = Substitute.For<IRepository<HlgAnswerOption, Guid>>();
    private readonly IRepository<HlgGameSession, Guid> _sessions = Substitute.For<IRepository<HlgGameSession, Guid>>();
    private readonly IRepository<HlgSessionAnswer, Guid> _answers = Substitute.For<IRepository<HlgSessionAnswer, Guid>>();
    private readonly IRepository<HlgRankingEvent, Guid> _rankingEvents = Substitute.For<IRepository<HlgRankingEvent, Guid>>();
    private readonly IRepository<HlgRankingResultSnapshot, Guid> _rankingSnapshots = Substitute.For<IRepository<HlgRankingResultSnapshot, Guid>>();
    private readonly IRepository<HlgRankingEventGame, Guid> _rankingEventGames = Substitute.For<IRepository<HlgRankingEventGame, Guid>>();
    private readonly IRepository<HlgRankingWinner, Guid> _rankingWinners = Substitute.For<IRepository<HlgRankingWinner, Guid>>();
    private readonly IRepository<Customer, Guid> _customers = Substitute.For<IRepository<Customer, Guid>>();
    private readonly IRepository<HlgUserProfile, Guid> _profiles = Substitute.For<IRepository<HlgUserProfile, Guid>>();
    private readonly ServiceProvider _provider;
    private readonly Guid _gameId = Guid.NewGuid();
    public HlgAdminTests()
    {
        _tenant.IsAvailable.Returns(true); _tenant.Id.Returns(Guid.NewGuid());
        _features.IsEnabledAsync(AppHlgFeatures.Management).Returns(true);
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        _clock.Now.Returns(DateTime.Today);
        var guid = Substitute.For<IGuidGenerator>(); guid.Create().Returns(_ => Guid.NewGuid());
        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(c => new LocalizedString(c.Arg<string>(), c.Arg<string>()));
        var factory = Substitute.For<IStringLocalizerFactory>(); factory.Create(Arg.Any<Type>()).Returns(localizer);
        _provider = new ServiceCollection().AddSingleton(factory).AddSingleton<IAuthorizationService>(_auth).AddSingleton<ICurrentTenant>(_tenant)
            .AddSingleton(guid).AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .AddSingleton(_sessions).AddSingleton(_answers).AddSingleton(_games).AddSingleton(_customers).AddSingleton(_profiles).AddSingleton(_rankingWinners).AddSingleton(_clock).BuildServiceProvider();
        _questions.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgQuestion>().AsQueryable()));
        _options.GetListAsync(Arg.Any<Expression<Func<HlgAnswerOption, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new List<HlgAnswerOption>());
        _games.GetAsync(_gameId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new HlgGame(_gameId, "Quiz", HlgGameType.Quiz));
        _games.InsertAsync(Arg.Any<HlgGame>(), true, Arg.Any<CancellationToken>()).Returns(c => c.Arg<HlgGame>());
        _games.UpdateAsync(Arg.Any<HlgGame>(), true, Arg.Any<CancellationToken>()).Returns(c => c.Arg<HlgGame>());
        _rankingSnapshots.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgRankingResultSnapshot>().AsQueryable()));
        _rankingEventGames.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgRankingEventGame>().AsQueryable()));
        _rankingWinners.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgRankingWinner>().AsQueryable()));
        // Default empty cho thông tin live: các test export sẽ override khi cần.
        _sessions.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgGameSession>().AsQueryable()));
        _answers.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgSessionAnswer>().AsQueryable()));
        _customers.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<Customer>().AsQueryable()));
        _profiles.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<HlgUserProfile>().AsQueryable()));
    }
    private HlgQuestionAdminAppService Service() => new(_questions, _tenant, _features, _games, _options, _sessions, new HlgQuestionExcelTemplateGenerator(), new HlgQuestionExcelImporter()) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    private HlgGameAdminAppService GameService() => new(_games, _tenant, _features, _questions, _sessions) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    private HlgRankingAdminAppService RankingService() => new(_rankingEvents, _tenant, _features, new HlgRankingResultExcelExporter(), _rankingSnapshots, _rankingEventGames) { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
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
    public void Winner_Excel_Template_And_Importer_Preserve_Prize_And_Phone()
    {
        var rankingEvent = new HlgRankingEvent(Guid.NewGuid(), "Sự kiện tháng 9", DateTime.Today, DateTime.Today.AddDays(1), _tenant.Id);
        var game = new HlgGame(Guid.NewGuid(), "Game tuần 1", default, _tenant.Id) { EndAt = DateTime.Today.AddDays(-1) };
        var reward = new HlgReward(Guid.NewGuid(), "Chai Dạ Hương 100ml", default, 0, _tenant.Id);
        var prize = new HlgRankingPrize(Guid.NewGuid(), _tenant.Id)
        {
            EventId = rankingEvent.Id,
            RewardId = reward.Id,
            Title = "Giải nhất",
            Quantity = 1,
            IsActive = true
        };

        using var template = new HlgWinnerExcelTemplateGenerator().Generate(new[] { rankingEvent }, new[] { game }, new[] { prize }, new[] { reward });
        var templateStream = template.GetStream();
        using (var archive = new ZipArchive(templateStream, ZipArchiveMode.Read, leaveOpen: true))
        using (var reader = new StreamReader(archive.GetEntry("xl/worksheets/sheet1.xml")!.Open()))
        {
            var worksheetXml = reader.ReadToEnd();
            worksheetXml.ShouldContain("<x:formula1>=HlgWinnerPublishedValues</x:formula1>");
            worksheetXml.ShouldNotContain("<x:formula1>TRUE,FALSE</x:formula1>");
        }
        templateStream.Position = 0;
        using var workbook = new XLWorkbook(templateStream);
        var input = workbook.Worksheet("TraoThuong");
        var events = workbook.Worksheet("DanhMucSuKien");
        var games = workbook.Worksheet("DanhMucGame");
        var prizes = workbook.Worksheet("DanhMucGiai");
        input.Cell(3, 1).Value = $"{rankingEvent.Title} | {rankingEvent.Id}";
        input.Cell(3, 2).Value = $"{game.Name} | {game.Id}";
        input.Cell(3, 3).Value = $"{prize.Title} | {prize.Id}";
        input.Cell(3, 4).Value = "0900123456";
        input.Cell(3, 5).Value = "TRUE";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var rows = new HlgWinnerExcelImporter().Read(stream);

        events.Cell(2, 1).GetString().ShouldContain(rankingEvent.Id.ToString());
        events.Cell(2, 4).GetString().ShouldBe("TRUE");
        events.Cell(3, 4).GetString().ShouldBe("FALSE");
        games.Cell(2, 1).GetString().ShouldContain(game.Id.ToString());
        prizes.Cell(2, 1).GetString().ShouldBe($"{prize.Title} | {prize.Id}");
        prizes.Cell(2, 2).GetString().ShouldBe("Giải nhất");
        prizes.Cell(2, 3).GetString().ShouldBe("Chai Dạ Hương 100ml");
        prizes.Cell(2, 5).GetValue<int>().ShouldBe(1);
        input.Column(4).Style.NumberFormat.Format.ShouldBe("@");
        // Dropdown GIẢI THƯỞNG (*) phải đúng cột C (bug cũ trỏ nhầm cột B = GAME).
        using (var archive2 = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
        using (var reader2 = new StreamReader(archive2.GetEntry("xl/worksheets/sheet1.xml")!.Open()))
        {
            var xml = reader2.ReadToEnd();
            xml.ShouldContain("<x:formula1>=HlgWinnerPrizeIds</x:formula1>");
        }
        rows.Single().EventId.ShouldContain(rankingEvent.Id.ToString());
        rows.Single().GameId.ShouldContain(game.Id.ToString());
        rows.Single().PrizeId.ShouldBe($"{prize.Title} | {prize.Id}");
        rows.Single().CustomerPhone.ShouldBe("0900123456");
        rows.Single().IsActive.ShouldBe("TRUE");
    }

    [Fact]
    public void Ranking_Result_Excel_Contains_Report_Columns_And_Preserves_Player_Identifiers()
    {
        var playedAt = new DateTime(2026, 9, 30, 8, 15, 0);
        using var content = new HlgRankingResultExcelExporter().Export("Sự kiện tháng 9", new[]
        {
            new HlgRankingResultExcelRow
            {
                EventRank = 1,
                CustomerCode = "00123",
                PlayerName = "Nguyễn Văn A",
                PhoneNumber = "0900123456",
                ZaloUserId = "zalo-1",
                PharmaPhone = "0912345678",
                GameName = "Đố vui",
                PlayCount = 3,
                GameScore = 2500,
                BestScore = 1000,
                CorrectAnswerCount = 25,
                TotalQuestionCount = 30,
                EventScore = 2500,
                FirstPlayedAt = playedAt,
                LastPlayedAt = playedAt.AddHours(2),
                RewardReceived = true,
                GamesCompleted = 2,
                GamesInCampaign = 3,
                RewardAddress = "Địa chỉ nhà thuốc"
            }
        });
        using var workbook = new XLWorkbook(content.GetStream());
        var sheet = workbook.Worksheet("Kết quả sự kiện");

        sheet.Row(1).Cells(1, 19).Select(c => c.GetString()).ShouldBe(new[]
        {
            "STT", "Xếp hạng sự kiện", "Mã người chơi", "Họ và tên", "Số điện thoại", "Zalo User ID",
            "SĐT Chủ nhà thuốc", "Tên trò chơi", "Số lượt chơi", "Điểm trò chơi", "Điểm cao nhất/lượt",
            "Số câu đúng", "Tổng số câu", "Tổng điểm sự kiện", "Lượt đầu tiên", "Lượt gần nhất",
            "Quà nhận được", "Đã tham gia", "Địa chỉ nhận quà"
        });
        sheet.LastColumnUsed()!.ColumnNumber().ShouldBe(19);
        sheet.Cell(2, 3).GetString().ShouldBe("00123");
        sheet.Cell(2, 3).DataType.ShouldBe(XLDataType.Text);
        sheet.Cell(2, 5).GetString().ShouldBe("0900123456");
        sheet.Cell(2, 6).GetString().ShouldBe("zalo-1");
        sheet.Cell(2, 7).GetString().ShouldBe("0912345678");
        sheet.Cell(2, 7).DataType.ShouldBe(XLDataType.Text);
        sheet.Cell(2, 7).Style.NumberFormat.Format.ShouldBe("@");
        sheet.Cell(2, 8).GetString().ShouldBe("Đố vui");
        sheet.Row(2).Cells(9, 14).Select(c => c.GetValue<int>()).ShouldBe(new[] { 3, 2500, 1000, 25, 30, 2500 });
        foreach (var cell in sheet.Row(2).Cells(9, 14))
        {
            cell.DataType.ShouldBe(XLDataType.Number);
            cell.Style.NumberFormat.Format.ShouldBe("#,##0");
        }
        sheet.Cell(2, 15).GetDateTime().ShouldBe(playedAt);
        sheet.Cell(2, 16).GetDateTime().ShouldBe(playedAt.AddHours(2));
        sheet.Cell(2, 15).Style.DateFormat.Format.ShouldBe("dd/MM/yyyy HH:mm:ss");
        sheet.Cell(2, 16).Style.DateFormat.Format.ShouldBe("dd/MM/yyyy HH:mm:ss");
        sheet.Cell(2, 17).GetString().ShouldBe("Đã nhận quà");
        sheet.Cell(2, 18).GetString().ShouldBe("2/3");
        sheet.Cell(2, 19).GetString().ShouldBe("Địa chỉ nhà thuốc");
        sheet.AutoFilter.Range.RangeAddress.LastAddress.ColumnNumber.ShouldBe(19);
        sheet.AutoFilter.Range.RangeAddress.LastAddress.RowNumber.ShouldBe(2);
        sheet.SheetView.SplitRow.ShouldBe(1);
    }

    [Fact]
    public async Task Ranking_Result_First_Export_Saves_All_Customer_Game_Rows()
    {
        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var finishedAt = DateTime.Today.AddDays(-2);
        var rankingEvent = new HlgRankingEvent(eventId, "Sự kiện cũ", finishedAt.AddDays(-5), finishedAt.AddDays(1), _tenant.Id);
        var customer = new Customer(customerId, "0900123456", "Nguyễn Văn A") { TenantId = _tenant.Id, CustomerCode = "C001", ZaloUserId = "zalo-1", BonusPoint = 250 };
        var game = new HlgGame(gameId, "Đố vui", HlgGameType.Quiz) { TenantId = _tenant.Id };
        var sessions = new[]
        {
            new HlgGameSession(Guid.NewGuid(), gameId, customerId, _tenant.Id) { IsFinished = true, FinishedAt = finishedAt, Score = 100, CorrectCount = 1, TotalQuestions = 2 },
            new HlgGameSession(Guid.NewGuid(), gameId, customerId, _tenant.Id) { IsFinished = true, FinishedAt = finishedAt.AddHours(1), Score = 150, CorrectCount = 2, TotalQuestions = 2 }
        };
        _rankingEvents.GetAsync(eventId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(rankingEvent);
        _sessions.GetQueryableAsync().Returns(Task.FromResult(sessions.AsQueryable()));
        _customers.GetQueryableAsync().Returns(Task.FromResult(new[] { customer }.AsQueryable()));
        _games.GetQueryableAsync().Returns(Task.FromResult(new[] { game }.AsQueryable()));
        var profiles = new[]
        {
            new HlgUserProfile(Guid.NewGuid(), customerId, _tenant.Id) { PharmaPhone = "0912345678" }
        };
        _profiles.GetQueryableAsync().Returns(Task.FromResult(profiles.AsQueryable()));

        using var content = await RankingService().ExportResultsAsync(eventId);
        using var workbook = new XLWorkbook(content.GetStream());
        workbook.Worksheet("Kết quả sự kiện").Cell(2, 7).GetString().ShouldBe("0912345678");

        await _rankingSnapshots.Received(1).InsertManyAsync(
            Arg.Is<IEnumerable<HlgRankingResultSnapshot>>(rows => rows.Single().EventScore == 250 && rows.Single().PlayCount == 2 && rows.Single().CustomerId == customerId && rows.Single().GameId == gameId),
            true,
            Arg.Any<CancellationToken>());
        customer.BonusPoint.ShouldBe(0);
        await _customers.Received(1).UpdateManyAsync(
            Arg.Is<IEnumerable<Customer>>(rows => rows.Single().Id == customerId && rows.Single().BonusPoint == 0),
            true,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false, null, false, "")]
    [InlineData(true, null, false, "")]
    [InlineData(true, "0912345678", false, "0912345678")]
    [InlineData(true, "0912345678", true, "")]
    public async Task Ranking_Result_Reexport_Preserves_Snapshot_And_Enriches_Current_Profile(
        bool hasProfile, string? pharmaPhone, bool isDeleted, string expectedPhone)
    {
        var eventId = Guid.NewGuid();
        var snapshot = new HlgRankingResultSnapshot(Guid.NewGuid(), eventId, Guid.NewGuid(), Guid.NewGuid(), _tenant.Id)
        {
            EventRank = 1,
            PlayerName = "Tên đã lưu",
            PhoneNumber = "0900000000",
            GameName = "Game đã lưu",
            PlayCount = 1,
            GameScore = 900,
            BestScore = 900,
            CorrectAnswerCount = 9,
            TotalQuestionCount = 10,
            EventScore = 900,
            FirstPlayedAt = DateTime.Today.AddDays(-3),
            LastPlayedAt = DateTime.Today.AddDays(-3)
        };
        var rankingEvent = new HlgRankingEvent(eventId, "Sự kiện cũ", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-2), _tenant.Id);
        _rankingEvents.GetAsync(eventId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(rankingEvent);
        _rankingSnapshots.GetQueryableAsync().Returns(Task.FromResult(new[] { snapshot }.AsQueryable()));
        var profiles = new List<HlgUserProfile>();
        if (hasProfile)
            profiles.Add(new HlgUserProfile(Guid.NewGuid(), snapshot.CustomerId, _tenant.Id)
            {
                PharmaPhone = pharmaPhone, PharmacyCode = "legacy-code", IsDeleted = isDeleted
            });
        // Cùng CustomerId nhưng tenant khác không được dùng thay cho profile thiếu/null/đã xóa.
        profiles.Add(new HlgUserProfile(Guid.NewGuid(), snapshot.CustomerId, Guid.NewGuid()) { PharmaPhone = "0999999999" });
        _profiles.GetQueryableAsync().Returns(Task.FromResult(profiles.AsQueryable()));

        using var content = await RankingService().ExportResultsAsync(eventId);
        using var workbook = new XLWorkbook(content.GetStream());

        workbook.Worksheet("Kết quả sự kiện").Cell(2, 4).GetString().ShouldBe("Tên đã lưu");
        workbook.Worksheet("Kết quả sự kiện").Cell(2, 5).GetString().ShouldBe("0900000000");
        workbook.Worksheet("Kết quả sự kiện").Cell(2, 7).GetString().ShouldBe(expectedPhone);
        workbook.Worksheet("Kết quả sự kiện").Cell(2, 14).GetValue<int>().ShouldBe(900);
        // Nhánh reexport KHÔNG tạo lại snapshot và KHÔNG reset điểm (dữ liệu đóng băng).
        // Thông tin vận hành/PharmaPhone vẫn lấy từ dữ liệu live.
        await _customers.DidNotReceive().UpdateManyAsync(Arg.Any<IEnumerable<Customer>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _rankingSnapshots.DidNotReceive().InsertManyAsync(Arg.Any<IEnumerable<HlgRankingResultSnapshot>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ranking_Report_Exports_Owner_Phone_For_Each_Player_Game_In_Current_Scope(bool host)
    {
        if (host)
        {
            _tenant.Id.Returns((Guid?)null);
            _tenant.IsAvailable.Returns(false);
        }
        var eventId = Guid.NewGuid();
        var finishedAt = DateTime.Today.AddDays(-2);
        var ev = new HlgRankingEvent(eventId, "Báo cáo", finishedAt.AddDays(-5), DateTime.Today.AddDays(5), _tenant.Id);
        var owner = new Customer(Guid.NewGuid(), "0912345678", "Chủ nhà thuốc") { TenantId = _tenant.Id, CustomerCode = "OWNER", BonusPoint = 600 };
        var employee = new Customer(Guid.NewGuid(), "0900123456", "Nhân viên") { TenantId = _tenant.Id, CustomerCode = "EMPLOYEE", BonusPoint = 600 };
        var games = Enumerable.Range(1, 2).Select(i => new HlgGame(Guid.NewGuid(), $"Game {i}", HlgGameType.Quiz)
        {
            TenantId = _tenant.Id, EndAt = finishedAt.AddDays(1)
        }).ToList();
        var customers = new[] { owner, employee };
        var sessions = customers.SelectMany(c => games.Select(g => new HlgGameSession(Guid.NewGuid(), g.Id, c.Id, _tenant.Id)
        {
            IsFinished = true, FinishedAt = finishedAt, Score = 100, CorrectCount = 1, TotalQuestions = 1
        })).ToList();
        var profiles = customers.Select(c => new HlgUserProfile(Guid.NewGuid(), c.Id, _tenant.Id) { PharmaPhone = owner.PhoneNumber }).ToList();
        // Cùng CustomerId ở scope khác: không được leak hoặc gây duplicate key.
        var otherTenant = host ? Guid.NewGuid() : (Guid?)null;
        profiles.Add(new HlgUserProfile(Guid.NewGuid(), employee.Id, otherTenant) { PharmaPhone = "0999999999" });
        _rankingEvents.GetAsync(eventId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(ev);
        _sessions.GetQueryableAsync().Returns(Task.FromResult(sessions.AsQueryable()));
        _customers.GetQueryableAsync().Returns(Task.FromResult(customers.AsQueryable()));
        _games.GetQueryableAsync().Returns(Task.FromResult(games.AsQueryable()));
        _profiles.GetQueryableAsync().Returns(Task.FromResult(profiles.AsQueryable()));

        // Chọn một game rồi xuất tất cả game: phải giữ filter và không nhân bản dòng khi gắn profile.
        foreach (var selectedGames in new[] { new List<Guid> { games[0].Id }, new List<Guid>() })
        {
            using var content = await RankingService().ExportReportAsync(eventId, selectedGames);
            using var workbook = new XLWorkbook(content.GetStream());
            var sheet = workbook.Worksheet("Kết quả sự kiện");
            var expectedRows = selectedGames.Count == 0 ? 4 : 2;
            sheet.LastRowUsed()!.RowNumber().ShouldBe(expectedRows + 1);
            foreach (var row in sheet.Rows(2, expectedRows + 1))
            {
                row.Cell(7).GetString().ShouldBe(owner.PhoneNumber);
                row.Cell(7).DataType.ShouldBe(XLDataType.Text);
                row.Cell(5).GetString().ShouldBe(row.Cell(3).GetString() == "OWNER" ? owner.PhoneNumber : employee.PhoneNumber);
                if (selectedGames.Count > 0) row.Cell(8).GetString().ShouldBe(games[0].Name);
            }
        }
        owner.BonusPoint.ShouldBe(600);
        employee.BonusPoint.ShouldBe(600);
        await _customers.DidNotReceive().UpdateManyAsync(Arg.Any<IEnumerable<Customer>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _rankingSnapshots.DidNotReceive().InsertManyAsync(Arg.Any<IEnumerable<HlgRankingResultSnapshot>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ranking_Empty_Export_Still_Has_Owner_Phone_Header_Without_Writes(bool report)
    {
        var eventId = Guid.NewGuid();
        var end = DateTime.Today.AddDays(-1);
        var ev = new HlgRankingEvent(eventId, "Không có người chơi", end.AddDays(-5), end, _tenant.Id);
        var games = new[]
        {
            new HlgGame(Guid.NewGuid(), "Game đã kết thúc", HlgGameType.Quiz) { TenantId = _tenant.Id, EndAt = end }
        };
        _rankingEvents.GetAsync(eventId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ev);
        _games.GetQueryableAsync().Returns(Task.FromResult(games.AsQueryable()));
        using var content = report
            ? await RankingService().ExportReportAsync(eventId, new List<Guid>())
            : await RankingService().ExportResultsAsync(eventId);
        using var workbook = new XLWorkbook(content.GetStream());
        var sheet = workbook.Worksheet("Kết quả sự kiện");
        sheet.Cell(1, 7).GetString().ShouldBe("SĐT Chủ nhà thuốc");
        sheet.LastRowUsed()!.RowNumber().ShouldBe(1);
        sheet.LastColumnUsed()!.ColumnNumber().ShouldBe(19);
        sheet.AutoFilter.Range.RangeAddress.LastAddress.ColumnNumber.ShouldBe(19);
        await _profiles.DidNotReceive().GetQueryableAsync();
        await _customers.DidNotReceive().UpdateManyAsync(Arg.Any<IEnumerable<Customer>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _rankingSnapshots.DidNotReceive().InsertManyAsync(Arg.Any<IEnumerable<HlgRankingResultSnapshot>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    private HlgGameSession ReportSession(Guid customerId, Guid gameId, int score, int correct, int total = 10)
        => new(Guid.NewGuid(), gameId, customerId, _tenant.Id)
        {
            IsFinished = true, StartedAt = _clock.Now.AddDays(-2).AddMinutes(-5),
            FinishedAt = _clock.Now.AddDays(-2), Score = score, CorrectCount = correct,
            TotalQuestions = total, AllowedWrongAnswers = 1
        };

    private Guid SetUpReportSessions(params HlgGameSession[] sessions)
    {
        var eventId = Guid.NewGuid();
        var ev = new HlgRankingEvent(eventId, "BEST RESULT", _clock.Now.AddDays(-10), _clock.Now.AddDays(5), _tenant.Id);
        var customers = sessions.Select(s => s.CustomerId).Distinct()
            .Select(id => new Customer(id, "0900123456", id.ToString("N"))
            {
                TenantId = _tenant.Id, CustomerCode = id.ToString("N"), BonusPoint = 600
            }).ToList();
        var games = sessions.Select(s => s.GameId).Distinct()
            .Select(id => new HlgGame(id, id.ToString("N"), HlgGameType.Quiz, _tenant.Id)
            {
                EndAt = _clock.Now.AddDays(-1)
            }).ToList();
        _rankingEvents.GetAsync(eventId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(ev);
        _sessions.GetQueryableAsync().Returns(Task.FromResult(sessions.AsQueryable()));
        _customers.GetQueryableAsync().Returns(Task.FromResult(customers.AsQueryable()));
        _games.GetQueryableAsync().Returns(Task.FromResult(games.AsQueryable()));
        return eventId;
    }

    private HlgSessionAnswer ReportAnswer(HlgGameSession session, int seconds)
        => new(Guid.NewGuid(), session.Id, Guid.NewGuid(), _tenant.Id) { TimeSpentSec = seconds };

    private static IXLRow ReportRow(XLWorkbook workbook, Guid customerId, Guid gameId)
        => workbook.Worksheet("Kết quả sự kiện").RowsUsed().Skip(1)
            .Single(r => r.Cell(3).GetString() == customerId.ToString("N") && r.Cell(8).GetString() == gameId.ToString("N"));

    [Theory]
    [InlineData(3, 87, 74)]
    [InlineData(2, 76, 71)]
    [InlineData(3, 76, 68)]
    public async Task Ranking_Report_Uses_One_Best_Result_Instead_Of_Accumulating_Passed_Attempts(int plays, int best, int lower)
    {
        var customer = Guid.NewGuid();
        var perfect = Guid.NewGuid();
        var sessions = new List<HlgGameSession>
        {
            ReportSession(customer, _gameId, best, 9),
            ReportSession(customer, _gameId, lower, 9),
            ReportSession(perfect, _gameId, 100, 10)
        };
        if (plays == 3) sessions.Add(ReportSession(customer, _gameId, 40, 5));
        sessions[1].FinishedAt = sessions[0].FinishedAt!.Value.AddHours(1);
        var eventId = SetUpReportSessions(sessions.ToArray());

        using var content = await RankingService().ExportReportAsync(eventId, new List<Guid>());
        using var workbook = new XLWorkbook(content.GetStream());
        var row = ReportRow(workbook, customer, _gameId);

        row.Cells(9, 14).Select(c => c.GetValue<int>()).ShouldBe(new[] { plays, best, best, 9, 10, best });
        row.Cell(2).GetValue<int>().ShouldBe(2);
        ReportRow(workbook, perfect, _gameId).Cell(2).GetValue<int>().ShouldBe(1);
        row.Cell(15).GetDateTime().ShouldBe(sessions[0].FinishedAt!.Value);
        row.Cell(16).GetDateTime().ShouldBe(sessions[1].FinishedAt!.Value);
        row.Cell(18).GetString().ShouldBe("1/1");
    }

    [Fact]
    public async Task Ranking_Report_Includes_Failed_Attempts_And_Preserves_Genuine_Zero()
    {
        var customer = Guid.NewGuid();
        var zero = Guid.NewGuid();
        var eventId = SetUpReportSessions(
            ReportSession(customer, _gameId, 35, 4), ReportSession(customer, _gameId, 67, 8),
            ReportSession(zero, _gameId, 0, 0), ReportSession(zero, _gameId, 0, 0));

        using var content = await RankingService().ExportReportAsync(eventId, new List<Guid>());
        using var workbook = new XLWorkbook(content.GetStream());
        var row = ReportRow(workbook, customer, _gameId);
        row.Cells(9, 14).Select(c => c.GetValue<int>()).ShouldBe(new[] { 2, 67, 67, 8, 10, 67 });
        row.Cell(18).GetString().ShouldBe("0/1");
        ReportRow(workbook, zero, _gameId).Cells(9, 14).Select(c => c.GetValue<int>())
            .ShouldBe(new[] { 2, 0, 0, 0, 10, 0 });
    }

    [Fact]
    public async Task Ranking_Report_Does_Not_Mix_Score_And_Correct_Count_From_Different_Attempts()
    {
        var customer = Guid.NewGuid();
        var eventId = SetUpReportSessions(ReportSession(customer, _gameId, 80, 7), ReportSession(customer, _gameId, 70, 8));
        using var content = await RankingService().ExportReportAsync(eventId, new List<Guid>());
        using var workbook = new XLWorkbook(content.GetStream());
        ReportRow(workbook, customer, _gameId).Cells(10, 14).Select(c => c.GetValue<int>())
            .ShouldBe(new[] { 80, 80, 7, 10, 80 });
    }

    [Theory]
    [InlineData("correct")]
    [InlineData("time")]
    [InlineData("missing-time")]
    [InlineData("finished")]
    [InlineData("id")]
    public async Task Ranking_Report_Selects_A_Coherent_Best_Result_With_Deterministic_Ties(string tie)
    {
        var customer = Guid.NewGuid();
        var first = ReportSession(customer, _gameId, 80, 8);
        // Different historical question counts make the chosen session visible in Excel.
        var second = ReportSession(customer, _gameId, 80, tie == "correct" ? 9 : 8, 12);
        if (tie == "id")
        {
            first = new HlgGameSession(Guid.Parse("00000000-0000-0000-0000-000000000002"), _gameId, customer, _tenant.Id)
            { IsFinished = true, FinishedAt = first.FinishedAt, Score = 80, CorrectCount = 8, TotalQuestions = 10 };
            second = new HlgGameSession(Guid.Parse("00000000-0000-0000-0000-000000000001"), _gameId, customer, _tenant.Id)
            { IsFinished = true, FinishedAt = second.FinishedAt, Score = 80, CorrectCount = 8, TotalQuestions = 12 };
        }
        if (tie == "finished") second.FinishedAt = first.FinishedAt!.Value.AddMinutes(-1);
        var eventId = SetUpReportSessions(first, second);
        var answers = new List<HlgSessionAnswer>();
        if (tie != "missing-time") answers.Add(ReportAnswer(first, 30));
        answers.Add(ReportAnswer(second, tie == "correct" ? 100 : tie == "time" ? 10 : 30));
        if (tie == "time") answers.Add(ReportAnswer(second, 10));
        var foreignAnswer = ReportAnswer(second, 999);
        foreignAnswer.TenantId = Guid.NewGuid();
        answers.Add(foreignAnswer);
        _answers.GetQueryableAsync().Returns(Task.FromResult(answers.AsQueryable()));

        using var content = await RankingService().ExportReportAsync(eventId, new List<Guid>());
        using var workbook = new XLWorkbook(content.GetStream());
        ReportRow(workbook, customer, _gameId).Cells(10, 14).Select(c => c.GetValue<int>())
            .ShouldBe(new[] { 80, 80, second.CorrectCount, 12, 80 });
    }

    [Fact]
    public async Task Ranking_Report_Ranks_By_Best_Score_Correct_Count_Then_Summed_Answer_Time()
    {
        var game = Guid.NewGuid();
        var sessions = new[]
        {
            ReportSession(Guid.NewGuid(), game, 90, 7),
            ReportSession(Guid.NewGuid(), game, 80, 9),
            ReportSession(Guid.NewGuid(), game, 80, 8),
            ReportSession(Guid.NewGuid(), game, 80, 8),
            ReportSession(Guid.NewGuid(), game, 80, 8)
        };
        var eventId = SetUpReportSessions(sessions);
        var answers = new[]
        {
            ReportAnswer(sessions[0], 100), ReportAnswer(sessions[1], 100),
            ReportAnswer(sessions[2], 3), ReportAnswer(sessions[2], 2),
            ReportAnswer(sessions[3], 2), ReportAnswer(sessions[3], 8)
        };
        _answers.GetQueryableAsync().Returns(Task.FromResult(answers.AsQueryable()));

        using var content = await RankingService().ExportReportAsync(eventId, new List<Guid>());
        using var workbook = new XLWorkbook(content.GetStream());
        for (var i = 0; i < sessions.Length; i++)
            ReportRow(workbook, sessions[i].CustomerId, game).Cell(2).GetValue<int>().ShouldBe(i + 1);
    }

    [Fact]
    public async Task Ranking_Report_Sums_Only_One_Best_Per_Selected_Game_And_Keeps_Existing_Scope()
    {
        var customer = Guid.NewGuid();
        var secondGame = Guid.NewGuid();
        var valid = new[]
        {
            ReportSession(customer, _gameId, 87, 9), ReportSession(customer, _gameId, 74, 9),
            ReportSession(customer, secondGame, 50, 6), ReportSession(customer, secondGame, 45, 5)
        };
        var unfinished = ReportSession(customer, _gameId, 999, 10); unfinished.IsFinished = false;
        var outsidePeriod = ReportSession(customer, _gameId, 999, 10); outsidePeriod.FinishedAt = _clock.Now.AddDays(-11);
        var foreignTenant = ReportSession(customer, _gameId, 999, 10); foreignTenant.TenantId = Guid.NewGuid();
        var eventId = SetUpReportSessions(valid.Concat(new[] { unfinished, outsidePeriod, foreignTenant }).ToArray());

        foreach (var selected in new[] { new List<Guid>(), new List<Guid> { _gameId } })
        {
            using var content = await RankingService().ExportReportAsync(eventId, selected);
            using var workbook = new XLWorkbook(content.GetStream());
            var row = ReportRow(workbook, customer, _gameId);
            var expectedTotal = selected.Count == 0 ? 137 : 87;
            row.Cells(9, 14).Select(c => c.GetValue<int>()).ShouldBe(new[] { 2, 87, 87, 9, 10, expectedTotal });
            workbook.Worksheet("Kết quả sự kiện").LastRowUsed()!.RowNumber().ShouldBe(selected.Count == 0 ? 3 : 2);
            if (selected.Count == 0)
                ReportRow(workbook, customer, secondGame).Cells(9, 14).Select(c => c.GetValue<int>())
                    .ShouldBe(new[] { 2, 50, 50, 6, 10, 137 });
        }
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
