using DocumentFormat.OpenXml.Office2016.Excel;
using Genora.MultiTenancy.AppDtos.UrBox;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities;
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
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<UrBoxService> _logger;
    private readonly IBackgroundJobManager _jobManager;
    IStringLocalizer<MultiTenancyResource> l;

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
        IRepository<Customer, Guid> customerRepo,
        ICurrentTenant currentTenant,
        ILogger<UrBoxService> logger,
        IBackgroundJobManager jobManager,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _httpFactory = httpFactory;
        _settings = settings.Value;
        _giftExchangeRepo = giftExchangeRepo;
        _customerRepo = customerRepo;
        _currentTenant = currentTenant;
        _logger = logger;
        _jobManager = jobManager;
        this.l = l;
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

    public async Task<UrBoxGiftTransactionDetailDto?> GetGiftTransactionDetailAsync(Guid giftExchangeId)
    {
        // 1. Lấy bản ghi lịch sử đổi quà trong Genora
        var exchange = await _giftExchangeRepo.FindAsync(giftExchangeId);
        if (exchange == null) return null;

        var result = new UrBoxGiftTransactionDetailDto
        {
            Id = exchange.Id,
            ExchangeCode = exchange.ExchangeCode,
            CustomerCode = exchange.CustomerCode,
            CustomerName = exchange.CustomerName,
            CustomerPhone = exchange.CustomerPhone,
            Status = (int)exchange.Status,
            StatusText = GetExchangeStatusText(exchange.Status),
            PointsRequired = exchange.PointsRequired,
            Quantity = exchange.Quantity,
            TotalPointsUsed = exchange.TotalPointsUsed,
            CreationTime = exchange.CreationTime,
            GiftName = exchange.GiftName,
            GiftCode = exchange.GiftCode,
            GiftImageUrl = exchange.GiftImageUrl,
            VoucherCode = exchange.UrBoxVoucherCode
        };

        // 2. Cắt transaction_id từ InternalNote (format: "UrBox transaction_id=xxxx")
        var transactionId = ExtractTransactionId(exchange.InternalNote);
        result.TransactionId = transactionId;

        // 3. Gọi UrBox getByTransaction để lấy chi tiết voucher (code/QR/hạn dùng/người nhận)
        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            try
            {
                var cartResp = await GetCartByTransactionAsync(transactionId!);
                var cart = cartResp?.Data;
                if (cart != null && cartResp!.Status == UrBoxResponseStatus.Success)
                {
                    result.MoneyTotal = ParseDecimal(cart.MoneyTotal);
                    result.DeliveryStatus = cart.PayStatus;
                    result.ReceiverPhone = cart.Receiver?.Phone;
                    result.ReceiverEmail = cart.Receiver?.Email;
                    result.ReceiverAddress = cart.Receiver?.Address;

                    var item = cart.Detail?.FirstOrDefault();
                    if (item != null)
                    {
                        result.VoucherCode = item.Code ?? result.VoucherCode;
                        result.CodeImage = item.CodeImage;
                        result.CodeDisplay = item.CodeDisplay;
                        result.CodeDisplayType = item.CodeDisplayType;
                        result.Expired = item.Expired;
                        result.LinkGift = item.Link;
                        result.DeliveryStatus = item.Delivery ?? result.DeliveryStatus;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UrBox getByTransaction lỗi cho tx={Tx}", transactionId);
            }
        }

        // 4. Gọi UrBox gift detail (theo GiftCode) để lấy Note + danh sách Office + brand
        if (!string.IsNullOrWhiteSpace(exchange.GiftCode))
        {
            try
            {
                var detailResp = await GetGiftDetailAsync(exchange.GiftCode!, "vi");
                var detail = detailResp?.Data;
                if (detail != null && detailResp!.Status == UrBoxResponseStatus.Success)
                {
                    result.Note = detail.Note;
                    result.Content = detail.Content;
                    result.ExpireDuration = detail.ExpireDuration;
                    result.BrandName = detail.Brand;
                    result.BrandImage = detail.BrandImage;
                    if (string.IsNullOrWhiteSpace(result.GiftName)) result.GiftName = detail.Title;
                    if (string.IsNullOrWhiteSpace(result.GiftImageUrl)) result.GiftImageUrl = detail.Image;
                    if (detail.Office != null) result.Offices = detail.Office;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UrBox gift detail lỗi cho giftCode={Code}", exchange.GiftCode);
            }
        }

        return result;
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
            Status = HlGiftExchangeStatus.Processing,
            InternalNote = $"UrBox transaction_id={transactionId}"
        };

        // 5. Gọi UrBox
        var raw = await SendRawPostAsync(_settings.CartPayVoucherPath, requestData, signature, "CartPayVoucher");
        var result = Deserialize<UrBoxRedeemData>(raw);

        exchange.UrBoxResponse = Truncate(raw, 4000);

        // Thành công KHI: done == 1 && status == 200 (theo yêu cầu)
        var isSuccess = result != null && result.Done == 1 && result.Status == UrBoxResponseStatus.Success;

        if (!isSuccess)
        {
            exchange.Status = HlGiftExchangeStatus.Failed;
            exchange.InternalNote += $" | Lỗi: {result?.Msg ?? "Không parse được response"} (status={result?.Status})";
            await _giftExchangeRepo.InsertAsync(exchange, autoSave: true);

            _logger.LogWarning("UrBox redeem FAILED: tx={Tx} done={Done} status={Status} msg={Msg}",
                transactionId, result?.Done, result?.Status, result?.Msg);

            // Trả nguyên response UrBox (có msg + status) để Mini App hiển thị. KHÔNG trừ điểm.
            return result ?? new UrBoxResponse<UrBoxRedeemData>
            {
                Done = 0,
                Status = 500,
                Msg = UrBoxResponseStatus.GetMessage(500)
            };
        }

        // 6. Thành công → cập nhật voucher code + trạng thái
        var firstCode = result!.Data?.Cart?.CodeLinkGift?.FirstOrDefault();
        exchange.Status = HlGiftExchangeStatus.Success;
        exchange.UrBoxVoucherCode = firstCode?.Code;
        exchange.ApprovedAt = DateTime.Now;
        await _giftExchangeRepo.InsertAsync(exchange, autoSave: true);

        // Gán Id bản ghi HL.AppHlGiftExchanges vào response để Mini App gọi carts/{id}
        if (result.Data != null) result.Data.Id = exchange.Id;

        // 7. Trừ tiền thưởng (BonusAmount) khi đổi quà thành công (done==1 && status==200)
        await DeductBonusAmountAsync(input.SiteUserId, exchange);

        // ✅ gửi ZBS “Đổi quà thành công”
        if (!string.IsNullOrWhiteSpace(exchange.CustomerPhone))
        {
            try
            {
                var customer = await _customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == exchange.CustomerCode);
                var cartResp = await GetCartByTransactionAsync(transactionId!);
                var cart = cartResp?.Data;
                string? expired = "";
                if (cart != null && cartResp!.Status == UrBoxResponseStatus.Success)
                {
                    var item = cart.Detail?.FirstOrDefault();
                    if (item != null)
                    {
                        expired = item?.Expired;
                    }
                }
                await _jobManager.EnqueueAsync(
                    new ZbsSendJobArgs
                    {
                        TenantId = _currentTenant.Id,
                        TemplateKey = "ExchangeGift",
                        Phone = PhoneHelper.NormalizePhoneTo84(l, exchange.CustomerPhone),
                        TrackingId = exchange.CustomerCode,
                        TemplateData = new
                        {
                            exchange_code = exchangeCode,
                            customer_name = exchange.CustomerName,
                            customer_code = exchange.CustomerCode,
                            membership_tier = input.Rank,
                            gift_name = exchange.GiftName,
                            quantity = exchange.Quantity,
                            total_value = exchange.TotalPointsUsed,
                            expiry_date = expired
                        }
                    },
                    priority: BackgroundJobPriority.Normal
                );
            }
            catch
            {
                // không throw để không block luồng đăng ký
            }
        }

        _logger.LogInformation("UrBox redeem OK: tx={Tx} cartId={CartId} code={Code} exchangeId={ExId}",
            transactionId, result.Data?.Cart?.Id, firstCode?.Code, exchange.Id);

        return result;
    }

    /// <summary>
    /// Trừ tiền thưởng (BonusAmount) trong dbo.AppCustomers khi đổi quà UrBox thành công.
    /// Số tiền trừ = TotalPointsUsed (giá trị tiền của quà). Clamp >= 0. Bọc try/catch — không làm fail luồng đổi quà.
    /// </summary>
    private async Task DeductBonusAmountAsync(string customerCode, HlGiftExchange exchange)
    {
        if (string.IsNullOrWhiteSpace(customerCode)) return;
        var amount = (decimal)exchange.TotalPointsUsed;
        if (amount <= 0) return;

        try
        {
            var customer = await _customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == customerCode);
            if (customer == null)
            {
                _logger.LogWarning("UrBox redeem: không tìm thấy KH {Code} để trừ BonusAmount", customerCode);
                return;
            }

            customer.BonusAmount = Math.Max(0, customer.BonusAmount - amount);
            await _customerRepo.UpdateAsync(customer, autoSave: true);

            _logger.LogInformation("UrBox redeem: trừ BonusAmount KH {Code} -{Amount} → còn {Balance}",
                customerCode, amount, customer.BonusAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UrBox redeem: lỗi khi trừ BonusAmount KH {Code}", customerCode);
        }
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

    /// <summary>Cắt transaction_id từ InternalNote (format "UrBox transaction_id=xxxx"). Lấy phần sau dấu '='.</summary>
    private static string? ExtractTransactionId(string? internalNote)
    {
        if (string.IsNullOrWhiteSpace(internalNote)) return null;
        var idx = internalNote.IndexOf('=');
        if (idx < 0 || idx == internalNote.Length - 1) return null;
        // Lấy đoạn sau '=' đầu tiên, cắt tới khoảng trắng/dấu '|' nếu InternalNote có thêm ghi chú
        var after = internalNote[(idx + 1)..].Trim();
        var stop = after.IndexOfAny(new[] { ' ', '|' });
        return stop > 0 ? after[..stop] : after;
    }

    private static decimal? ParseDecimal(string? s)
        => decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : (decimal?)null;

    private static string GetExchangeStatusText(HlGiftExchangeStatus status) => status switch
    {
        HlGiftExchangeStatus.Failed => "Thất bại",
        HlGiftExchangeStatus.Success => "Thành công",
        HlGiftExchangeStatus.Processing => "Đang xử lý",
        HlGiftExchangeStatus.Used => "Đã sử dụng",
        _ => "Không xác định"
    };

    #endregion
}
