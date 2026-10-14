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
    private readonly IManageImageService _images = Substitute.For<IManageImageService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly Hl25MiniAppCache catalogCache = new();

    public Hl25MiniAppTurnTests()
    {
        var participants = Substitute.For<IRepository<Hl25Participant, Guid>>();
        participants.GetQueryableAsync().Returns(Task.FromResult(new[] { _participant }.AsQueryable()));
        var frames = Substitute.For<IRepository<Hl25FrameCreation, Guid>>();
        frames.GetQueryableAsync().Returns(_ => Task.FromResult(_frames.AsQueryable()));
        frames.InsertAsync(Arg.Any<Hl25FrameCreation>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => { var frame = call.Arg<Hl25FrameCreation>(); _frames.Add(frame); return Task.FromResult(frame); });
        frames.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(_frames.FirstOrDefault(x => x.Id == call.Arg<Guid>())));
        var turns = Substitute.For<IRepository<Hl25SpinTurnLog, Guid>>();
        turns.InsertAsync(Arg.Any<Hl25SpinTurnLog>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => { var turn = call.Arg<Hl25SpinTurnLog>(); _turns.Add(turn); return Task.FromResult(turn); });
        var uowManager = Substitute.For<IUnitOfWorkManager>();
        uowManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(_uow);
        _provider = new ServiceCollection()
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .AddSingleton(Substitute.For<ICurrentTenant>())
            .AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance)
            .AddLogging()
            .BuildServiceProvider();
        _service = new MiniAppHl25Service(
            Substitute.For<IRepository<Hl25AppConfig, Guid>>(), participants,
            Substitute.For<IRepository<Hl25FrameCampaign, Guid>>(), Substitute.For<IRepository<Hl25FrameTemplate, Guid>>(),
            frames, turns, Substitute.For<IRepository<Hl25WheelConfig, Guid>>(), Substitute.For<IRepository<Hl25WheelSlot, Guid>>(),
            Substitute.For<IRepository<Hl25Gift, Guid>>(), Substitute.For<IRepository<Hl25SpinLog, Guid>>(),
            uowManager, _images, Substitute.For<IHttpContextAccessor>(),
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "S3",
                ["Storage:S3:PublicBaseUrl"] = "https://s3.test/bucket",
                ["Storage:S3:SubFolders:0"] = "hl25"
            }).Build(), catalogCache)
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

    [Fact]
    public async Task Renamed_S3_Frame_Deletes_Only_Temporary_Guid_After_Commit()
    {
        var source = $"https://s3.test/bucket/uploads/hl25/host/{Guid.NewGuid():N}.jpg";
        var steps = new List<string>();
        _images.CopyAsync(source, "host", "hl25", Arg.Any<string>())
            .Returns(call => Task.FromResult($"https://s3.test/bucket/uploads/hl25/host/{call.ArgAt<string>(3)}"));
        _uow.CompleteAsync(Arg.Any<CancellationToken>()).Returns(_ => { steps.Add("commit"); return Task.CompletedTask; });
        _images.DeleteFileAsync(source).Returns(_ => { steps.Add("delete"); return Task.CompletedTask; });

        var result = await _service.CreateFrameAsync(new Hl25CreateFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = source, RenameWithTimestamp = true });

        result.ResultImageUrl.ShouldMatch(@"/test-zalo-user_\d{14}\.jpg$");
        _frames.Single().ResultImageUrl.ShouldBe(result.ResultImageUrl);
        steps.ShouldBe(new[] { "commit", "delete" });
        await _images.Received(1).DeleteFileAsync(source);
        await _images.DidNotReceive().DeleteFileAsync(result.ResultImageUrl);
    }

    [Fact]
    public async Task Failed_Rename_Does_Not_Save_Frame_Or_Delete_Source()
    {
        var source = $"https://s3.test/bucket/uploads/hl25/host/{Guid.NewGuid():N}.jpg";
        _images.CopyAsync(source, "host", "hl25", Arg.Any<string>()).Returns(Task.FromResult<string>(null!));

        await Should.ThrowAsync<UserFriendlyException>(() => _service.CreateFrameAsync(new Hl25CreateFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = source, RenameWithTimestamp = true }));

        _frames.ShouldBeEmpty();
        await _images.DidNotReceive().DeleteFileAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task S3_Copy_Error_Returns_Upload_Error_Without_Deleting_Source()
    {
        var source = $"https://s3.test/bucket/uploads/hl25/host/{Guid.NewGuid():N}.jpg";
        _images.CopyAsync(source, "host", "hl25", Arg.Any<string>())
            .Returns(_ => Task.FromException<string>(new InvalidOperationException("S3 unavailable")));

        var error = await Should.ThrowAsync<UserFriendlyException>(() => _service.CreateFrameAsync(new Hl25CreateFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = source, RenameWithTimestamp = true }));

        error.Code.ShouldBe(Hl25ErrorCodes.UploadFailed);
        _frames.ShouldBeEmpty();
        await _images.DidNotReceive().DeleteFileAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Renamed_Frame_Does_Not_Delete_Source_Referenced_By_Existing_Frame()
    {
        var source = $"https://s3.test/bucket/uploads/hl25/host/{Guid.NewGuid():N}.jpg";
        _frames.Add(new Hl25FrameCreation(Guid.NewGuid(), _participant.Id, source));
        _images.CopyAsync(source, "host", "hl25", Arg.Any<string>())
            .Returns(call => Task.FromResult($"https://s3.test/bucket/uploads/hl25/host/{call.ArgAt<string>(3)}"));

        await _service.CreateFrameAsync(new Hl25CreateFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = source, RenameWithTimestamp = true });

        await _images.DidNotReceive().DeleteFileAsync(source);
    }

    [Fact]
    public async Task Failed_Temporary_Delete_Does_Not_Turn_Committed_Frame_Into_Error()
    {
        var source = $"https://s3.test/bucket/uploads/hl25/host/{Guid.NewGuid():N}.jpg";
        _images.CopyAsync(source, "host", "hl25", Arg.Any<string>())
            .Returns(call => Task.FromResult($"https://s3.test/bucket/uploads/hl25/host/{call.ArgAt<string>(3)}"));
        _images.DeleteFileAsync(source)
            .Returns(_ => Task.FromException(new InvalidOperationException("S3 delete unavailable")));

        var result = await _service.CreateFrameAsync(new Hl25CreateFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = source, RenameWithTimestamp = true });

        _frames.Single().ResultImageUrl.ShouldBe(result.ResultImageUrl);
        result.ResultImageUrl.ShouldMatch(@"/test-zalo-user_\d{14}\.jpg$");
        await _images.Received(1).DeleteFileAsync(source);
    }

    [Fact]
    public async Task Already_Named_Frame_Does_Not_Copy_Or_Delete_Again()
    {
        var source = "https://s3.test/bucket/uploads/hl25/host/test-zalo-user_20261014155831.jpg";

        var result = await _service.CreateFrameAsync(new Hl25CreateFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = source, RenameWithTimestamp = true });

        result.ResultImageUrl.ShouldBe(source);
        await _images.DidNotReceive().CopyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        await _images.DidNotReceive().DeleteFileAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Non_S3_Source_Is_Not_Deleted_After_Rename()
    {
        var source = "https://mini-app.test/uploads/hl25/host/card.jpg";
        _images.CopyAsync(source, "host", "hl25", Arg.Any<string>())
            .Returns(call => Task.FromResult($"/uploads/hl25/host/{call.ArgAt<string>(3)}"));

        await _service.CreateFrameAsync(new Hl25CreateFrameRequest
        { ZaloUserId = _participant.ZaloUserId!, ResultImageUrl = source, RenameWithTimestamp = true });

        await _images.DidNotReceive().DeleteFileAsync(source);
    }

    public void Dispose() { _provider.Dispose(); catalogCache.Dispose(); }
}
