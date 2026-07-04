using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Genora.MultiTenancy.AppServices.UrBox;

/// <summary>
/// Helper tạo chữ ký điện tử (Signature) cho API cartPayVoucher của UrBox.
/// Quy trình (khớp code tham khảo hệ thống cũ):
///   1. Serialize payload → JSON.
///   2. Sắp xếp các field top-level theo thứ tự alphabet (A→Z) tăng dần.
///   3. Compact JSON (không khoảng trắng).
///   4. Ký RSA-SHA256 (PKCS#1) bằng private key PEM.
///   5. Base64-encode kết quả → gán vào header "Signature".
/// </summary>
public static class UrBoxSignatureHelper
{
    // Không escape ký tự non-ASCII (giữ nguyên tiếng Việt/ký tự đặc biệt) để khớp cách UrBox verify.
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Sắp xếp field top-level theo alphabet + compact JSON. Trả về chuỗi JSON đã chuẩn hóa để ký
    /// và cũng dùng lưu RequestData trong DB.
    /// </summary>
    public static string BuildCanonicalJson(object payload)
    {
        // Serialize theo type runtime để [JsonPropertyName] có hiệu lực
        var jsonData = JsonSerializer.Serialize(payload, payload.GetType(), CompactOptions);
        var parsed = JsonNode.Parse(jsonData)!.AsObject();

        var sorted = new JsonObject();
        foreach (var property in parsed.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            // Detach node trước khi gán sang object mới (JsonNode không cho gán khi còn parent)
            sorted[property.Key] = property.Value?.DeepClone();
        }

        return sorted.ToJsonString(CompactOptions);
    }

    /// <summary>
    /// Ký chuỗi canonical bằng RSA private key PEM, trả về chữ ký Base64.
    /// </summary>
    public static string Sign(string canonicalJson, string privateKeyPath)
    {
        if (!File.Exists(privateKeyPath))
            throw new FileNotFoundException($"Không tìm thấy private key UrBox tại: {privateKeyPath}");

        var pem = File.ReadAllText(privateKeyPath);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem);

        var dataBytes = Encoding.UTF8.GetBytes(canonicalJson);
        var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signature);
    }

    /// <summary>
    /// Tạo chữ ký từ payload: build canonical JSON → ký. Trả về (signature, canonicalJson).
    /// </summary>
    public static (string Signature, string CanonicalJson) GenerateSignature(object payload, string privateKeyPath)
    {
        var canonicalJson = BuildCanonicalJson(payload);
        var signature = Sign(canonicalJson, privateKeyPath);
        return (signature, canonicalJson);
    }
}
