using DocumentFormat.OpenXml.Office2016.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh;
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
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
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
    private readonly IHlPointAppService _hlPoint;
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
        IHlPointAppService hlPoint,
        ICurrentTenant currentTenant,
        ILogger<UrBoxService> logger,
        IBackgroundJobManager jobManager,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _httpFactory = httpFactory;
        _settings = settings.Value;
        _giftExchangeRepo = giftExchangeRepo;
        _customerRepo = customerRepo;
        _hlPoint = hlPoint;
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

        // 2. Cắt transaction_id từ InternalNote (format: "UrBox transaction_id=xxxx") — giữ để trả về model.
        var transactionId = ExtractTransactionId(exchange.InternalNote);
        result.TransactionId = transactionId;

        // 3. Lấy chi tiết voucher TỪ UrBoxResponse đã lưu lúc đổi quà (response cartPayVoucher).
        //    API getByTransaction bên đối tác NGƯNG hoạt động → KHÔNG gọi API nữa, đọc từ DB.
        //    UrBoxResponse là JSON của cartPayVoucher: data.cart.code_link_gift[] chứa đủ code/QR/hạn dùng/link.
        if (!string.IsNullOrWhiteSpace(exchange.UrBoxResponse))
        {
            try
            {
                var payResp = Deserialize<UrBoxRedeemData>(exchange.UrBoxResponse);
                var cart = payResp?.Data?.Cart;
                if (cart != null && payResp!.Status == UrBoxResponseStatus.Success)
                {
                    result.MoneyTotal = ParseDecimal(cart.MoneyTotal);

                    // Map TẤT CẢ voucher trong giao dịch (mỗi giao dịch có thể đổi số lượng > 1).
                    var codes = cart.CodeLinkGift;
                    if (codes != null && codes.Count > 0)
                    {
                        result.Vouchers = codes.Select(d => new UrBoxVoucherDetailDto
                        {
                            Code = d.Code,
                            CodeImage = d.CodeImage,
                            CodeDisplay = d.CodeDisplay,
                            CodeDisplayType = d.CodeDisplayType,
                            Expired = d.Expired,
                            Link = d.Link,
                            Delivery = d.DeliveryCode?.ToString()
                        }).ToList();

                        // Giữ field voucher ĐẦU TIÊN để tương thích ngược với client cũ.
                        var item = codes.First();
                        result.VoucherCode = item.Code ?? result.VoucherCode;
                        result.CodeImage = item.CodeImage;
                        result.CodeDisplay = item.CodeDisplay;
                        result.CodeDisplayType = item.CodeDisplayType;
                        result.Expired = item.Expired;
                        result.LinkGift = item.Link;

                        // Người nhận: cartPayVoucher trả ttphone/ttemail/ttaddress trong từng code_link_gift.
                        result.ReceiverPhone = item.TtPhone ?? result.ReceiverPhone;
                        result.ReceiverEmail = item.TtEmail ?? result.ReceiverEmail;
                        result.ReceiverAddress = item.TtAddress ?? result.ReceiverAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "UrBox parse UrBoxResponse lỗi cho exchangeId={Id}", exchange.Id);
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
        // ── 0. Xác định SĐT — KHÓA của quỹ tiền thưởng (1 SĐT = 1 quỹ, nhiều mã KH DMS) ──
        var phone = NormalizePhone(input.Phone);
        if (string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(input.SiteUserId))
        {
            // FE gửi siteUserId = mã KH DMS. Thiếu SĐT thì tra ngược từ AppCustomers.
            var byCode = await _customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == input.SiteUserId);
            phone = NormalizePhone(byCode?.PhoneNumber);
        }
        if (string.IsNullOrWhiteSpace(phone))
            return RedeemFail(400, "Thiếu số điện thoại khách hàng — không xác định được quỹ tiền thưởng.");

        // ── 1. F3: GIÁ LẤY TỪ URBOX (server). KHÔNG tin PointsRequired do client gửi ──
        //    Mọi call API ngoài phải xong TRƯỚC khi vào lock trừ tiền trong SpendAsync.
        decimal amountUsed = 0;
        foreach (var item in input.Items)
        {
            if (string.IsNullOrWhiteSpace(item.PriceId))
                return RedeemFail(400, "Thiếu mã mệnh giá quà tặng (priceId).");

            var qty = item.Quantity > 0 ? item.Quantity : 1;
            var unitPrice = await ResolveServerPriceAsync(item.PriceId);
            if (unitPrice is null || unitPrice <= 0)
                return RedeemFail(400,
                    $"Không tra được giá quà tặng từ UrBox (priceId={item.PriceId}). Vui lòng thử lại.");

            var lineTotal = unitPrice.Value * qty;
            amountUsed += lineTotal;

            // Client gửi PointsRequired = đơn giá × số lượng. Lệch = app cũ/cache/giả mạo.
            if (item.PointsRequired > 0 && item.PointsRequired != lineTotal)
                _logger.LogWarning(
                    "UrBox redeem: giá client lệch giá server. phone={Phone} priceId={PriceId} qty={Qty} client={Client} server={Server}",
                    phone, item.PriceId, qty, item.PointsRequired, lineTotal);
        }
        if (amountUsed <= 0)
            return RedeemFail(400, "Giá trị đơn đổi quà không hợp lệ.");

        // ── 2. F4: idempotency. Mini App chưa gửi requestId → tự sinh fingerprint theo
        //    (SĐT + giỏ quà + tổng tiền + mốc 2 phút) để chặn double-submit / retry mạng.
        //    Unique index (TenantId, RequestId) là chốt cuối khi 2 request vào đúng cùng lúc.
        var requestId = string.IsNullOrWhiteSpace(input.RequestId)
            ? BuildAutoRequestId(phone, input.Items, amountUsed)
            : input.RequestId!.Trim();

        var duplicate = await _giftExchangeRepo.FirstOrDefaultAsync(x => x.RequestId == requestId);
        if (duplicate != null)
        {
            _logger.LogWarning("UrBox redeem: chặn yêu cầu trùng requestId={Req} exchange={Code} phone={Phone}",
                requestId, duplicate.ExchangeCode, phone);
            return RedeemFail(409,
                "Yêu cầu đổi quà này đã được ghi nhận. Vui lòng kiểm tra lịch sử đổi quà trước khi thử lại.");
        }

        var transactionId = GenerateTransactionId();

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

        // ── 3. Tạo bản ghi Processing và COMMIT TRƯỚC khi gọi UrBox (F6) ──
        //    Process chết sau khi UrBox đã phát hành voucher thì vẫn còn dấu vết đối soát.
        var firstItem = input.Items.FirstOrDefault();
        var exchangeCode = $"UB-{DateTime.Now:yyMMdd}{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        var exchange = new HlGiftExchange(
            Guid.NewGuid(),
            exchangeCode,
            firstItem?.GiftName ?? "UrBox eVoucher",
            (int)amountUsed, // giá SERVER, không phải số client gửi
            _currentTenant.Id)
        {
            RequestId = requestId,
            CustomerCode = input.SiteUserId,
            CustomerName = input.CustomerName,
            CustomerPhone = phone,
            GiftCode = firstItem?.PriceId,
            GiftImageUrl = firstItem?.GiftImageUrl,
            Quantity = firstItem?.Quantity ?? 1,
            TotalPointsUsed = (int)amountUsed,
            AmountUsed = amountUsed,
            Status = HlGiftExchangeStatus.Processing,
            InternalNote = $"UrBox transaction_id={transactionId}"
        };

        try
        {
            await _giftExchangeRepo.InsertAsync(exchange, autoSave: true);
        }
        catch (Exception ex)
        {
            // Unique index (TenantId, RequestId) chặn 2 request song song cùng fingerprint.
            _logger.LogWarning(ex, "UrBox redeem: insert trùng requestId={Req} phone={Phone}", requestId, phone);
            return RedeemFail(409,
                "Yêu cầu đổi quà này đã được ghi nhận. Vui lòng kiểm tra lịch sử đổi quà trước khi thử lại.");
        }

        // ── 4. F2: TRỪ QUỸ TRƯỚC KHI PHÁT HÀNH. Không đủ số dư → KHÔNG gọi UrBox ──
        try
        {
            await _hlPoint.SpendAsync(
                phone,
                (int)HlPointUnit.Amount,
                amountUsed,
                exchangeCode,
                $"Đổi quà UrBox: {exchange.GiftName}");
        }
        catch (UserFriendlyException ex)
        {
            exchange.Status = HlGiftExchangeStatus.Failed;
            exchange.InternalNote += $" | Chặn trước khi gọi UrBox: {ex.Message}";
            await _giftExchangeRepo.UpdateAsync(exchange, autoSave: true);

            _logger.LogWarning("UrBox redeem CHẶN: phone={Phone} cần={Amount} lý do={Msg}",
                phone, amountUsed, ex.Message);
            return RedeemFail(403, ex.Message);
        }

        // 5. Gọi UrBox
        var raw = await SendRawPostAsync(_settings.CartPayVoucherPath, requestData, signature, "CartPayVoucher");
        var result = Deserialize<UrBoxRedeemData>(raw);

        //exchange.UrBoxResponse = Truncate(raw, 4000);
        exchange.UrBoxResponse = raw;

        // Thành công KHI: done == 1 && status == 200 (theo yêu cầu)
        var isSuccess = result != null && result.Done == 1 && result.Status == UrBoxResponseStatus.Success;

        if (!isSuccess)
        {
            exchange.Status = HlGiftExchangeStatus.Failed;
            exchange.InternalNote += $" | Lỗi: {result?.Msg ?? "Không parse được response"} (status={result?.Status})";
            await _giftExchangeRepo.UpdateAsync(exchange, autoSave: true);

            // Đã trừ quỹ ở bước 4 nhưng UrBox không phát hành → HOÀN LẠI, nếu không khách mất tiền.
            try
            {
                await _hlPoint.RefundAsync(
                    phone, (int)HlPointUnit.Amount, amountUsed, exchangeCode,
                    $"Hoàn tiền thưởng — UrBox từ chối đơn {exchangeCode}");
            }
            catch (Exception refundEx)
            {
                // Hoàn thất bại là sai lệch quỹ phải xử lý tay → log Error để alert bắt được.
                _logger.LogError(refundEx,
                    "UrBox redeem: HOÀN QUỸ THẤT BẠI — cần xử lý tay. phone={Phone} amount={Amount} exchange={Code}",
                    phone, amountUsed, exchangeCode);
            }

            _logger.LogWarning("UrBox redeem FAILED: tx={Tx} done={Done} status={Status} msg={Msg}",
                transactionId, result?.Done, result?.Status, result?.Msg);

            // Trả nguyên response UrBox (có msg + status) để Mini App hiển thị.
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
        await _giftExchangeRepo.UpdateAsync(exchange, autoSave: true);

        // Gán Id bản ghi HL.AppHlGiftExchanges vào response để Mini App gọi carts/{id}
        if (result.Data != null) result.Data.Id = exchange.Id;

        // Quỹ đã trừ ở bước 4 (trước khi gọi UrBox) — không trừ lại ở đây.

        // ✅ gửi ZBS “Đổi quà thành công”
        if (!string.IsNullOrWhiteSpace(exchange.CustomerPhone))
        {
            try
            {
                // Lấy hạn dùng TỪ response cartPayVoucher (firstCode) — KHÔNG gọi API getByTransaction (đã ngưng).
                string? expired = firstCode?.Expired;
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

    /// <summary>Response lỗi cho Mini App theo đúng shape UrBox (done=0 + msg + status).</summary>
    private static UrBoxResponse<UrBoxRedeemData> RedeemFail(int status, string message) => new()
    {
        Done = 0,
        Status = status,
        Msg = message
    };

    /// <summary>
    /// Tra giá 1 mệnh giá quà TỪ URBOX (nguồn chân lý về giá) theo priceId.
    /// Trả null khi không tra được — caller phải từ chối đơn, KHÔNG fallback sang giá client gửi.
    /// </summary>
    private async Task<decimal?> ResolveServerPriceAsync(string priceId)
    {
        try
        {
            var resp = await GetGiftDetailAsync(priceId, "vi");
            if (resp == null || resp.Status != UrBoxResponseStatus.Success || resp.Data == null)
            {
                _logger.LogWarning("UrBox price: không tra được priceId={PriceId} status={Status} msg={Msg}",
                    priceId, resp?.Status, resp?.Msg);
                return null;
            }

            if (!decimal.TryParse(resp.Data.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)
                || price <= 0)
            {
                _logger.LogWarning("UrBox price: giá không parse được priceId={PriceId} raw={Raw}",
                    priceId, resp.Data.Price);
                return null;
            }

            // Quà đang khuyến mãi: ghi log để đối soát, vẫn trừ quỹ theo mệnh giá quà khách nhận.
            if (resp.Data.IsPromo == 1 && resp.Data.PricePromo > 0 && resp.Data.PricePromo != price)
                _logger.LogInformation("UrBox price: priceId={PriceId} có promo {Promo} (mệnh giá {Price})",
                    priceId, resp.Data.PricePromo, price);

            return price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UrBox price: lỗi khi tra giá priceId={PriceId}", priceId);
            return null;
        }
    }

    /// <summary>
    /// Fingerprint idempotency khi client không gửi requestId (Mini App hiện tại).
    /// Gom theo (SĐT + giỏ quà + tổng tiền + mốc 2 phút) → double-submit / retry mạng trong
    /// cùng mốc bị chặn, còn lần đổi cố ý sau 2 phút vẫn cho qua (và đã có F2 chặn theo số dư).
    /// Best-effort: chốt chặn thật là unique index + lock trong SpendAsync.
    /// </summary>
    private static string BuildAutoRequestId(string phone, List<UrBoxRedeemItem> items, decimal amountUsed)
    {
        var bucket = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 120;
        var cart = string.Join("|", items
            .Select(i => $"{i.PriceId}x{(i.Quantity > 0 ? i.Quantity : 1)}")
            .OrderBy(x => x, StringComparer.Ordinal));

        var raw = $"{phone}|{cart}|{amountUsed}|{bucket}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return "auto:" + Convert.ToHexString(hash).ToLowerInvariant()[..40];
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
