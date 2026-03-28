using System;

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
}