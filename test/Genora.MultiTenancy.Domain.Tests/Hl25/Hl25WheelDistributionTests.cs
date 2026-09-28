using Genora.MultiTenancy.DomainModels.AppHl25;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Xunit;
using Xunit.Abstractions;

namespace Genora.MultiTenancy.Hl25;

public class Hl25WheelDistributionTests
{
    private readonly ITestOutputHelper _output;
    public Hl25WheelDistributionTests(ITestOutputHelper output) => _output = output;

    private static (List<Hl25WheelSlot> Slots, Dictionary<Guid, int> Quantities) Fixture(Guid configId)
    {
        var slots = Enumerable.Range(0, 5).Select(i => new Hl25WheelSlot(Guid.NewGuid(), configId)
        { GiftId = Guid.NewGuid(), WinRate = 1.67m, DisplayOrder = i }).ToList();
        slots.Add(new Hl25WheelSlot(Guid.NewGuid(), configId) { WinRate = 91.65m, DisplayOrder = 5 });
        return (slots, slots.Where(s => s.GiftId.HasValue).ToDictionary(s => s.GiftId!.Value, _ => 50));
    }

    [Fact]
    public void Absolute_Weighted_Rates_Match_100000_Trials()
    {
        var (slots, _) = Fixture(Guid.NewGuid());
        var random = new Random(20260928);
        var counts = slots.ToDictionary(s => s.Id, _ => 0);
        for (var i = 0; i < 100_000; i++)
            counts[Hl25WheelDistribution.PickWeighted(slots, random.NextDouble()).Id]++;

        foreach (var slot in slots)
        {
            var expected = slot.WinRate * 1000m;
            var actual = counts[slot.Id];
            Assert.InRange(actual, (int)expected - (slot.GiftId.HasValue ? 200 : 450),
                (int)expected + (slot.GiftId.HasValue ? 200 : 450));
            _output.WriteLine($"weighted slot {slot.DisplayOrder}: {actual} / 100000 ({actual / 1000.0:F3}%)");
        }
    }

    [Fact]
    public void Current_Low_Rate_Config_Matches_005_Percent_Per_Gift_Over_One_Million_Spins()
    {
        var (slots, _) = Fixture(Guid.NewGuid());
        foreach (var slot in slots) slot.WinRate = slot.GiftId.HasValue ? 0.05m : 99.75m;
        var random = new Random(20260929);
        var counts = slots.ToDictionary(slot => slot.Id, _ => 0);
        for (var i = 0; i < 1_000_000; i++)
            counts[Hl25WheelDistribution.PickWeighted(slots, random.NextDouble()).Id]++;
        foreach (var slot in slots)
        {
            var expected = slot.GiftId.HasValue ? 500 : 997_500;
            Assert.InRange(counts[slot.Id], expected - 160, expected + 160);
            _output.WriteLine($"low-rate slot {slot.DisplayOrder}: {counts[slot.Id]} / 1000000");
        }
    }

    [Fact]
    public void Paced_Random_Positions_Distribute_50_Per_Gift_Across_3000_Eligible_Spins()
    {
        var at1600 = new List<int>();
        var at3000 = new List<int>();
        for (var batch = 0; batch < 200; batch++)
        {
            var configId = Guid.NewGuid();
            var (slots, quantities) = Fixture(configId);
            var counts = slots.ToDictionary(s => s.Id, _ => 0);
            for (var ordinal = 1; ordinal <= 3000; ordinal++)
            {
                var selected = Hl25WheelDistribution.PickPaced(slots, quantities, configId, ordinal, 3000);
                counts[selected.Id]++;
                if (ordinal == 1600) at1600.AddRange(slots.Where(s => s.GiftId.HasValue).Select(s => counts[s.Id]));
            }
            at3000.AddRange(slots.Where(s => s.GiftId.HasValue).Select(s => counts[s.Id]));
            Assert.Equal(2750, counts[slots.Single(s => !s.GiftId.HasValue).Id]);
        }

        Assert.All(at1600, count => Assert.InRange(count, 26, 27));
        Assert.All(at3000, count => Assert.Equal(50, count));
        Report("paced@1600", at1600);
        Report("paced@3000", at3000);
    }

    [Fact]
    public void Pure_Random_3000_Spin_Batches_Show_Natural_Variance()
    {
        var (slots, _) = Fixture(Guid.NewGuid());
        var at1600 = new List<int>();
        var at3000 = new List<int>();
        for (var batch = 0; batch < 500; batch++)
        {
            var random = new Random(10000 + batch);
            var counts = slots.ToDictionary(s => s.Id, _ => 0);
            for (var spin = 1; spin <= 3000; spin++)
            {
                counts[Hl25WheelDistribution.PickWeighted(slots, random.NextDouble()).Id]++;
                if (spin == 1600)
                    at1600.AddRange(slots.Where(s => s.GiftId.HasValue).Select(s => counts[s.Id]));
            }
            at3000.AddRange(slots.Where(s => s.GiftId.HasValue).Select(s => counts[s.Id]));
        }
        Report("pure@1600", at1600);
        Report("pure@3000", at3000);
        Assert.InRange(at1600.Average(), 25.5, 28.0);
        Assert.InRange(at3000.Average(), 48.5, 51.5);
    }

