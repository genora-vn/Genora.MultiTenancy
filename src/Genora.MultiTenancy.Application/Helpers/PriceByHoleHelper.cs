using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Genora.MultiTenancy.Helpers;

public static class PriceByHoleHelper
{
    public static decimal GetPriceByNumberHoles(CalendarSlotPrice p, short? numberHoles)
    {
        return numberHoles switch
        {
            9 => p.Price9 ?? p.Price18,
            18 => p.Price18,
            27 => p.Price27 ?? p.Price18,
            36 => p.Price36 ?? p.Price18,
            _ => p.Price18
        };
    }

    public static decimal GetPriceByNumberHoles(AppCalendarSlotPriceDto p, short? numberHoles)
    {
        return numberHoles switch
        {
            9 => p.Price9 ?? p.Price18,
            18 => p.Price18,
            27 => p.Price27 ?? p.Price18,
            36 => p.Price36 ?? p.Price18,
            _ => p.Price18
        };
    }

    public static HashSet<int> ResolveSupportedHoles(string? numberHoles)
    {
        if (string.IsNullOrWhiteSpace(numberHoles))
            return new HashSet<int> { 9, 18 };

        var numbers = Regex.Matches(numberHoles, @"\d+")
            .Select(x => int.TryParse(x.Value, out var n) ? n : 0)
            .Where(x => x is 9 or 18 or 27 or 36)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        if (numbers.Count == 0)
            return new HashSet<int> { 9, 18 };

        var max = numbers.Max();

        return max switch
        {
            36 => new HashSet<int> { 9, 18, 27, 36 },
            27 => new HashSet<int> { 9, 18, 27 },
            18 => new HashSet<int> { 9, 18 },
            9 => new HashSet<int> { 9 },
            _ => new HashSet<int> { 9, 18 }
        };
    }

    public static void NormalizePricesByGolfCourse(
        CustomerTypeExcelRowDto price,
        GolfCourse golfCourse)
    {
        var supported = ResolveSupportedHoles(golfCourse.NumberHoles);

        if (!supported.Contains(9))
            price.Price9 = null;

        if (!supported.Contains(18))
            price.Price18 = 0m;

        if (!supported.Contains(27))
            price.Price27 = null;

        if (!supported.Contains(36))
            price.Price36 = null;
    }

    public static List<CustomerTypeExcelRowDto> NormalizePricesByGolfCourse(
        List<CustomerTypeExcelRowDto>? prices,
        GolfCourse golfCourse)
    {
        if (prices == null || prices.Count == 0)
            return new List<CustomerTypeExcelRowDto>();

        foreach (var p in prices)
        {
            NormalizePricesByGolfCourse(p, golfCourse);
        }

        return prices;
    }

    public static bool Requires18Holes(GolfCourse golfCourse)
    {
        var supported = ResolveSupportedHoles(golfCourse.NumberHoles);
        return supported.Contains(18);
    }
}

