using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Genora.MultiTenancy.Helpers;

public static class DocumentSlugifier
{
    public static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark) continue;

            // Vietnamese đ/Đ → d
            if (ch == 'đ' || ch == 'Đ')
            {
                sb.Append('d');
                continue;
            }

            sb.Append(ch);
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        // Replace non-alphanumeric with dash
        result = Regex.Replace(result, @"[^a-z0-9]+", "-");
        result = result.Trim('-');

        if (result.Length > 200) result = result.Substring(0, 200).TrimEnd('-');

        return result;
    }

    public static string EnsureUnique(string baseSlug, Func<string, bool> exists)
    {
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "page";
        if (!exists(baseSlug)) return baseSlug;

        for (var i = 2; i < 1000; i++)
        {
            var candidate = $"{baseSlug}-{i}";
            if (!exists(candidate)) return candidate;
        }

        return $"{baseSlug}-{Guid.NewGuid():N}".Substring(0, 200);
    }
}
