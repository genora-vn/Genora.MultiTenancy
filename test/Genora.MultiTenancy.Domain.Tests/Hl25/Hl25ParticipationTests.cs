using System;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Hl25;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Genora.MultiTenancy.Hl25;

public class Hl25ParticipationTests
{
    [Fact]
    public void Creating_Multiple_Cards_Grants_Only_First_Turn()
    {
        var participant = new Hl25Participant(Guid.NewGuid());
        participant.TryGrantFrameTurn().ShouldBeTrue();
        participant.TryGrantFrameTurn().ShouldBeFalse();
        participant.RemainingSpinTurns.ShouldBe(1);
        participant.TotalSpinTurns.ShouldBe(1);
        participant.EarnedCycles.ShouldBe(1);
    }

    [Fact]
    public void Sharing_Grants_Second_Turn_Only_Once_Even_After_First_Turn_Is_Spent()
    {
        var participant = new Hl25Participant(Guid.NewGuid());
        participant.TryGrantFrameTurn().ShouldBeTrue();
        participant.RemainingSpinTurns--;
        participant.TryGrantShareTurn().ShouldBeTrue();
        participant.TryGrantShareTurn().ShouldBeFalse();
        participant.TryGrantFrameTurn().ShouldBeFalse();
        participant.RemainingSpinTurns.ShouldBe(1);
        participant.TotalSpinTurns.ShouldBe(2);
        participant.EarnedCycles.ShouldBe(2);
    }

    [Fact]
    public void Sharing_Cannot_Grant_First_Turn()
    {
        var participant = new Hl25Participant(Guid.NewGuid());
        participant.TryGrantShareTurn().ShouldBeFalse();
        participant.TotalSpinTurns.ShouldBe(0);
    }

    [Fact]
    public void Admin_Grants_Do_Not_Consume_Automatic_Turns()
    {
        var participant = new Hl25Participant(Guid.NewGuid()) { RemainingSpinTurns = 5, TotalSpinTurns = 5 };
        participant.TryGrantFrameTurn().ShouldBeTrue();
        participant.TryGrantShareTurn().ShouldBeTrue();
        participant.TotalSpinTurns.ShouldBe(7);
        participant.RemainingSpinTurns.ShouldBe(7);
        participant.EarnedCycles.ShouldBe(2);
    }

    [Fact]
    public void Legacy_Participant_With_Two_Turns_Keeps_Their_Balance()
    {
        var participant = new Hl25Participant(Guid.NewGuid()) { EarnedCycles = 2, TotalSpinTurns = 2, RemainingSpinTurns = 1 };
        participant.TryGrantFrameTurn().ShouldBeFalse();
        participant.TryGrantShareTurn().ShouldBeFalse();
        participant.RemainingSpinTurns.ShouldBe(1);
    }

    [Fact]
    public void Overflow_Does_Not_Partially_Grant_A_Turn()
    {
        var participant = new Hl25Participant(Guid.NewGuid()) { TotalSpinTurns = int.MaxValue };
        Should.Throw<OverflowException>(() => participant.TryGrantFrameTurn());
        participant.EarnedCycles.ShouldBe(0);
        participant.RemainingSpinTurns.ShouldBe(0);
    }

    [Fact]
    public void Repeated_Delivery_Preserves_First_Confirmation_Time()
    {
        var spin = new Hl25SpinLog(Guid.NewGuid(), Guid.NewGuid()) { RewardStatus = Hl25RewardStatus.Won, GiftId = Guid.NewGuid() };
        var first = new DateTime(2026, 9, 14, 10, 0, 0);
        spin.ConfirmDelivery(first);
        spin.ConfirmDelivery(first.AddHours(1));
        spin.RewardStatus.ShouldBe(Hl25RewardStatus.Delivered);
        spin.DeliveredTime.ShouldBe(first);
    }

    [Theory]
    [InlineData(Hl25RewardStatus.NotWon)]
    [InlineData(Hl25RewardStatus.Pending)]
    [InlineData(Hl25RewardStatus.Cancelled)]
    public void Nonwinning_Spins_Cannot_Be_Delivered(Hl25RewardStatus status)
    {
        var spin = new Hl25SpinLog(Guid.NewGuid(), Guid.NewGuid()) { RewardStatus = status, GiftId = Guid.NewGuid() };
        Should.Throw<BusinessException>(() => spin.ConfirmDelivery(DateTime.Now)).Code.ShouldBe("Hl25:InvalidRewardTransition");
        spin.RewardStatus.ShouldBe(status);
        spin.DeliveredTime.ShouldBeNull();
    }

    [Fact]
    public void Winning_Spin_Without_Gift_Cannot_Be_Delivered()
    {
        var spin = new Hl25SpinLog(Guid.NewGuid(), Guid.NewGuid()) { RewardStatus = Hl25RewardStatus.Won };
        Should.Throw<BusinessException>(() => spin.ConfirmDelivery(DateTime.Now));
    }

    [Theory]
    [InlineData(10, 11)]
    [InlineData(-1, 0)]
    [InlineData(10, -1)]
    public void Invalid_Stock_Is_Rejected(int total, int remaining)
        => Should.Throw<BusinessException>(() => Hl25AdminRules.ValidateStock(total, remaining));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 0)]
    [InlineData(10, 10)]
    public void Valid_Stock_Is_Accepted(int total, int remaining)
        => Hl25AdminRules.ValidateStock(total, remaining);

    [Fact]
    public void Date_Range_Rejects_Reversed_Dates_And_Allows_Open_Ended_Ranges()
    {
        var day = new DateTime(2026, 9, 14);
        Should.Throw<BusinessException>(() => Hl25AdminRules.ValidateDateRange(day, day.AddDays(-1)));
        Hl25AdminRules.ValidateDateRange(day, day);
        Hl25AdminRules.ValidateDateRange(day, null);
        Hl25AdminRules.ValidateDateRange(null, day);
    }
}
