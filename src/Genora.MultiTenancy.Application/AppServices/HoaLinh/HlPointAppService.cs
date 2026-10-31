using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.HoaLinh;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Service điểm thưởng Hoa Linh: đổi điểm/tiền từ chiến dịch, tiêu điểm (FIFO), lịch sử, số dư.
/// Internal — chỉ controller/service khác gọi (không expose auto-API).
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlPointAppService : ApplicationService, IHlPointAppService
{
    private readonly IRepository<HlPointBatch, Guid> _batchRepo;
    private readonly IRepository<HlPointTransaction, Guid> _txnRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IHlApiClientService _hlApi;
    private readonly IHlPointSpendLock _spendLock;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly HlLoyaltyOptions _loyaltyOptions;
    private readonly ILogger<HlPointAppService> _logger;
    private readonly IBackgroundJobManager _jobManager;
    IStringLocalizer<MultiTenancyResource> l;

    public HlPointAppService(
        IRepository<HlPointBatch, Guid> batchRepo,
        IRepository<HlPointTransaction, Guid> txnRepo,
        IRepository<Customer, Guid> customerRepo,
        IHlApiClientService hlApi,
        IHlPointSpendLock spendLock,
        IUnitOfWorkManager uowManager,
        ICurrentTenant currentTenant,
        IOptionsSnapshot<HlLoyaltyOptions> loyaltyOptions,
        ILogger<HlPointAppService> logger,
        IBackgroundJobManager jobManager,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _batchRepo = batchRepo;
        _txnRepo = txnRepo;
        _customerRepo = customerRepo;
        _hlApi = hlApi;
        _spendLock = spendLock;
        _uowManager = uowManager;
        _currentTenant = currentTenant;
        _loyaltyOptions = loyaltyOptions.Value;
        _logger = logger;
        _jobManager = jobManager;
        this.l = l;
    }

    public async Task<HlPointBatchDto> RedeemFromCampaignAsync(HlRedeemPointInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.CustomerCode))
            throw new UserFriendlyException("Thiếu mã khách hàng");
        if (string.IsNullOrWhiteSpace(input.CampaignCode))
            throw new UserFriendlyException("Thiếu mã chiến dịch");

        // Quỹ thưởng thuộc về SĐT (1 SĐT có nhiều mã KH DMS, mọi mã cộng dồn vào 1 quỹ).
        // Thiếu SĐT thì lô sẽ không tiêu được về sau → chặn ngay, không tạo lô mồ côi.
        var phone = PhoneKey(input.CustomerPhone);
        if (string.IsNullOrWhiteSpace(phone))
            throw new UserFriendlyException("Thiếu số điện thoại khách hàng — không thể ghi nhận tiền thưởng.");

        // 1. Lấy chi tiết chiến dịch từ HL DMS — gọi TRƯỚC khi mở transaction và lấy lock.
        //    Không giữ lock SQL xuyên qua lời gọi HTTP (xem ghi chú trong HlgRegistrationLock).
        var campaignResult = await _hlApi.GetCampaignDetailAsync(input.CustomerCode);
        if (!campaignResult.Success || campaignResult.Data == null || campaignResult.Data.Count == 0)
            throw new UserFriendlyException("Không tìm thấy chiến dịch của khách hàng này.");

        var campaign = campaignResult.Data.FirstOrDefault(x =>
            string.Equals(x.CampaignCode, input.CampaignCode, StringComparison.OrdinalIgnoreCase));
        if (campaign == null)
            throw new UserFriendlyException("Không tìm thấy chiến dịch với mã đã chọn.");

        // 3. Quyết định loại quy đổi theo voucherType của chiến dịch (BR: backend tự quyết, không theo client)
        //    voucherType = 1: đổi bằng TIỀN → cộng BonusAmount (giá trị = voucherValue). ĐANG HỖ TRỢ.
        //    voucherType = 2: quà tặng hàng hóa — chưa hỗ trợ (mở rộng sau).
        //    voucherType = 3: voucher giảm giá (%) — chưa hỗ trợ (mở rộng sau).
        var voucherType = campaign.VoucherType ?? 0;
        if (voucherType != 1)
            throw new UserFriendlyException(
                "Chiến dịch này chưa hỗ trợ quy đổi tự động (chỉ hỗ trợ đổi bằng tiền thưởng). Vui lòng liên hệ để được hỗ trợ.");

        var unit = HlPointUnit.Amount; // voucherType=1 → tiền thưởng (BonusAmount)

        // Giá trị quy đổi = voucherValue (số tiền thưởng của chiến dịch)
        var sourceValue = campaign.VoucherValue ?? 0;
        if (sourceValue <= 0)
            throw new UserFriendlyException("Giá trị voucher của chiến dịch không hợp lệ để quy đổi.");

        // Áp tỉ lệ quy đổi tiền theo cấu hình (mặc định 1 = giữ nguyên). Làm tròn 2 chữ số.
        var rate = _loyaltyOptions.AmountRate;
        if (rate <= 0) rate = 1m;
        var convertedValue = Math.Round(sourceValue * rate, 2, MidpointRounding.AwayFromZero);

        // 4. Mở transaction + KHÓA quỹ của SĐT, rồi mới chặn trùng và ghi dữ liệu.
        //    Thiếu lock thì hai request đổi thưởng song song cùng (mã KH + chiến dịch) đều qua
        //    được check bên dưới → cấp 2 lô, thổi hạn mức lên gấp đôi.
        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);
        await _spendLock.AcquireAsync(phone, ct);

        // Chặn đổi trùng: mỗi (khách + chiến dịch) chỉ đổi 1 lần
        var already = await _batchRepo.FirstOrDefaultAsync(
            x => x.CustomerCode == input.CustomerCode && x.CampaignCode == input.CampaignCode, ct);
        if (already != null)
            throw new UserFriendlyException("Chiến dịch này đã được đổi điểm trước đó.");

        // 5. Tạo lô (hạn +1 năm) — lưu đầy đủ thông tin chiến dịch + voucher để đối soát
        var now = DateTime.Now;
        var batch = new HlPointBatch(GuidGenerator.Create(), await GenerateBatchCodeAsync(now), _currentTenant.Id)
        {
            CustomerCode = input.CustomerCode,
            CustomerName = input.CustomerName ?? campaign.CampaignName,
            CustomerPhone = phone,
            CampaignCode = campaign.CampaignCode,
            CampaignName = campaign.CampaignName,
            CampaignPeriod = campaign.CampaignPeriod,
            DisplayType = campaign.DisplayType,
            MembershipTier = campaign.MembershipTier,
            AccumulatedSales = campaign.AccumulatedSales,
            AccumulatedPoints = campaign.AccumulatedPoints,
            VoucherCode = campaign.VoucherCode,
            VoucherName = campaign.VoucherName,
            VoucherType = campaign.VoucherType,
            VoucherValue = campaign.VoucherValue,
            Unit = unit,
            SourceValue = sourceValue,
            ConvertedValue = convertedValue,
            RemainingValue = convertedValue,
            Status = HlPointBatchStatus.Active,
            ExchangedAt = now,
            ExpireDate = now.AddYears(1)
        };

        // 5. Cộng quỹ. NGUỒN CHÂN LÝ = dbo.AppCustomers.BonusAmount (khóa theo SĐT).
        //    Lô chỉ là bản ghi lịch sử cấp thưởng để đối soát, KHÔNG dùng để tính số dư:
        //    luồng UrBox cũ chưa bao giờ trừ lô nên RemainingValue của lô cũ không phản ánh
        //    phần khách đã tiêu. Tính số dư từ lô sẽ cấp lại tiền đã tiêu.
        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct);
        decimal balancePoint = 0, balanceAmount = 0;
        if (customer != null)
        {
            batch.CustomerId = customer.Id;
            if (string.IsNullOrWhiteSpace(batch.CustomerName)) batch.CustomerName = customer.FullName;

            if (unit == HlPointUnit.Point) customer.BonusPoint += convertedValue;
            else customer.BonusAmount += convertedValue;

            await _customerRepo.UpdateAsync(customer, autoSave: false, cancellationToken: ct);
            balancePoint = customer.BonusPoint;
            balanceAmount = customer.BonusAmount;
        }

        await _batchRepo.InsertAsync(batch, autoSave: false, cancellationToken: ct);

        // 6. Ghi sổ cái
        var txn = new HlPointTransaction(GuidGenerator.Create(), _currentTenant.Id)
        {
            CustomerId = customer?.Id,
            CustomerCode = input.CustomerCode,
            CustomerName = batch.CustomerName,
            CustomerPhone = batch.CustomerPhone,
            Type = HlPointTransactionType.Earn,
            Unit = unit,
            Value = convertedValue,
            BalancePointAfter = balancePoint,
            BalanceAmountAfter = balanceAmount,
            BatchId = batch.Id,
            RefCode = batch.BatchCode,
            Description = $"Đổi từ chiến dịch {campaign.CampaignName} ({campaign.CampaignCode})"
        };
        await _txnRepo.InsertAsync(txn, autoSave: false, cancellationToken: ct);

        await uow.CompleteAsync(ct);

        // ✅ gửi ZBS “Đổi thưởng thành công”
        if (!string.IsNullOrWhiteSpace(batch.CustomerPhone))
        {
            try
            {
                await _jobManager.EnqueueAsync(
                    new ZbsSendJobArgs
                    {
                        TenantId = _currentTenant.Id,
                        TemplateKey = "RedeemPoint",
                        Phone = PhoneHelper.NormalizePhoneTo84(l, batch.CustomerPhone),
                        TrackingId = batch.BatchCode,
                        TemplateData = new
                        {
                            batch_code = batch.BatchCode,
                            customer_name = batch.CustomerName,
                            customer_code = batch.CustomerCode,
                            membership_tier = batch.MembershipTier,
                            campaign_name = batch.CampaignName,
                            voucher_value = batch.VoucherValue,
                            exchanged_at = batch.ExchangedAt.ToString("dd/MM/yyyy"),
                            expire_date = batch.ExpireDate.ToString("dd/MM/yyyy"),
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

        _logger.LogInformation("HL redeem: {Cust} campaign={Camp} unit={Unit} value={Val}",
            input.CustomerCode, campaign.CampaignCode, unit, convertedValue);

        return MapBatch(batch);
    }

    public async Task SpendAsync(string customerPhone, int unit, decimal value, string? refCode = null, string? description = null, CancellationToken ct = default)
    {
        var phone = PhoneKey(customerPhone);
        if (string.IsNullOrWhiteSpace(phone)) throw new UserFriendlyException("Thiếu số điện thoại khách hàng");
        if (value <= 0) throw new UserFriendlyException("Giá trị tiêu phải lớn hơn 0");

        var pointUnit = unit == (int)HlPointUnit.Amount ? HlPointUnit.Amount : HlPointUnit.Point;
        var now = DateTime.Now;

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        // Khóa quỹ của SĐT này trong suốt transaction → hai request song song không thể
        // cùng đọc một số dư rồi cùng trừ. Mọi call API ngoài phải xong TRƯỚC khi tới đây.
        await _spendLock.AcquireAsync(phone, ct);

        // ══ CHỐT CHẶN HẠN MỨC ══════════════════════════════════════════════════
        // NGUỒN CHÂN LÝ = dbo.AppCustomers.BonusAmount (khóa theo SĐT), KHÔNG phải lô.
        // Lý do: luồng UrBox cũ chỉ trừ BonusAmount, chưa bao giờ trừ RemainingValue của lô
        // (RC4) → lô cũ vẫn giữ nguyên giá trị gốc. Dùng lô để tính số dư sẽ cấp lại toàn bộ
        // phần khách đã tiêu. BonusAmount là số đã được đối soát với HL DMS và UrBox.
        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct)
            ?? throw new UserFriendlyException("Không tìm thấy khách hàng theo số điện thoại này.");

        var available = pointUnit == HlPointUnit.Point ? customer.BonusPoint : customer.BonusAmount;
        if (available < value)
            throw new UserFriendlyException(
                $"Số dư tiền thưởng không đủ. Khả dụng: {available:#,##0}đ, cần: {value:#,##0}đ.");

        // Trừ quỹ. KHÔNG clamp bằng Math.Max(0, …): đã guard ở trên nên không thể âm,
        // và nếu có bug khác làm âm thì phải lộ ra chứ không được che đi.
        if (pointUnit == HlPointUnit.Point) customer.BonusPoint = available - value;
        else customer.BonusAmount = available - value;
        await _customerRepo.UpdateAsync(customer, autoSave: false, cancellationToken: ct);

        var balancePoint = customer.BonusPoint;
        var balanceAmount = customer.BonusAmount;

        // Trừ lô FIFO — chỉ để ghi sổ sách, KHÔNG dùng để chặn hạn mức.
        // Lô cũ chưa được đối soát nên có thể không đủ để trừ; thiếu thì bỏ qua,
        // không chặn giao dịch (hạn mức đã được chặn bằng BonusAmount ở trên).
        var queryable = await _batchRepo.GetQueryableAsync();
        var batches = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerPhone == phone
                                 && x.Unit == pointUnit
                                 && x.Status == HlPointBatchStatus.Active
                                 && x.RemainingValue > 0
                                 && x.ExpireDate > now)
                     .OrderBy(x => x.ExpireDate), ct);

        var remainingToSpend = value;
        foreach (var b in batches)
        {
            if (remainingToSpend <= 0) break;
            var take = Math.Min(b.RemainingValue, remainingToSpend);
            b.RemainingValue -= take;
            remainingToSpend -= take;
            if (b.RemainingValue <= 0) b.Status = HlPointBatchStatus.Exhausted;
            await _batchRepo.UpdateAsync(b, autoSave: false, cancellationToken: ct);
        }

        var txn = new HlPointTransaction(GuidGenerator.Create(), _currentTenant.Id)
        {
            CustomerId = customer?.Id,
            CustomerCode = customer?.CustomerCode,
            CustomerName = customer?.FullName,
            CustomerPhone = phone,
            Type = HlPointTransactionType.Spend,
            Unit = pointUnit,
            Value = -value,
            BalancePointAfter = balancePoint,
            BalanceAmountAfter = balanceAmount,
            RefCode = refCode,
            Description = description ?? "Tiêu điểm đổi quà"
        };
        await _txnRepo.InsertAsync(txn, autoSave: false, cancellationToken: ct);

        await uow.CompleteAsync(ct);
    }

    public async Task RefundAsync(string customerPhone, int unit, decimal value, string? refCode = null, string? description = null, CancellationToken ct = default)
    {
        var phone = PhoneKey(customerPhone);
        if (string.IsNullOrWhiteSpace(phone)) throw new UserFriendlyException("Thiếu số điện thoại khách hàng");
        if (value <= 0) return;

        var pointUnit = unit == (int)HlPointUnit.Amount ? HlPointUnit.Amount : HlPointUnit.Point;
        var now = DateTime.Now;

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        await _spendLock.AcquireAsync(phone, ct);

        // Hoàn bằng lô mới, giữ hạn của lô gốc sớm nhất còn hiệu lực để không kéo dài
        // thời hạn ưu đãi; không còn lô nào thì fallback +1 năm.
        var queryable = await _batchRepo.GetQueryableAsync();
        var reference = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(x => x.CustomerPhone == phone && x.Unit == pointUnit)
                     .OrderByDescending(x => x.ExpireDate), ct);

        var expireDate = reference?.ExpireDate > now ? reference.ExpireDate : now.AddYears(1);
        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct);

        var batch = new HlPointBatch(GuidGenerator.Create(), await GenerateBatchCodeAsync(now), _currentTenant.Id)
        {
            CustomerId = customer?.Id,
            CustomerCode = reference?.CustomerCode ?? customer?.CustomerCode,
            CustomerName = customer?.FullName ?? reference?.CustomerName,
            CustomerPhone = phone,
            CampaignCode = reference?.CampaignCode,
            CampaignName = reference?.CampaignName,
            Unit = pointUnit,
            SourceValue = value,
            ConvertedValue = value,
            RemainingValue = value,
            Status = HlPointBatchStatus.Active,
            ExchangedAt = now,
            ExpireDate = expireDate
        };
        await _batchRepo.InsertAsync(batch, autoSave: false, cancellationToken: ct);

        // Hoàn vào NGUỒN CHÂN LÝ = BonusAmount trên AppCustomers (cộng số học, không tính từ lô)
        decimal balancePoint = 0, balanceAmount = 0;
        if (customer != null)
        {
            if (pointUnit == HlPointUnit.Point) customer.BonusPoint += value;
            else customer.BonusAmount += value;

            await _customerRepo.UpdateAsync(customer, autoSave: false, cancellationToken: ct);
            balancePoint = customer.BonusPoint;
            balanceAmount = customer.BonusAmount;
        }

        var txn = new HlPointTransaction(GuidGenerator.Create(), _currentTenant.Id)
        {
            CustomerId = customer?.Id,
            CustomerCode = batch.CustomerCode,
            CustomerName = batch.CustomerName,
            CustomerPhone = phone,
            Type = HlPointTransactionType.Adjust,
            Unit = pointUnit,
            Value = value,
            BalancePointAfter = balancePoint,
            BalanceAmountAfter = balanceAmount,
            BatchId = batch.Id,
            RefCode = refCode,
            Description = description ?? "Hoàn tiền thưởng do phát hành quà thất bại"
        };
        await _txnRepo.InsertAsync(txn, autoSave: false, cancellationToken: ct);

        await uow.CompleteAsync(ct);

        _logger.LogWarning("HL refund: phone={Phone} unit={Unit} value={Val} ref={Ref}",
            phone, pointUnit, value, refCode);
    }

    public async Task<HlPointBalanceDto> GetBalanceAsync(string customerPhone, CancellationToken ct = default)
    {
        var phone = PhoneKey(customerPhone);
        var now = DateTime.Now;
        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phone, ct);

        // Chỉ tính lô còn hiệu lực (Active + chưa hết hạn) — nguồn chân lý cho số dư khả dụng.
        var queryable = await _batchRepo.GetQueryableAsync();
        var batches = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerPhone == phone
                                 && x.Status == HlPointBatchStatus.Active
                                 && x.RemainingValue > 0
                                 && x.ExpireDate > now)
                     .OrderBy(x => x.ExpireDate), ct);

        return new HlPointBalanceDto
        {
            CustomerCode = customer?.CustomerCode,
            CustomerName = customer?.FullName,
            // Số dư khả dụng = BonusPoint/BonusAmount trên AppCustomers (nguồn chân lý).
            // KHÔNG tính từ tổng lô: lô cũ chưa từng bị trừ nên sẽ trả về cả phần đã tiêu.
            BonusPoint = customer?.BonusPoint ?? 0,
            BonusAmount = customer?.BonusAmount ?? 0,
            // Danh sách lô chỉ để tham khảo/đối soát, không phải cơ sở tính số dư.
            ActiveBatches = batches.Select(MapBatch).ToList()
        };
    }

    public async Task<List<HlPointTransactionDto>> GetCustomerHistoryAsync(string customerPhone, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        var phone = PhoneKey(customerPhone);
        var queryable = await _txnRepo.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerPhone == phone)
                     .OrderByDescending(x => x.CreationTime)
                     .Skip(skip).Take(take), ct);
        return items.Select(MapTxn).ToList();
    }

    public async Task<string?> ResolvePhoneAsync(string customerCodeOrPhone, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(customerCodeOrPhone)) return null;
        var raw = customerCodeOrPhone.Trim();

        // 1. Đã là SĐT (toàn số, 9–11 chữ số sau khi chuẩn hóa)
        if (raw.All(char.IsDigit) || raw.StartsWith("+"))
        {
            var normalized = PhoneKey(raw);
            if (normalized.Length is >= 10 and <= 11) return normalized;
        }

        // 2. Mã KH → dbo.AppCustomers
        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == raw, ct);
        if (!string.IsNullOrWhiteSpace(customer?.PhoneNumber)) return PhoneKey(customer!.PhoneNumber);

        // 3. Mã KH → lô điểm. Cần bước này vì 1 SĐT có nhiều mã KH DMS mà AppCustomers
        //    chỉ giữ được 1 mã; mã còn lại chỉ tra được qua lô.
        var queryable = await _batchRepo.GetQueryableAsync();
        var batch = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(x => x.CustomerCode == raw && x.CustomerPhone != null)
                     .OrderByDescending(x => x.CreationTime), ct);

        return string.IsNullOrWhiteSpace(batch?.CustomerPhone) ? null : PhoneKey(batch!.CustomerPhone);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Chuẩn hóa SĐT về dạng 0xxxxxxxxx — đây là KHÓA ĐỊNH DANH của quỹ thưởng.
    /// Phải khớp với dạng đang lưu trong dbo.AppCustomers.PhoneNumber.
    /// </summary>
    private static string PhoneKey(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return string.Empty;
        if (digits.StartsWith("84") && digits.Length >= 11) return "0" + digits[2..];
        if (!digits.StartsWith("0") && digits.Length == 9) return "0" + digits;
        return digits;
    }

    /// <summary>
    /// Đồng bộ bản sao BonusPoint/BonusAmount trên dbo.AppCustomers = tổng lô còn hiệu lực của SĐT.
    /// Luôn TÍNH LẠI từ lô thay vì cộng/trừ số học → bản sao không thể lệch khỏi nguồn chân lý.
    /// extraPoint/extraAmount: giá trị của lô vừa Insert trong cùng UoW (chưa flush nên query chưa thấy).
    /// </summary>
    // Đã bỏ SyncMirrorAsync: hàm này tính BonusPoint/BonusAmount = tổng RemainingValue của lô.
    // Lô cũ chưa bao giờ bị trừ (luồng UrBox cũ bỏ qua lô) nên gọi nó sẽ cấp lại toàn bộ phần
    // khách đã tiêu. BonusAmount trên dbo.AppCustomers là nguồn chân lý, chỉ cộng/trừ số học.

    private async Task<string> GenerateBatchCodeAsync(DateTime now)
    {
        var prefix = "PB" + now.ToString("yyMMdd", CultureInfo.InvariantCulture);
        var queryable = await _batchRepo.GetQueryableAsync();
        var countToday = await AsyncExecuter.CountAsync(queryable.Where(x => x.BatchCode.StartsWith(prefix)));
        var next = countToday + 1;
        var candidate = $"{prefix}{next:D4}";
        while (await _batchRepo.AnyAsync(x => x.BatchCode == candidate))
        {
            next++;
            candidate = $"{prefix}{next:D4}";
        }
        return candidate;
    }

    private static HlPointBatchDto MapBatch(HlPointBatch b) => new()
    {
        Id = b.Id,
        BatchCode = b.BatchCode,
        CustomerCode = b.CustomerCode,
        CustomerName = b.CustomerName,
        CustomerPhone = b.CustomerPhone,
        CampaignCode = b.CampaignCode,
        CampaignName = b.CampaignName,
        CampaignPeriod = b.CampaignPeriod,
        DisplayType = b.DisplayType,
        MembershipTier = b.MembershipTier,
        AccumulatedSales = b.AccumulatedSales,
        AccumulatedPoints = b.AccumulatedPoints,
        VoucherCode = b.VoucherCode,
        VoucherName = b.VoucherName,
        VoucherType = b.VoucherType,
        VoucherValue = b.VoucherValue,
        Unit = (int)b.Unit,
        UnitText = GetUnitText(b.Unit),
        SourceValue = b.SourceValue,
        ConvertedValue = b.ConvertedValue,
        RemainingValue = b.RemainingValue,
        Status = (int)b.Status,
        StatusText = GetBatchStatusText(b.Status),
        ExchangedAt = b.ExchangedAt,
        ExpireDate = b.ExpireDate
    };

    private static HlPointTransactionDto MapTxn(HlPointTransaction t) => new()
    {
        Id = t.Id,
        CustomerCode = t.CustomerCode,
        CustomerName = t.CustomerName,
        CustomerPhone = t.CustomerPhone,
        Type = (int)t.Type,
        TypeText = GetTypeText(t.Type),
        Unit = (int)t.Unit,
        UnitText = GetUnitText(t.Unit),
        Value = t.Value,
        BalancePointAfter = t.BalancePointAfter,
        BalanceAmountAfter = t.BalanceAmountAfter,
        BatchId = t.BatchId,
        RefCode = t.RefCode,
        Description = t.Description,
        CreationTime = t.CreationTime
    };

    private static string GetUnitText(HlPointUnit u) => u == HlPointUnit.Amount ? "Tiền" : "Điểm";

    private static string GetBatchStatusText(HlPointBatchStatus s) => s switch
    {
        HlPointBatchStatus.Active => "Còn hiệu lực",
        HlPointBatchStatus.Exhausted => "Đã dùng hết",
        HlPointBatchStatus.Expired => "Hết hạn",
        _ => "Không xác định"
    };

    private static string GetTypeText(HlPointTransactionType t) => t switch
    {
        HlPointTransactionType.Earn => "Đổi điểm",
        HlPointTransactionType.Spend => "Tiêu điểm",
        HlPointTransactionType.Expire => "Hết hạn",
        HlPointTransactionType.Adjust => "Điều chỉnh",
        _ => "Không xác định"
    };
}
