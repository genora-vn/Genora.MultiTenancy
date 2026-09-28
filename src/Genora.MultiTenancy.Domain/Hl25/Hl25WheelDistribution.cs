using Genora.MultiTenancy.DomainModels.AppHl25;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp;

namespace Genora.MultiTenancy.Hl25;

/// <summary>Absolute percentages and a reproducible, stratified gift schedule.</summary>
public static class Hl25WheelDistribution
{
    // Existing campaigns replenish the same gifts daily. Pacing requires an explicit
    // campaign-wide target; otherwise preserve the configured absolute percentages.
    public const int DefaultTargetEligibleSpins = 0;
    private static readonly ConcurrentDictionary<string, Guid[]> Schedules = new();

    public static Hl25WheelConfig? SelectConfig(IReadOnlyList<Hl25WheelConfig> configs)
    {
        var active = configs.Where(c => c.IsActive).ToList();
        if (active.Count > 1 || (active.Count == 0 && configs.Count > 1))
            throw new BusinessException("Hl25:AmbiguousWheelConfig");
        return active.Count == 1 ? active[0] : configs.SingleOrDefault();
    }

    public static void Validate(IReadOnlyList<Hl25WheelSlot> slots)
    {
        if (slots.Count == 0 || slots.Any(s => s.WinRate < 0m || s.WinRate > 100m ||
            decimal.Round(s.WinRate, 4) != s.WinRate) || slots.Sum(s => s.WinRate) != 100m)
            throw new BusinessException("Hl25:WheelWinRateInvalid");

        if (slots.Count(s => !s.GiftId.HasValue) != 1 ||
            slots.Where(s => s.GiftId.HasValue).GroupBy(s => s.GiftId).Any(g => g.Count() > 1))
            throw new BusinessException("Hl25:InvalidWheelSlot");
    }

    /// <summary>Original weighted algorithm, with an absolute 0..100 roll and strict validation.</summary>
    public static Hl25WheelSlot PickWeighted(IReadOnlyList<Hl25WheelSlot> slots, double roll)
    {
        Validate(slots);
        if (roll < 0 || roll >= 1) throw new ArgumentOutOfRangeException(nameof(roll));
        var point = (decimal)roll * 100m;
        decimal cumulative = 0;
        foreach (var slot in slots)
        {
            cumulative += slot.WinRate;
            if (point < cumulative) return slot;
        }
        return slots[^1];
    }

    /// <summary>
    /// Assign each gift one unpredictable-looking position in each of its equal-sized windows.
    /// Positions are unique across gifts. For five 1.67% slots/50 gifts over 3000 eligible
    /// spins, each gift appears once in every 60-spin window.
    /// </summary>
    public static Hl25WheelSlot PickPaced(
        IReadOnlyList<Hl25WheelSlot> slots, IReadOnlyDictionary<Guid, int> giftQuantities,
        Guid wheelConfigId, long eligibleOrdinal, int targetEligibleSpins)
    {
        Validate(slots);
        if (targetEligibleSpins <= 0) throw new ArgumentOutOfRangeException(nameof(targetEligibleSpins));
        var noGift = slots.Single(s => !s.GiftId.HasValue);
        if (eligibleOrdinal <= 0) throw new ArgumentOutOfRangeException(nameof(eligibleOrdinal));
        if (eligibleOrdinal > targetEligibleSpins)
            return PickWeighted(slots, Random.Shared.NextDouble());

        var key = ScheduleKey(slots, giftQuantities, wheelConfigId, targetEligibleSpins);
        // Bounded process cache; a changed admin configuration gets a different key.
        if (Schedules.Count > 64) Schedules.Clear();
        var schedule = Schedules.GetOrAdd(key,
            _ => BuildSchedule(slots, giftQuantities, wheelConfigId, targetEligibleSpins));
        var selectedId = schedule[(int)eligibleOrdinal];
        return selectedId == Guid.Empty ? noGift : slots.Single(s => s.Id == selectedId);
    }

    private static Guid[] BuildSchedule(IReadOnlyList<Hl25WheelSlot> slots,
        IReadOnlyDictionary<Guid, int> giftQuantities, Guid wheelConfigId, int targetEligibleSpins)
    {
        var occupied = new Guid[targetEligibleSpins + 1];
        foreach (var slot in slots.Where(s => s.GiftId.HasValue).OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id))
        {
            var giftId = slot.GiftId!.Value;
            if (!giftQuantities.TryGetValue(giftId, out var quantity) || quantity < 0)
                throw new BusinessException("Hl25:InvalidWheelSlot");

            var target = Math.Min(quantity,
                (int)Math.Round(targetEligibleSpins * slot.WinRate / 100m, MidpointRounding.AwayFromZero));
            for (var window = 0; window < target; window++)
            {
                var start = (int)((long)window * targetEligibleSpins / target) + 1;
                var end = (int)((long)(window + 1) * targetEligibleSpins / target);
                var width = end - start + 1;
                var offset = StableOffset(wheelConfigId, slot.Id, window, width);
                var placed = false;
                for (var probe = 0; probe < width; probe++)
                {
                    var position = start + (offset + probe) % width;
                    if (occupied[position] == Guid.Empty)
                    {
                        occupied[position] = slot.Id;
                        placed = true;
                        break;
                    }
                }
                // Unequal gift rates can create overlapping narrow windows. Keep the
                // schedule valid by moving to the next free position in the campaign.
                for (var probe = 0; !placed && probe < targetEligibleSpins; probe++)
                {
                    var position = 1 + (start + offset + probe - 1) % targetEligibleSpins;
                    if (occupied[position] != Guid.Empty) continue;
                    occupied[position] = slot.Id;
                    placed = true;
                }
                if (!placed) throw new BusinessException("Hl25:InvalidWheelSlot");
            }
        }
        return occupied;
    }

    private static string ScheduleKey(IReadOnlyList<Hl25WheelSlot> slots,
        IReadOnlyDictionary<Guid, int> quantities, Guid configId, int targetSpins)
    {
        var key = new StringBuilder().Append(configId).Append('|').Append(targetSpins);
        foreach (var slot in slots.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id))
        {
            key.Append('|').Append(slot.Id).Append(':').Append(slot.DisplayOrder).Append(':')
                .Append(slot.GiftId).Append(':')
                .Append(slot.WinRate.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(slot.GiftId.HasValue && quantities.TryGetValue(slot.GiftId.Value, out var n) ? n : 0);
        }
        return key.ToString();
    }

    private static int StableOffset(Guid configId, Guid slotId, int window, int width)
    {
        Span<byte> source = stackalloc byte[36];
        configId.TryWriteBytes(source[..16]);
        slotId.TryWriteBytes(source.Slice(16, 16));
        BitConverter.TryWriteBytes(source.Slice(32, 4), window);
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(source, hash);
        return (int)(BitConverter.ToUInt32(hash[..4]) % (uint)width);
    }
}
