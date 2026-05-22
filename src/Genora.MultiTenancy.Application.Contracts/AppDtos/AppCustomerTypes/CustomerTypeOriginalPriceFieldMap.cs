using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppCustomerTypes;

/// <summary>
/// Map giữa SpecialDate.Name (canonical) và field OriginalPrice tương ứng trong CustomerType.
/// Dùng cho UI Create/Edit modal để render input động theo cấu hình AppSpecialDates.
/// </summary>
public static class CustomerTypeOriginalPriceFieldMap
{
    public const string WeekdayField = nameof(CreateUpdateAppCustomerTypeDto.OriginalPrice);
    public const string WeekendField = nameof(CreateUpdateAppCustomerTypeDto.OriginalPriceWeekend);
    public const string HolidayField = nameof(CreateUpdateAppCustomerTypeDto.OriginalPriceHoliday);
    public const string MemberDayField = nameof(CreateUpdateAppCustomerTypeDto.OriginalPriceMemberDay);

    /// <summary>
    /// Resolve field name (DTO property) theo canonical SpecialDate name.
    /// </summary>
    public static string? ResolveField(string? specialDateName)
    {
        if (string.IsNullOrWhiteSpace(specialDateName)) return null;
        var name = specialDateName.Trim();

        if (Equals(name, "Ngày trong tuần") || Equals(name, "Weekday")) return WeekdayField;
        if (Equals(name, "Ngày cuối tuần") || Equals(name, "Weekend")) return WeekendField;
        if (Equals(name, "Ngày lễ") || Equals(name, "Ngay le") || Equals(name, "Holiday")) return HolidayField;
        if (Equals(name, "Member day") || Equals(name, "Memberday") || Equals(name, "MemberDay")) return MemberDayField;
        return null;
    }

    /// <summary>
    /// Resolve label cho UI input dựa trên canonical SpecialDate name.
    /// </summary>
    public static string ResolveLabel(string? specialDateName)
    {
        if (string.IsNullOrWhiteSpace(specialDateName)) return "Giá gốc";
        var name = specialDateName.Trim();

        if (Equals(name, "Ngày trong tuần") || Equals(name, "Weekday")) return "Giá gốc trong tuần";
        if (Equals(name, "Ngày cuối tuần") || Equals(name, "Weekend")) return "Giá gốc cuối tuần";
        if (Equals(name, "Ngày lễ") || Equals(name, "Ngay le") || Equals(name, "Holiday")) return "Giá gốc ngày lễ";
        if (Equals(name, "Member day") || Equals(name, "Memberday") || Equals(name, "MemberDay")) return "Giá gốc Member day";

        return "Giá gốc " + name;
    }

    /// <summary>
    /// Set value vào DTO theo field name.
    /// </summary>
    public static void SetValue(CreateUpdateAppCustomerTypeDto dto, string fieldName, decimal? value)
    {
        switch (fieldName)
        {
            case WeekdayField: dto.OriginalPrice = value; break;
            case WeekendField: dto.OriginalPriceWeekend = value; break;
            case HolidayField: dto.OriginalPriceHoliday = value; break;
            case MemberDayField: dto.OriginalPriceMemberDay = value; break;
        }
    }

    /// <summary>
    /// Get value từ DTO theo field name.
    /// </summary>
    public static decimal? GetValue(CreateUpdateAppCustomerTypeDto? dto, string fieldName)
    {
        if (dto == null) return null;
        return fieldName switch
        {
            WeekdayField => dto.OriginalPrice,
            WeekendField => dto.OriginalPriceWeekend,
            HolidayField => dto.OriginalPriceHoliday,
            MemberDayField => dto.OriginalPriceMemberDay,
            _ => null
        };
    }

    public static decimal? GetValue(AppCustomerTypeDto? dto, string fieldName)
    {
        if (dto == null) return null;
        return fieldName switch
        {
            WeekdayField => dto.OriginalPrice,
            WeekendField => dto.OriginalPriceWeekend,
            HolidayField => dto.OriginalPriceHoliday,
            MemberDayField => dto.OriginalPriceMemberDay,
            _ => null
        };
    }

    private static bool Equals(string a, string b)
        => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Mục cấu hình input động cho modal CustomerType.
/// </summary>
public class CustomerTypeOriginalPriceFieldDto
{
    public string SpecialDateName { get; set; } = default!;
    public string FieldName { get; set; } = default!;
    public string Label { get; set; } = default!;
    public decimal? Value { get; set; }
}
