using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Genora.MultiTenancy.Helpers;

/// <summary>
/// Resolve OriginalPrice theo loại ngày (Weekday/Weekend/Holiday/MemberDay) dựa trên cấu hình AppSpecialDates và PlayDate.
/// </summary>
public static class CustomerTypeOriginalPriceResolver
{
    public enum SpecialDateKind
    {
        Weekday = 0,
        Weekend = 1,
        Holiday = 2,
        MemberDay = 3
    }

    private const int AllWeekdaysMask = (1 << 7) - 1;

    /// <summary>
    /// Identify kind theo Name của AppSpecialDate (canonical).
    /// </summary>
    public static SpecialDateKind? IdentifyKind(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var n = name.Trim();
        if (n.Equals("Ngày trong tuần", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("Weekday", StringComparison.OrdinalIgnoreCase))
            return SpecialDateKind.Weekday;
        if (n.Equals("Ngày cuối tuần", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("Weekend", StringComparison.OrdinalIgnoreCase))
            return SpecialDateKind.Weekend;
        if (n.Equals("Ngày lễ", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("Ngay le", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("Holiday", StringComparison.OrdinalIgnoreCase))
            return SpecialDateKind.Holiday;
        if (n.Equals("Member day", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("Memberday", StringComparison.OrdinalIgnoreCase) ||
            n.Equals("MemberDay", StringComparison.OrdinalIgnoreCase))
            return SpecialDateKind.MemberDay;
        return null;
    }

    /// <summary>
    /// Resolve loại ngày của playDate dựa trên cấu hình SpecialDates.
    /// Priority: Holiday > MemberDay > Weekend > Weekday.
    /// MemberDay được ưu tiên hơn Weekday khi WeekdaysMask trùng (vd MemberDay=Thứ 5 trùng Weekday).
    /// </summary>
    public static SpecialDateKind ResolveKind(DateTime playDate, IEnumerable<SpecialDate>? specialDates)
    {
        if (specialDates == null) return SpecialDateKind.Weekday;

        var actives = specialDates.Where(x => x.IsActive).ToList();
        if (actives.Count == 0) return SpecialDateKind.Weekday;

        // 1) Holiday — kiểm tra DatesJson trước (specific date)
        var holiday = actives.FirstOrDefault(x => IdentifyKind(x.Name) == SpecialDateKind.Holiday);
        if (holiday != null && IsDateInHolidayList(playDate, holiday.DatesJson))
        {
            return SpecialDateKind.Holiday;
        }

        // 2) MemberDay — recurring weekday, override Weekday/Weekend nếu match
        var memberDay = actives.FirstOrDefault(x => IdentifyKind(x.Name) == SpecialDateKind.MemberDay);
        if (memberDay != null && IsWeekdayMatched(playDate, memberDay.WeekdaysMask))
        {
            return SpecialDateKind.MemberDay;
        }

        // 3) Weekend
        var weekend = actives.FirstOrDefault(x => IdentifyKind(x.Name) == SpecialDateKind.Weekend);
        if (weekend != null && IsWeekdayMatched(playDate, weekend.WeekdaysMask))
        {
            return SpecialDateKind.Weekend;
        }

        // 4) Weekday (default fallback hoặc match Weekday entry)
        return SpecialDateKind.Weekday;
    }

    /// <summary>
    /// Get OriginalPrice của CustomerType theo kind. Nếu kind không có giá → fallback về OriginalPrice (Weekday).
    /// </summary>
    public static decimal? GetOriginalPriceByKind(CustomerType? ct, SpecialDateKind kind)
    {
        if (ct == null) return null;

        decimal? primary = kind switch
        {
            SpecialDateKind.Weekend => ct.OriginalPriceWeekend,
            SpecialDateKind.Holiday => ct.OriginalPriceHoliday,
            SpecialDateKind.MemberDay => ct.OriginalPriceMemberDay,
            _ => ct.OriginalPrice
        };

        if (primary.HasValue && primary.Value > 0) return primary;

        // fallback Weekday nếu không có cấu hình giá riêng cho kind
        return ct.OriginalPrice;
    }

    /// <summary>
    /// Convert DateTime.DayOfWeek (Sun=0..Sat=6) sang index của AppSpecialDates (Mon=0..Sun=6).
    /// </summary>
    private static int ToAbpWeekdayIndex(DateTime date)
    {
        // .NET DayOfWeek: Sun=0, Mon=1, ..., Sat=6
        // ABP convention: Mon=0, Tue=1, Wed=2, Thu=3, Fri=4, Sat=5, Sun=6
        return ((int)date.DayOfWeek + 6) % 7;
    }

    private static bool IsWeekdayMatched(DateTime date, int? mask)
    {
        if (!mask.HasValue) return false;
        var m = mask.Value;
        if (m <= 0) return false;
        var idx = ToAbpWeekdayIndex(date);
        return ((m >> idx) & 1) == 1;
    }

    private static bool IsDateInHolidayList(DateTime date, string? datesJson)
    {
        if (string.IsNullOrWhiteSpace(datesJson)) return false;

        try
        {
            var arr = JsonSerializer.Deserialize<List<string>>(datesJson) ?? new List<string>();
            foreach (var s in arr)
            {
                if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                {
                    if (d.Date == date.Date) return true;
                }
                else if (DateTime.TryParse(s, out d))
                {
                    if (d.Date == date.Date) return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
