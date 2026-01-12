using System;
using Volo.Abp.Security.Encryption;

namespace Genora.MultiTenancy.Helpers;

public class SecurityHelper
{
    private const string Prefix = "enc:";

    public static string? EncryptMaybe(string? plain, IStringEncryptionService encrypt)
    {
        if (string.IsNullOrWhiteSpace(plain)) return plain;
        if (plain.StartsWith(Prefix, StringComparison.Ordinal)) return plain;
        return Prefix + encrypt.Encrypt(plain);
    }

    /// <summary>
    /// Nếu không có prefix enc: => coi là plaintext.
    /// Nếu có prefix mà decrypt fail => throw (strict).
    /// </summary>
    public static string? DecryptMaybe(string? value, IStringEncryptionService encrypt)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        if (!value.StartsWith(Prefix, StringComparison.Ordinal)) return value;
        return encrypt.Decrypt(value.Substring(Prefix.Length));
    }

    public static string MaskToken(string? plain)
    {
        if (string.IsNullOrWhiteSpace(plain)) return "";
        var t = plain.Trim();
        if (t.Length <= 10) return $"{t[0]}••••{t[^1]}";
        return $"{t.Substring(0, 4)}••••••••{t.Substring(t.Length - 4)}";
    }

    //public static string MaskToken(string s)
    //{
    //    if (string.IsNullOrWhiteSpace(s)) return s;
    //    if (s.Length <= 12) return "***";
    //    return s.Substring(0, 6) + "..." + s.Substring(s.Length - 6);
    //}

    public static string MaskCode(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        if (s.Length <= 8) return "***";
        return s.Substring(0, 4) + "..." + s.Substring(s.Length - 4);
    }

    public static string? MaskPhoneInResponse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;

        // mask nhẹ trường "number":"09xxxx"
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data)) return json;
            if (!data.TryGetProperty("number", out var numEl)) return json;

            var num = numEl.GetString();
            if (string.IsNullOrWhiteSpace(num) || num.Length < 6) return json;

            var masked = num.Substring(0, 3) + "****" + num.Substring(num.Length - 3);

            // replace chuỗi thô (đủ dùng cho log)
            return json.Replace(num, masked);
        }
        catch
        {
            return json;
        }
    }
}