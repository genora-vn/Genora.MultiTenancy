using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Client gọi VietQR API để lấy chuỗi QR chuẩn EMVCo.
/// Đăng ký: services.AddHttpClient("VietQR") trong MultiTenancyApplicationModule.
///
/// Tài liệu: https://vietqr.io/danh-sach-api/generate-qr/
/// API free, không cần auth cho generate cơ bản.
/// </summary>
public class VietQrApiClient : ITransientDependency
{
    private const string BaseUrl      = "https://api.vietqr.io";
    private const string ImageBaseUrl = "https://img.vietqr.io/image";

    /// <summary>
    /// Deep link scheme VietQR — mở trực tiếp app ngân hàng từ Zalo Mini App.
    /// Cú pháp: vietqr://pay?app={bankCode}&ba={accountNo}&am={amount}&tn={note}&nn={accountName}
    /// </summary>
    private const string DeeplinkScheme = "vietqr://pay";

    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<VietQrApiClient> _logger;

    public VietQrApiClient(
        IHttpClientFactory httpFactory,
        ILogger<VietQrApiClient> logger)
    {
        _httpFactory = httpFactory;
        _logger      = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GenerateAsync — Gọi VietQR API lấy chuỗi QR EMVCo
    // Trả về null nếu lỗi (caller fallback sang imageUrl only)
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<VietQrResult?> GenerateAsync(VietQrRequest request)
    {
        try
        {
            var client = _httpFactory.CreateClient("VietQR");

            var payload = new
            {
                accountNo   = request.AccountNumber,
                accountName = request.AccountOwner,
                acqId       = request.BankBin,
                amount      = request.Amount,
                addInfo     = Truncate(request.AddInfo, 50),  // VietQR giới hạn 50 ký tự
                format      = "text",
                template    = "qr_only",
            };

            var response = await client.PostAsJsonAsync("/v2/generate", payload);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[VietQrApiClient] HTTP {Status} khi generate QR cho acqId={BankBin}", response.StatusCode, request.BankBin);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<VietQrApiResponse>();
            if (json?.Code != "00" || json.Data == null)
            {
                _logger.LogWarning("[VietQrApiClient] VietQR trả về lỗi: code={Code} desc={Desc}", json?.Code, json?.Desc);
                return null;
            }

            return new VietQrResult
            {
                QrCode     = json.Data.QrCode,
                QrImageUrl = BuildImageUrl(request),
                BankAppUrl = BuildDeeplink(request),
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[VietQrApiClient] Exception generate QR — fallback sang imageUrl");
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildFallback — Không cần API call, chỉ trả về imageUrl + deeplink
    // Dùng khi bankBin không xác định được hoặc khi cần tốc độ
    // ─────────────────────────────────────────────────────────────────────────
    public VietQrResult BuildFallback(VietQrRequest request)
    {
        return new VietQrResult
        {
            QrCode     = null,   // Không có raw QR string khi không gọi API
            QrImageUrl = BuildImageUrl(request),
            BankAppUrl = BuildDeeplink(request),
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string BuildImageUrl(VietQrRequest r)
    {
        // https://img.vietqr.io/image/{shortCode}-{accountNo}-qr_only.jpg?amount=...&addInfo=...&accountName=...
        var addInfo     = HttpUtility.UrlEncode(Truncate(r.AddInfo, 50));
        var accountName = HttpUtility.UrlEncode(r.AccountOwner);

        return $"{ImageBaseUrl}/{r.BankShortCode}-{r.AccountNumber}-qr_only.jpg" +
               $"?amount={r.Amount}&addInfo={addInfo}&accountName={accountName}";
    }

    //private static string BuildDeeplink(VietQrRequest r)
    //{
    //    // vietqr://pay?app={shortCode}&ba={accountNo}&am={amount}&tn={note}&nn={accountName}
    //    // Scheme này hoạt động trên Zalo Mini App để mở trực tiếp app ngân hàng.
    //    var tn  = HttpUtility.UrlEncode(Truncate(r.AddInfo, 50));
    //    var nn  = HttpUtility.UrlEncode(r.AccountOwner);
    //    var app = r.BankShortCode.ToLowerInvariant();

    //    return $"{DeeplinkScheme}?app={app}&ba={r.AccountNumber}&am={r.Amount}&tn={tn}&nn={nn}";
    //}

    private static string BuildDeeplink(VietQrRequest r)
    {
        // Sử dụng link https thay vì scheme vietqr://
        // Cấu trúc: https://dl.vietqr.io/pay?app={bank_id}&ba={account_no}&am={amount}&tn={note}

        var tn = HttpUtility.UrlEncode(Truncate(r.AddInfo, 50));
        var app = r.BankShortCode.ToLowerInvariant();

        // Dùng HTTPS link để vượt qua kiểm tra bảo mật -1403 của Zalo
        return $"https://dl.vietqr.io/pay?app={app}&ba={r.AccountNumber}&am={r.Amount}&tn={tn}";
    }

    private static string Truncate(string? s, int max)
        => string.IsNullOrWhiteSpace(s) ? string.Empty
           : s.Length <= max ? s
           : s[..max];

    // ─────────────────────────────────────────────────────────────────────────
    // DTOs nội bộ
    // ─────────────────────────────────────────────────────────────────────────

    private class VietQrApiResponse
    {
        [JsonPropertyName("code")]  public string? Code { get; set; }
        [JsonPropertyName("desc")]  public string? Desc { get; set; }
        [JsonPropertyName("data")]  public VietQrDataPayload? Data { get; set; }
    }

    private class VietQrDataPayload
    {
        [JsonPropertyName("qrCode")]     public string? QrCode    { get; set; }
        [JsonPropertyName("qrDataURL")]  public string? QrDataUrl { get; set; }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Input / Output models (public — dùng trong AppServices)
// ─────────────────────────────────────────────────────────────────────────────

public class VietQrRequest
{
    public string BankBin       { get; set; } = string.Empty;  // VD: "970423"
    public string BankShortCode { get; set; } = string.Empty;  // VD: "TPB"
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountOwner  { get; set; } = string.Empty;
    public long   Amount        { get; set; }
    public string AddInfo       { get; set; } = string.Empty;
}

public class VietQrResult
{
    /// <summary>Chuỗi QR chuẩn EMVCo (từ VietQR API). Null nếu API lỗi.</summary>
    public string? QrCode     { get; set; }

    /// <summary>URL ảnh QR CDN — luôn có giá trị.</summary>
    public string  QrImageUrl { get; set; } = string.Empty;

    /// <summary>Deep link mở app ngân hàng — luôn có giá trị.</summary>
    public string  BankAppUrl { get; set; } = string.Empty;
}
