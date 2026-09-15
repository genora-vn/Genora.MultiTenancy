using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.AppServices.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25ReportTests : IDisposable
{
    private readonly Hl25ReportAppService _service;
    private readonly ServiceProvider _provider;
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IAbpAuthorizationService _auth = Substitute.For<IAbpAuthorizationService>();
    private readonly IFeatureChecker _features = Substitute.For<IFeatureChecker>();
    private readonly DateTime _day = new(2026, 9, 14);
    private readonly Guid _giftId = Guid.NewGuid();

    public Hl25ReportTests()
    {
        var person = Guid.NewGuid();
        var spins = new[]
        {
            Spin(person, Hl25RewardStatus.Won, _day.AddHours(23).AddMinutes(59)),
            Spin(person, Hl25RewardStatus.Delivered, _day.AddHours(12)),
            Spin(person, Hl25RewardStatus.Cancelled, _day.AddHours(13)),
            Spin(person, Hl25RewardStatus.Pending, _day.AddHours(14)),
            Spin(person, Hl25RewardStatus.NotWon, _day.AddHours(15)),
            Spin(person, Hl25RewardStatus.Won, _day.AddDays(1))
        };
        var spinRepository = Substitute.For<IRepository<Hl25SpinLog, Guid>>();
        spinRepository.GetQueryableAsync().Returns(Task.FromResult(spins.AsQueryable()));
        var giftRepository = Substitute.For<IRepository<Hl25Gift, Guid>>();
        giftRepository.GetQueryableAsync().Returns(Task.FromResult(new[]
        { new Hl25Gift(_giftId, "Test gift") { TotalQuantity = 10, RemainingQuantity = 6 } }.AsQueryable()));
        var turnRepository = Substitute.For<IRepository<Hl25SpinTurnLog, Guid>>();
        turnRepository.GetQueryableAsync().Returns(Task.FromResult(new[]
        {
            new Hl25SpinTurnLog(Guid.NewGuid(), person, Hl25SpinTurnSource.Other, 1) { GrantedTime = _day.AddHours(10) },
            new Hl25SpinTurnLog(Guid.NewGuid(), person, Hl25SpinTurnSource.AdminGrant, 3) { GrantedTime = _day.AddHours(23) },
            new Hl25SpinTurnLog(Guid.NewGuid(), person, Hl25SpinTurnSource.AdminGrant, 9) { GrantedTime = _day.AddDays(1) }
        }.AsQueryable()));
        var participantRepository = Substitute.For<IRepository<Hl25Participant, Guid>>();
        participantRepository.GetQueryableAsync().Returns(Task.FromResult(Array.Empty<Hl25Participant>().AsQueryable()));
        _auth.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>())
            .Returns(AuthorizationResult.Success());
        _provider = new ServiceCollection()
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .AddSingleton(_tenant)
            .AddSingleton<IAuthorizationService>(_auth)
            .BuildServiceProvider();
        _service = new Hl25ReportAppService(Substitute.For<IRepository<Hl25FrameCreation, Guid>>(), spinRepository,
            turnRepository, participantRepository, giftRepository, _features)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
    }

    private Hl25SpinLog Spin(Guid participantId, Hl25RewardStatus status, DateTime time)
        => new(Guid.NewGuid(), participantId) { GiftId = _giftId, RewardStatus = status, SpinTime = time };

    [Fact]
    public async Task Both_Reports_Count_Only_Wins_And_Include_Entire_End_Date()
    {
        var input = new Hl25ReportInput { FromDate = _day, ToDate = _day };
        var participation = await _service.GetWheelParticipationStatsAsync(input);
        var gifts = await _service.GetWheelGiftStatsAsync(input);
        participation.TotalSpins.ShouldBe(5);
        participation.TotalWins.ShouldBe(2);
        gifts.TotalWon.ShouldBe(participation.TotalWins);
        gifts.Rows.Single().DeliveredCount.ShouldBe(1);
        participation.TotalTurnsGranted.ShouldBe(4);
        participation.AutomaticTurnsGranted.ShouldBe(1);
        participation.AdminTurnsGranted.ShouldBe(3);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Uses_Permission_For_Current_Tenancy_Side(bool isTenant)
    {
        _tenant.IsAvailable.Returns(isTenant);
        _features.IsEnabledAsync(AppHl25Features.Management).Returns(true);
        await _service.GetWheelParticipationStatsAsync(new Hl25ReportInput());
        await _auth.Received().AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(),
            isTenant ? MultiTenancyPermissions.AppHl25Reports.Default : MultiTenancyPermissions.HostAppHl25Reports.Default);
    }

    [Fact]
    public async Task Disabled_Tenant_Feature_Is_Rejected()
    {
        _tenant.IsAvailable.Returns(true);
        _features.IsEnabledAsync(AppHl25Features.Management).Returns(false);
        await Should.ThrowAsync<AbpAuthorizationException>(() => _service.GetWheelParticipationStatsAsync(new Hl25ReportInput()));
    }

    public void Dispose() => _provider.Dispose();
}
