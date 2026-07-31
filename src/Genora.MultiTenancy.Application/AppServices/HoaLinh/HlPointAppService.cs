using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Excel;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.HoaLinh;
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
    private readonly IUnitOfWorkManager _uowManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly HlLoyaltyOptions _loyaltyOptions;
    private readonly ILogger<HlPointAppService> _logger;
    private readonly IBackgroundJobManager _jobManager;

    public HlPointAppService(
        IRepository<HlPointBatch, Guid> batchRepo,
        IRepository<HlPointTransaction, Guid> txnRepo,
        IRepository<Customer, Guid> customerRepo,
        IHlApiClientService hlApi,
        IUnitOfWorkManager uowManager,
        ICurrentTenant currentTenant,
        IOptionsSnapshot<HlLoyaltyOptions> loyaltyOptions,
        ILogger<HlPointAppService> logger,
        IBackgroundJobManager jobManager)
    {
        _batchRepo = batchRepo;
        _txnRepo = txnRepo;
        _customerRepo = customerRepo;
        _hlApi = hlApi;
        _uowManager = uowManager;
        _currentTenant = currentTenant;
        _loyaltyOptions = loyaltyOptions.Value;
        _logger = logger;
        _jobManager = jobManager;
    }

    public async Task<HlPointBatchDto> RedeemFromCampaignAsync(HlRedeemPointInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.CustomerCode))
            throw new UserFriendlyException("Thiếu mã khách hàng");
        if (string.IsNullOrWhiteSpace(input.CampaignCode))
            throw new UserFriendlyException("Thiếu mã chiến dịch");

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        // 1. Chặn đổi trùng: mỗi (khách + chiến dịch) chỉ đổi 1 lần
        var already = await _batchRepo.FirstOrDefaultAsync(
            x => x.CustomerCode == input.CustomerCode && x.CampaignCode == input.CampaignCode, ct);
        if (already != null)
            throw new UserFriendlyException("Chiến dịch này đã được đổi điểm trước đó.");

        // 2. Lấy chi tiết chiến dịch từ HL DMS
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

        // 4. Tạo lô (hạn +1 năm) — lưu đầy đủ thông tin chiến dịch + voucher để đối soát
        var now = DateTime.Now;
        var batch = new HlPointBatch(GuidGenerator.Create(), await GenerateBatchCodeAsync(now), _currentTenant.Id)
        {
            CustomerCode = input.CustomerCode,
            CustomerName = input.CustomerName ?? campaign.CampaignName,
            CustomerPhone = input.CustomerPhone,
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

        // 5. Cộng quỹ AppCustomers + gán CustomerId nếu tìm được
        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == input.CustomerCode, ct);
        decimal balancePoint = 0, balanceAmount = 0;
        if (customer != null)
        {
            batch.CustomerId = customer.Id;
            if (string.IsNullOrWhiteSpace(batch.CustomerName)) batch.CustomerName = customer.FullName;
            if (string.IsNullOrWhiteSpace(batch.CustomerPhone)) batch.CustomerPhone = customer.PhoneNumber;

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
                        Phone = PhoneHelper.NormalizePhoneTo84(batch.CustomerPhone),
                        TrackingId = batch.BatchCode,
                        TemplateData = new
                        {
                            batch_code = batch.BatchCode,
                            customer_name = batch.CustomerName,
                            customer_code = batch.CustomerCode,
                            membership_tier = batch.MembershipTier,
                            campaign_name = batch.CampaignName,
                            voucher_value = batch.VoucherValue,
                            exchanged_at = batch.ExchangedAt,
                            expire_date = batch.ExpireDate,
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

    public async Task SpendAsync(string customerCode, int unit, decimal value, string? refCode = null, string? description = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(customerCode)) throw new UserFriendlyException("Thiếu mã khách hàng");
        if (value <= 0) throw new UserFriendlyException("Giá trị tiêu phải lớn hơn 0");

        var pointUnit = unit == (int)HlPointUnit.Amount ? HlPointUnit.Amount : HlPointUnit.Point;
        var now = DateTime.Now;

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == customerCode, ct)
            ?? throw new UserFriendlyException("Không tìm thấy khách hàng");

        // FIFO: chỉ lấy lô CÒN HIỆU LỰC (Active + chưa hết hạn). Filter ExpireDate > now để
        // race-proof khi worker hết hạn chưa kịp chạy.
        var queryable = await _batchRepo.GetQueryableAsync();
        var batches = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerCode == customerCode
                                 && x.Unit == pointUnit
                                 && x.Status == HlPointBatchStatus.Active
                                 && x.RemainingValue > 0
                                 && x.ExpireDate > now)
                     .OrderBy(x => x.ExpireDate), ct);

        // Nguồn chân lý = tổng RemainingValue của lô còn hiệu lực, KHÔNG dùng BonusPoint thô
        // (có thể còn điểm "mồ côi" không thuộc lô nào hoặc lô đã hết hạn).
        var available = batches.Sum(x => x.RemainingValue);
        if (available < value)
            throw new UserFriendlyException("Số dư điểm thưởng không đủ hoặc đã hết hạn.");

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

        // Trừ quỹ (clamp >= 0)
        if (pointUnit == HlPointUnit.Point) customer.BonusPoint = Math.Max(0, customer.BonusPoint - value);
        else customer.BonusAmount = Math.Max(0, customer.BonusAmount - value);
        await _customerRepo.UpdateAsync(customer, autoSave: false, cancellationToken: ct);

        var txn = new HlPointTransaction(GuidGenerator.Create(), _currentTenant.Id)
        {
            CustomerId = customer.Id,
            CustomerCode = customerCode,
            CustomerName = customer.FullName,
            CustomerPhone = customer.PhoneNumber,
            Type = HlPointTransactionType.Spend,
            Unit = pointUnit,
            Value = -value,
            BalancePointAfter = customer.BonusPoint,
            BalanceAmountAfter = customer.BonusAmount,
            RefCode = refCode,
            Description = description ?? "Tiêu điểm đổi quà"
        };
        await _txnRepo.InsertAsync(txn, autoSave: false, cancellationToken: ct);

        await uow.CompleteAsync(ct);
    }

    public async Task<HlPointBalanceDto> GetBalanceAsync(string customerCode, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == customerCode, ct);

        // Chỉ tính lô còn hiệu lực (Active + chưa hết hạn) — nguồn chân lý cho điểm khả dụng.
        var queryable = await _batchRepo.GetQueryableAsync();
        var batches = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerCode == customerCode
                                 && x.Status == HlPointBatchStatus.Active
                                 && x.RemainingValue > 0
                                 && x.ExpireDate > now)
                     .OrderBy(x => x.ExpireDate), ct);

        var availablePoint = batches.Where(x => x.Unit == HlPointUnit.Point).Sum(x => x.RemainingValue);
        var availableAmount = batches.Where(x => x.Unit == HlPointUnit.Amount).Sum(x => x.RemainingValue);

        return new HlPointBalanceDto
        {
            CustomerCode = customerCode,
            CustomerName = customer?.FullName,
            // Số dư khả dụng thực tế = tổng lô còn hiệu lực (không dùng BonusPoint thô có thể lệch/mồ côi)
            BonusPoint = availablePoint,
            BonusAmount = availableAmount,
            ActiveBatches = batches.Select(MapBatch).ToList()
        };
    }

    public async Task<List<HlPointTransactionDto>> GetCustomerHistoryAsync(string customerCode, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        var queryable = await _txnRepo.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerCode == customerCode)
                     .OrderByDescending(x => x.CreationTime)
                     .Skip(skip).Take(take), ct);
        return items.Select(MapTxn).ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

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
