using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Helper tạo và xác thực chữ ký MAC cho Zalo Checkout SDK V1 (HMAC-SHA256)
/// </summary>
public static class ZaloMacHelper
{
    // ─────────────────────────────────────────────────────────────────────────
    // MAC cho createOrder (phía Backend tạo để trả cho Mini App)
    // Công thức: HMAC-SHA256(privateKey, appId|orderId|amount)
    // ─────────────────────────────────────────────────────────────────────────
    public static string GenerateCreateOrderMac(string privateKey, string appId, string orderId, long amount)
    {
        var rawData = $"{appId}|{orderId}|{amount}";
        return ComputeHmacSha256(privateKey, rawData);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MAC xác thực callback (Zalo gửi về, Backend verify)
    // Công thức: HMAC-SHA256(privateKey, appId|orderId|transId|amount|description|resultCode|message)
    // ─────────────────────────────────────────────────────────────────────────
    public static string GenerateCallbackMac(
        string privateKey,
        string appId,
        string orderId,
        string transId,
        long   amount,
        string description,
        int    resultCode,
        string message)
    {
        var rawData = $"{appId}|{orderId}|{transId}|{amount}|{description}|{resultCode}|{message}";
        return ComputeHmacSha256(privateKey, rawData);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // overallMac: tất cả fields của data sắp xếp theo thứ tự từ điển tăng dần
    // Công thức: HMAC-SHA256(privateKey, key1=val1&key2=val2&... sorted by key)
    // ─────────────────────────────────────────────────────────────────────────
    public static string GenerateOverallMac(string privateKey, IDictionary<string, string> fields)
    {
        var sorted   = fields.OrderBy(kv => kv.Key, StringComparer.Ordinal);
        var rawData  = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
        return ComputeHmacSha256(privateKey, rawData);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Xác thực MAC nhận từ callback
    // ─────────────────────────────────────────────────────────────────────────
    public static bool VerifyCallbackMac(
        string privateKey,
        string receivedMac,
        string appId,
        string orderId,
        string transId,
        long   amount,
        string description,
        int    resultCode,
        string message)
    {
        var expected = GenerateCallbackMac(privateKey, appId, orderId, transId, amount, description, resultCode, message);
        return string.Equals(expected, receivedMac, StringComparison.OrdinalIgnoreCase);
    }

    public static bool VerifyOverallMac(string privateKey, string receivedOverallMac, IDictionary<string, string> fields)
    {
        var expected = GenerateOverallMac(privateKey, fields);
        return string.Equals(expected, receivedOverallMac, StringComparison.OrdinalIgnoreCase);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Core HMAC-SHA256
    // ─────────────────────────────────────────────────────────────────────────
    private static string ComputeHmacSha256(string key, string data)
    {
        var keyBytes  = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
