using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Settings;

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

    public static async Task<(string To, string? Cc, string? Bcc, string Subject)> GetEmailConfigAsync(ISettingProvider _settingProvider,
        string toKey, string ccKey, string bccKey, string subjectKey,
        string bookingCode, string fallbackTo)
    {
        var to = await _settingProvider.GetOrNullAsync(toKey);
        var cc = await _settingProvider.GetOrNullAsync(ccKey);
        var bcc = await _settingProvider.GetOrNullAsync(bccKey);
        var subjectTpl = await _settingProvider.GetOrNullAsync(subjectKey);

        var toFinal = EmailHelper.NormalizeEmailList(to);
        if (string.IsNullOrWhiteSpace(toFinal)) toFinal = EmailHelper.NormalizeEmailList(fallbackTo);

        return (
            To: toFinal,
            Cc: NullIfEmpty(EmailHelper.NormalizeEmailList(cc)),
            Bcc: NullIfEmpty(EmailHelper.NormalizeEmailList(bcc)),
            Subject: ApplySubjectTemplate(subjectTpl, bookingCode)
        );
    }

    private static string? NullIfEmpty(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Trim();
    }

    private static string ApplySubjectTemplate(string? template, string bookingCode)
    {
        template ??= "{BookingCode}";
        return template.Replace("{BookingCode}", bookingCode ?? "");
    }
}