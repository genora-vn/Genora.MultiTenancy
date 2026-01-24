using System;
using System.Text.RegularExpressions;

namespace Genora.MultiTenancy.Helpers;

public static class ZaloLogHelper
{
    public static string? Truncate(string? s, int max = 8000)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max);
    }

    // Mask value nhưng vẫn giữ key để dễ đọc log
    public static string? MaskTokens(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;

        // JSON: "access_token":"...."
        s = Regex.Replace(s, "(\"access_token\"\\s*:\\s*\")([^\"]+)(\")", "$1***$3", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "(\"refresh_token\"\\s*:\\s*\")([^\"]+)(\")", "$1***$3", RegexOptions.IgnoreCase);

        // form/query: access_token=...&
        s = Regex.Replace(s, "(access_token=)([^&\\s]+)", "$1***", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "(refresh_token=)([^&\\s]+)", "$1***", RegexOptions.IgnoreCase);

        // secret header
        s = Regex.Replace(s, "(secret_key\\s*:\\s*)([^\\r\\n]+)", "$1***", RegexOptions.IgnoreCase);

        return s;
    }
}