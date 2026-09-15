using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25.MiniApp;
using Genora.MultiTenancy.AppServices.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25MiniAppTurnTests : IDisposable
{
    private readonly Hl25Participant _participant = new(Guid.NewGuid()) { ZaloUserId = "test-zalo-user" };
    private readonly List<Hl25FrameCreation> _frames = new();
    private readonly List<Hl25SpinTurnLog> _turns = new();
    private readonly MiniAppHl25Service _service;
    private readonly ServiceProvider _provider;

    public Hl25MiniAppTurnTests()
    {
        var participants = Substitute.For<IRepository<Hl25Participant, Guid>>();
        participants.GetQueryableAsync().Returns(Task.FromResult(new[] { _participant }.AsQueryable()));
        var frames = Substitute.For<IRepository<Hl25FrameCreation, Guid>>();
        frames.InsertAsync(Arg.Any<Hl25FrameCreation>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => { var frame = call.Arg<Hl25FrameCreation>(); _frames.Add(frame); return Task.FromResult(frame); });
        frames.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(_frames.FirstOrDefault(x => x.Id == call.Arg<Guid>())));
        var turns = Substitute.For<IRepository<Hl25SpinTurnLog, Guid>>();
        turns.InsertAsync(Arg.Any<Hl25SpinTurnLog>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => { var turn = call.Arg<Hl25SpinTurnLog>(); _turns.Add(turn); return Task.FromResult(turn); });
        var uowManager = Substitute.For<IUnitOfWorkManager>();
        uowManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(Substitute.For<IUnitOfWork>());
        _provider = new ServiceCollection()
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .AddSingleton(Substitute.For<ICurrentTenant>())
            .AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance)
            .BuildServiceProvider();
        _service = new MiniAppHl25Service(
            Substitute.For<IRepository<Hl25AppConfig, Guid>>(), participants,
            Substitute.For<IRepository<Hl25FrameCampaign, Guid>>(), Substitute.For<IRepository<Hl25FrameTemplate, Guid>>(),
            frames, turns, Substitute.For<IRepository<Hl25WheelConfig, Guid>>(), Substitute.For<IRepository<Hl25WheelSlot, Guid>>(),
            Substitute.For<IRepository<Hl25Gift, Guid>>(), Substitute.For<IRepository<Hl25SpinLog, Guid>>(),
            uowManager, Substitute.For<IManageImageService>(), Substitute.For<IHttpContextAccessor>(), new ConfigurationBuilder().Build())
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    }

    private Task<Hl25FrameResultDto> CreateFrame() => _service.CreateFrameAsync(new Hl25CreateFrameRequest
    { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = "https://example.test/card.png", WishMessage = "Hoa Linh 25" });

    private Task<Hl25ShareResultDto> Share(Guid frameId, Hl25SharePlatform platform = Hl25SharePlatform.Zalo)
        => _service.ShareFrameAsync(new Hl25ShareFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, FrameCreationId = frameId, SharePlatform = platform });

    [Fact]
    public async Task Multiple_Cards_And_Shares_Produce_Exactly_Two_Grant_Logs()
    {
        var first = await CreateFrame();
        var second = await CreateFrame();
        first.TurnGranted.ShouldBeTrue();
        second.TurnGranted.ShouldBeFalse();
        (await Share(first.FrameCreationId)).TurnGranted.ShouldBeTrue();
        (await Share(first.FrameCreationId, Hl25SharePlatform.Facebook)).TurnGranted.ShouldBeFalse();
        (await Share(second.FrameCreationId)).TurnGranted.ShouldBeFalse();
        _participant.RemainingSpinTurns.ShouldBe(2);
        _turns.Count.ShouldBe(2);
        _turns[0].Source.ShouldBe(Hl25SpinTurnSource.Other);
        _turns[1].Source.ShouldBe(Hl25SpinTurnSource.ShareZalo);
        _turns.Sum(x => x.TurnsAdded).ShouldBe(_participant.TotalSpinTurns);
    }

    [Theory]
    [InlineData(Hl25SharePlatform.None)]
    [InlineData((Hl25SharePlatform)255)]
    public async Task Invalid_Share_Platform_Does_Not_Grant_Or_Mark_Shared(Hl25SharePlatform platform)
    {
        var frame = await CreateFrame();
        var error = await Should.ThrowAsync<UserFriendlyException>(() => Share(frame.FrameCreationId, platform));
        error.Code.ShouldBe(Hl25ErrorCodes.InvalidSharePlatform);
        _participant.RemainingSpinTurns.ShouldBe(1);
        _turns.Count.ShouldBe(1);
        _frames.Single().SharePlatform.ShouldBe(Hl25SharePlatform.None);
    }

    [Fact]
    public async Task Sharing_Someone_Elses_Card_Is_Rejected_With_Stable_Error_Code()
    {
        var frame = await CreateFrame();
        _frames.Single().ParticipantId = Guid.NewGuid();
        var error = await Should.ThrowAsync<UserFriendlyException>(() => Share(frame.FrameCreationId));
        error.Code.ShouldBe(Hl25ErrorCodes.FrameNotOwned);
        error.Message.ShouldBe("Thiệp không thuộc về người dùng này.");
        _turns.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Missing_Zalo_Id_Returns_Code_And_Readable_Message()
    {
        var error = await Should.ThrowAsync<UserFriendlyException>(() => _service.GetMeAsync(""));
        error.Code.ShouldBe(Hl25ErrorCodes.MissingZaloUserId);
        error.Message.ShouldBe("Thiếu ZaloUserId.");
    }

    [Fact]
    public async Task Oversized_Image_Returns_Stable_Error_Code()
    {
        var image = Substitute.For<IRemoteStreamContent>();
        image.ContentLength.Returns(Hl25Consts.MaxCardImageSizeBytes + 1);
        var error = await Should.ThrowAsync<UserFriendlyException>(() => _service.UploadImageAsync(image));
        error.Code.ShouldBe(Hl25ErrorCodes.ImageTooLarge);
    }

    public void Dispose() => _provider.Dispose();
}
