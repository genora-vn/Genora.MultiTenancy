using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Text.RegularExpressions;

namespace Genora.MultiTenancy.Helpers;

public static class PhoneHelper
{
    public static string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        var clean = phone.Trim();
        if (clean.Length <= 7)
            return clean;

        var first = clean.Substring(0, Math.Min(4, clean.Length));
        var last = clean.Substring(clean.Length - 3, 3);
        return $"{first}***{last}";
    }

    public static string NormalizePhoneTo84(IStringLocalizer<MultiTenancyResource> _l, string input)
    {
        var s = (input ?? "").Trim();
        s = Regex.Replace(s, @"[\s\.\-\(\)]", "");

        if (string.IsNullOrWhiteSpace(s))
            throw ErrorHelper.BusinessError(_l, "Customer:PhoneRequired");

        if (s.StartsWith("+")) s = s.Substring(1);

        // Format đầu 0 sang 84
        if (s.StartsWith("0"))
        {
            var rest = s.Substring(1);
            return "84" + rest;
        }

        // Format để chuẩn 84 loại bỏ kiểu 840
        if (s.StartsWith("84"))
        {
            var rest = s.Substring(2);
            if (rest.StartsWith("0")) rest = rest.Substring(1);
            return "84" + rest;
        }

        // Xử lý khi file excel hoặc tạo mới mất số 0 (vd 974456114)
        if (Regex.IsMatch(s, @"^\d{9,10}$"))
        {
            return "84" + s;
        }

        throw ErrorHelper.BusinessError(
                _l,
                "Customer:PhoneInvalid",
                detailCode: "Customer:PhoneInvalid_Data",
                detailArgs: new { PhoneNumber = input }
            )
            .WithData("PhoneNumber", input);
    }
}