using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.UrBox;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.UrBox;

/// <summary>
/// Service gọi API UrBox. Theo pattern HlApiClientService (HttpClient + System.Text.Json).
/// - Brand list: GET + query string.
/// - Còn lại: POST + JSON body (app_secret/app_id trong body).
/// - cartPayVoucher: POST + JSON body + header Signature (RSA-SHA256).
/// </summary>
public class UrBoxService : IUrBoxService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly UrBoxSettings _settings;
    private readonly IRepository<HlGiftExchange, Guid> _giftExchangeRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<UrBoxService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Body POST dùng camelCase mặc định; các field UrBox có [JsonPropertyName] snake_case riêng
    private static readonly JsonSerializerOptions BodyOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public UrBoxService(
        IHttpClientFactory httpFactory,
        IOptionsSnapshot<UrBoxSettings> settings,
        IRepository<HlGiftExchange, Guid> giftExchangeRepo,
        ICurrentTenant currentTenant,
        ILogger<UrBoxService> logger)
    {
        _httpFactory = httpFactory;
        _settings = settings.Value;
        _giftExchangeRepo = giftExchangeRepo;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    #region Brands (GET + query string)

    public async Task<UrBoxResponse<UrBoxPagedData<UrBoxBrandDto>>> GetBrandsAsync(int? catId = null, int? perPage = null, int? pageNo = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["app_secret"] = _settings.AppSecret,
            ["app_id"] = _settings.AppId,
            //["cat_id"] = (catId ?? 0).ToString(),
            ["page_no"] = (pageNo ?? 0).ToString(),
            ["per_page"] = (perPage ?? 0).ToString()
        };

        var url = BuildUrl(_settings.GiftBrandPath, query);
        return await SendGetAsync<UrBoxPagedData<UrBoxBrandDto>>(url, "Brand");
    }

    #endregion

    #region Categories / Gifts (GET + query string)

    public async Task<UrBoxResponse<List<UrBoxCategoryDto>>> GetCategoriesAsync(int? parentId = null, string? lang = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["app_secret"] = _settings.AppSecret,
            ["app_id"] = _settings.AppId,
            ["parent_id"] = (parentId ?? 0).ToString(),
            ["lang"] = string.IsNullOrWhiteSpace(lang) ? null : lang
        };

        var url = BuildUrl(_settings.CategoryListPath, query);
        return await SendGetAsync<List<UrBoxCategoryDto>>(url, "Category");
    }

    public async Task<UrBoxResponse<UrBoxPagedData<UrBoxGiftItemDto>>> GetGiftListAsync(
        string? catId = null, string? brandId = null, string? field = null, string? lang = null,
        int? stock = null, string? title = null, int? perPage = null, int? pageNo = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["app_secret"] = _settings.AppSecret,
            ["app_id"] = _settings.AppId,
            ["cat_id"] = string.IsNullOrWhiteSpace(catId) ? null : catId,
            ["brand_id"] = string.IsNullOrWhiteSpace(brandId) ? null : brandId,
            ["field"] = string.IsNullOrWhiteSpace(field) ? null : field,
            ["lang"] = string.IsNullOrWhiteSpace(lang) ? null : lang,
            ["stock"] = stock.HasValue && stock > 0 ? stock.ToString() : null,
            ["title"] = string.IsNullOrWhiteSpace(title) ? null : title,
            ["per_page"] = perPage.HasValue && perPage > 0 ? perPage.ToString() : null,
            ["page_no"] = pageNo.HasValue && pageNo > 0 ? pageNo.ToString() : null
        };

        var url = BuildUrl(_settings.GiftListPath, query);
        return await SendGetAsync<UrBoxPagedData<UrBoxGiftItemDto>>(url, "GiftList");
    }

    public async Task<UrBoxResponse<UrBoxGiftDetailDto>> GetGiftDetailAsync(string giftId, string? lang = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["app_secret"] = _settings.AppSecret,
            ["app_id"] = _settings.AppId,
            ["id"] = giftId,
            ["lang"] = string.IsNullOrWhiteSpace(lang) ? null : lang
        };

        var url = BuildUrl(_settings.GiftDetailPath, query);
        return await SendGetAsync<UrBoxGiftDetailDto>(url, "GiftDetail");
    }

    #endregion

    #region Cart lookup (GET + query string)

    public async Task<UrBoxResponse<List<UrBoxCartDto>>> GetCartListByUserAsync(string siteUserId)
    {
        var query = new Dictionary<string, string?>
        {
            ["app_secret"] = _settings.AppSecret,
            ["app_id"] = _settings.AppId,
            ["site_user_id"] = siteUserId
        };

        var url = BuildUrl(_settings.CartListByUserPath, query);
        return await SendGetAsync<List<UrBoxCartDto>>(url, "CartList");
    }

    public async Task<UrBoxResponse<UrBoxCartByTransactionDto>> GetCartByTransactionAsync(string transactionId)
    {
        var query = new Dictionary<string, string?>
        {
            ["app_secret"] = _settings.AppSecret,
            ["app_id"] = _settings.AppId,
            ["transaction_id"] = transactionId
        };

        var url = BuildUrl(_settings.CartByTransactionPath, query);
        return await SendGetAsync<UrBoxCartByTransactionDto>(url, "CartByTransaction");
    }

    #endregion

    #region Redeem eVoucher (POST + Signature)

    public async Task<UrBoxResponse<UrBoxRedeemData>> CreateOrderEvoucherAsync(UrBoxRedeemInput input)
    {
        var transactionId = GenerateTransactionId();

        // Chuẩn hóa SĐT: 84xxx → 0xxx
        var phone = NormalizePhone(input.Phone);

        var dataBuy = input.Items
            .Select(i => new UrBoxDataBuy { PriceId = i.PriceId, Quantity = (i.Quantity > 0 ? i.Quantity : 1).ToString() })
            .ToList();

        // 1. Payload gửi lên UrBox (có ttphone)
        var requestData = new UrBoxCartPayVoucherRequest
        {
            AppSecret = _settings.AppSecret,
            AppId = _settings.AppId,
            CampaignCode = _settings.CampaignCode,
            SiteUserId = input.SiteUserId,
            Ttphone = phone,
            TransactionId = transactionId,
            IsSendSms = _settings.IsSendSms,
            DataBuy = dataBuy
        };

        // 2. Payload để ký (KHÔNG có ttphone) → sort alphabet + compact JSON → RSA sign
        var signaturePayload = new UrBoxSignaturePayload
        {
            AppId = _settings.AppId,
            AppSecret = _settings.AppSecret,
            CampaignCode = _settings.CampaignCode,
            DataBuy = dataBuy,
            IsSendSms = _settings.IsSendSms,
            SiteUserId = input.SiteUserId,
            TransactionId = transactionId
        };

        var privateKeyPath = ResolvePrivateKeyPath();
        var (signature, canonicalJson) = UrBoxSignatureHelper.GenerateSignature(signaturePayload, privateKeyPath);

        // 3. Tính tổng điểm sử dụng (lưu lịch sử)
        var totalPoints = input.Items.Sum(i => i.PointsRequired * (i.Quantity > 0 ? i.Quantity : 1));
        var firstItem = input.Items.FirstOrDefault();

        // 4. Tạo bản ghi lịch sử đổi quà (Pending)
        var exchangeCode = $"UB-{DateTime.Now:yyMMdd}{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        var exchange = new HlGiftExchange(
            Guid.NewGuid(),
            exchangeCode,
            firstItem?.GiftName ?? "UrBox eVoucher",
            firstItem?.PointsRequired ?? 0,
            _currentTenant.Id)
        {
            CustomerCode = input.SiteUserId,
            CustomerName = input.CustomerName,
            CustomerPhone = phone,
            GiftCode = firstItem?.PriceId,
            GiftImageUrl = firstItem?.GiftImageUrl,
            Quantity = firstItem?.Quantity ?? 1,
            TotalPointsUsed = totalPoints,
            Status = HlGiftExchangeStatus.Pending,
            InternalNote = $"UrBox transaction_id={transactionId}"
        };

        // 5. Gọi UrBox
        var raw = await SendRawPostAsync(_settings.CartPayVoucherPath, requestData, signature, "CartPayVoucher");
        var result = Deserialize<UrBoxRedeemData>(raw);

        exchange.UrBoxResponse = Truncate(raw, 4000);

        if (result == null || result.Status != UrBoxResponseStatus.Success)
        {
            exchange.Status = HlGiftExchangeStatus.Rejected;
            exchange.InternalNote += $" | Lỗi: {result?.Msg ?? "Không parse được response"} (status={result?.Status})";
            await _giftExchangeRepo.InsertAsync(exchange, autoSave: true);

            _logger.LogWarning("UrBox redeem FAILED: tx={Tx} status={Status} msg={Msg}",
                transactionId, result?.Status, result?.Msg);

            // Trả nguyên response UrBox (có msg + status) để Mini App hiển thị
            return result ?? new UrBoxResponse<UrBoxRedeemData>
            {
                Done = 0,
                Status = 500,
                Msg = UrBoxResponseStatus.GetMessage(500)
            };
        }

        // 6. Thành công → cập nhật voucher code + trạng thái
        var firstCode = result.Data?.Cart?.CodeLinkGift?.FirstOrDefault();
        exchange.Status = HlGiftExchangeStatus.Approved;
        exchange.UrBoxVoucherCode = firstCode?.Code;
        exchange.ApprovedAt = DateTime.Now;
        await _giftExchangeRepo.InsertAsync(exchange, autoSave: true);

        _logger.LogInformation("UrBox redeem OK: tx={Tx} cartId={CartId} code={Code}",
            transactionId, result.Data?.Cart?.Id, firstCode?.Code);

        return result;
    }

    #endregion

    #region Private Helpers

    private async Task<UrBoxResponse<T>> SendGetAsync<T>(string url, string dataType)
    {
        try
        {
            var client = _httpFactory.CreateClient("UrBox");
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("UrBox GET {Url} → {Status} [{Type}]", url, (int)response.StatusCode, dataType);

            return Deserialize<T>(body) ?? FailResponse<T>(500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UrBox GET error: {Url}", url);
            return FailResponse<T>(500, ex.Message);
        }
    }

    /// <summary>Gửi POST JSON, kèm header Signature nếu có. Trả về raw response body.</summary>
    private async Task<string> SendRawPostAsync(string path, object body, string? signature, string dataType)
    {
        var client = _httpFactory.CreateClient("UrBox");
        var json = JsonSerializer.Serialize(body, body.GetType(), BodyOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        if (!string.IsNullOrWhiteSpace(signature))
            request.Headers.TryAddWithoutValidation("Signature", signature);

        var response = await client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("UrBox POST {Path} → {Status} [{Type}]", path, (int)response.StatusCode, dataType);
        return raw;
    }

    private static UrBoxResponse<T>? Deserialize<T>(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return JsonSerializer.Deserialize<UrBoxResponse<T>>(raw, JsonOptions);
    }

    private static UrBoxResponse<T> FailResponse<T>(int status, string? detail = null)
        => new()
        {
            Done = 0,
            Status = status,
            Msg = detail ?? UrBoxResponseStatus.GetMessage(status)
        };

    private static string BuildUrl(string path, Dictionary<string, string?> query)
    {
        var pairs = query
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}");
        var qs = string.Join("&", pairs);
        return string.IsNullOrEmpty(qs) ? path : $"{path}?{qs}";
    }

    private string ResolvePrivateKeyPath()
    {
        var path = _settings.PrivateKeyPath;
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(Directory.GetCurrentDirectory(), path);
    }

    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        phone = phone.Trim();
        if (phone.StartsWith("84") && phone.Length >= 11)
            return "0" + phone[2..];
        return phone;
    }

    private static string GenerateTransactionId() => Guid.NewGuid().ToString("N");

    private static string? Truncate(string? s, int max)
        => string.IsNullOrEmpty(s) ? s : (s.Length > max ? s[..max] + "...[truncated]" : s);

    #endregion
}
