using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

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

    public static async Task InsertLogAsync(
        IRepository<ZaloLog, Guid> logRepo,
        string action,
        string endpoint,
        int? httpStatus,
        long durationMs,
        string? requestBody,
        string? responseBody,
        string? error,
        Guid? tenantId = null
    )
    {
        var log = new ZaloLog(Guid.NewGuid())
        {
            TenantId = tenantId,
            Action = action,
            Endpoint = endpoint,
            HttpStatus = httpStatus,
            DurationMs = durationMs,
            RequestBody = Truncate(MaskTokens(requestBody)),
            ResponseBody = Truncate(MaskTokens(responseBody)),
            Error = Truncate(error)
        };

        await logRepo.InsertAsync(log, autoSave: true);
    }
}