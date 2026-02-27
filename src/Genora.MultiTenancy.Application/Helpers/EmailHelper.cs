using System;
using System.Linq;

namespace Genora.MultiTenancy.Helpers;

public class EmailHelper
{
    public static string NormalizeEmailList(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var parts = raw.Split(new[] { ';', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => x.Trim())
                       .Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join(";", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }
}