    [Fact]
    public void Paced_100000_Spins_Without_Stock_Decrement_Match_Configured_Rates()
    {
        var configId = Guid.NewGuid();
        var (slots, quantities) = Fixture(configId);
        var counts = slots.ToDictionary(s => s.Id, _ => 0);
        for (var spin = 1; spin <= 100_000; spin++)
            counts[Hl25WheelDistribution.PickPaced(slots, quantities, configId, spin, 3000).Id]++;

        foreach (var slot in slots)
        {
            var expected = (int)(slot.WinRate * 1000m);
            Assert.InRange(counts[slot.Id], expected - (slot.GiftId.HasValue ? 200 : 450),
                expected + (slot.GiftId.HasValue ? 200 : 450));
            _output.WriteLine($"paced100k slot {slot.DisplayOrder}: {counts[slot.Id]} / 100000 ({counts[slot.Id] / 1000.0:F3}%)");
        }
    }

    [Fact]
    public void Explicit_3000_Target_With_1000_Total_Gifts_Exhausts_Stock_Long_Before_30_October()
    {
        // An explicitly enabled 3,000-spin target paces that initial interval,
        // then resumes weighted random. Simulate five gifts with 200 pieces each.
        var depletionOrdinals = new List<int>();
        var awardsAt3000 = new List<int>();
        var awardsAt12000 = new List<int>();
        for (var batch = 0; batch < 100; batch++)
        {
            var configId = Guid.NewGuid();
            var (slots, quantities) = Fixture(configId);
            foreach (var giftId in quantities.Keys.ToList()) quantities[giftId] = 200;
            var won = slots.Where(s => s.GiftId.HasValue).ToDictionary(s => s.GiftId!.Value, _ => 0);
            var random = new Random(20260928 + batch);
            var finalOrdinal = 0;
            for (var ordinal = 1; ordinal <= 99_000; ordinal++)
            {
                // PickPaced delegates to PickWeighted with Random.Shared after 3,000;
                // an injected seeded roll makes the same production weighted algorithm reproducible.
                var selected = ordinal <= 3000
                    ? Hl25WheelDistribution.PickPaced(slots, quantities, configId, ordinal, 3000)
                    : Hl25WheelDistribution.PickWeighted(slots, random.NextDouble());
                if (selected.GiftId is Guid giftId && won[giftId] < quantities[giftId])
                    won[giftId]++;
                if (ordinal == 3000) awardsAt3000.AddRange(won.Values);
                if (ordinal == 12000) awardsAt12000.AddRange(won.Values);
                if (finalOrdinal == 0 && won.Values.All(value => value == 200))
                    finalOrdinal = ordinal;
                if (finalOrdinal != 0 && ordinal >= 12000) break;
            }
            Assert.True(finalOrdinal > 0, "All gifts should be exhausted before 99,000 eligible spins.");
            Assert.All(won.Values, value => Assert.Equal(200, value));
            depletionOrdinals.Add(finalOrdinal);
        }
        Assert.All(awardsAt3000, value => Assert.Equal(50, value));
        Report("200-per-gift@12000", awardsAt12000);
        Report("all-gifts-exhausted-spin", depletionOrdinals);
    }

    [Fact]
    public void Staging_Ten_Per_Gift_Produces_Ten_Wins_Over_3000_Not_167_Percent()
    {
        var configId = Guid.NewGuid();
        var (slots, quantities) = Fixture(configId);
        foreach (var giftId in quantities.Keys.ToList()) quantities[giftId] = 10;
        var counts = slots.Where(s => s.GiftId.HasValue).ToDictionary(s => s.GiftId!.Value, _ => 0);
        for (var ordinal = 1; ordinal <= 3000; ordinal++)
        {
            var selected = Hl25WheelDistribution.PickPaced(slots, quantities, configId, ordinal, 3000);
            if (selected.GiftId is Guid giftId) counts[giftId]++;
        }
        Assert.All(counts.Values, value => Assert.Equal(10, value));
    }

    [Fact]
    public void Campaign_Length_Target_Spreads_1000_Total_Gifts_Over_99000_Eligible_Spins()
    {
        var configId = Guid.NewGuid();
        var (slots, quantities) = Fixture(configId);
        foreach (var giftId in quantities.Keys.ToList()) quantities[giftId] = 200;
        var counts = slots.Where(s => s.GiftId.HasValue).ToDictionary(s => s.GiftId!.Value, _ => 0);
        for (var ordinal = 1; ordinal <= 99_000; ordinal++)
        {
            var selected = Hl25WheelDistribution.PickPaced(slots, quantities, configId, ordinal, 99_000);
            if (selected.GiftId is Guid giftId) counts[giftId]++;
            if (ordinal == 3000)
                Assert.All(counts.Values, value => Assert.InRange(value, 6, 7));
            if (ordinal == 12_000)
                Assert.All(counts.Values, value => Assert.InRange(value, 24, 25));
        }
        Assert.All(counts.Values, value => Assert.Equal(200, value));
    }

    [Fact]
    public void Invalid_Total_And_Ambiguous_Active_Config_Fail_Fast()
    {
        var (slots, _) = Fixture(Guid.NewGuid());
        slots[^1].WinRate = 45m;
        Assert.Throws<BusinessException>(() => Hl25WheelDistribution.PickWeighted(slots, 0.5));
        var configs = new[] { new Hl25WheelConfig(Guid.NewGuid()), new Hl25WheelConfig(Guid.NewGuid()) };
        Assert.Throws<BusinessException>(() => Hl25WheelDistribution.SelectConfig(configs));
        configs[0].IsActive = false;
        Assert.Same(configs[1], Hl25WheelDistribution.SelectConfig(configs));
    }

    private void Report(string label, List<int> values)
    {
        var mean = values.Average();
        var deviation = Math.Sqrt(values.Average(value => Math.Pow(value - mean, 2)));
        _output.WriteLine($"{label}: mean={mean:F3}, min={values.Min()}, max={values.Max()}, sd={deviation:F3}");
    }
}
