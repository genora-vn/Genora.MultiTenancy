using System.Text.Json;

namespace Genora.MultiTenancy.Helpers;

public static class JsonHelper
{
    public static long ReadLongFlexible(JsonElement root, string propName, long defaultValue = 0)
    {
        if (!root.TryGetProperty(propName, out var el)) return defaultValue;

        if (el.ValueKind == JsonValueKind.Number)
            return el.GetInt64();

        if (el.ValueKind == JsonValueKind.String)
        {
            var s = el.GetString();
            if (long.TryParse(s, out var v)) return v;
        }

        return defaultValue;
    }
}