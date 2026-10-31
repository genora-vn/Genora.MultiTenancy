using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.HoaLinh;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.BackgroundWorkers;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Job quét lô điểm/tiền thưởng Hoa Linh hết hạn → trừ phần còn lại khỏi quỹ AppCustomers
/// (BonusPoint/BonusAmount) + ghi giao dịch Expire. Chạy mỗi giờ (cấu hình qua HlPointExpireOptions).
/// </summary>
public class HlPointExpireWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly HlPointExpireOptions _options;
    private readonly IClock _clock;
    private readonly IDataFilter _dataFilter;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<HlPointExpireWorker> _logger;

    public HlPointExpireWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory factory,
        IOptions<HlPointExpireOptions> opts,
        IClock clock,
        IDataFilter dataFilter,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        ILogger<HlPointExpireWorker> logger)
        : base(timer, factory)
    {
        _options = opts.Value;
        _clock = clock;
        _dataFilter = dataFilter;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _logger = logger;

        Timer.Period = (int)_options.Period.TotalMilliseconds;
        Timer.RunOnStart = false;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext context)
    {
        if (!_options.Enabled) return;

        var now = _clock.Now;

        var batchRepo = context.ServiceProvider.GetRequiredService<IRepository<HlPointBatch, Guid>>();
        var txnRepo = context.ServiceProvider.GetRequiredService<IRepository<HlPointTransaction, Guid>>();
        var customerRepo = context.ServiceProvider.GetRequiredService<IRepository<Customer, Guid>>();
        var uowManager = context.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var asyncExecuter = context.ServiceProvider.GetRequiredService<IAsyncQueryableExecuter>();

        // Quét toàn bộ tenant (disable multi-tenant filter)
        using (_dataFilter.Disable<IMultiTenant>())
        using (var uow = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            try
            {
                var queryable = await batchRepo.GetQueryableAsync();
                var expiredBatches = await asyncExecuter.ToListAsync(
                    queryable.Where(x => x.Status == HlPointBatchStatus.Active && x.ExpireDate <= now));

                if (expiredBatches.Count == 0)
                {
                    await uow.CompleteAsync();
                    return;
                }

                var totalExpired = 0m;

                // Gom theo SĐT — quỹ thưởng khóa theo SĐT, không theo mã KH.
                // Code cũ tra customer theo CustomerCode nên hụt đúng các ca 1 SĐT nhiều mã KH
                // (mã trong lô khác mã đang lưu ở AppCustomers) → không trừ được gì.
                var trustFrom = _options.TrustBatchesFrom;

                foreach (var group in expiredBatches
                             .Where(x => !string.IsNullOrWhiteSpace(x.CustomerPhone))
                             .GroupBy(x => x.CustomerPhone!))
                {
                    var phone = group.Key;
                    var customer = await customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phone);

                    foreach (var batch in group)
                    {
                        var amount = batch.RemainingValue;

                        // RemainingValue CHỈ đáng tin với lô tạo từ mốc TrustBatchesFrom trở đi.
                        // Lô cũ hơn chưa bao giờ bị trừ (luồng UrBox cũ bỏ qua lô) nên vẫn giữ
                        // nguyên giá trị gốc dù khách đã tiêu — trừ con số đó khỏi BonusAmount
                        // sẽ xóa oan số dư hợp lệ. Với lô cũ: chỉ đóng lô, KHÔNG động vào quỹ.
                        var trusted = trustFrom.HasValue && batch.CreationTime >= trustFrom.Value;

                        batch.RemainingValue = 0;
                        batch.Status = HlPointBatchStatus.Expired;
                        await batchRepo.UpdateAsync(batch, autoSave: false);

                        if (!trusted)
                        {
                            if (amount > 0)
                                _logger.LogWarning(
                                    "[HlPointExpire] Lô {Code} (tạo {Created:dd/MM/yyyy}) chưa đối soát — đóng lô, KHÔNG trừ quỹ {Amount}",
                                    batch.BatchCode, batch.CreationTime, amount);
                            continue;
                        }

                        if (amount <= 0 || customer == null) continue;

                        // Trừ SỐ HỌC khỏi nguồn chân lý (BonusPoint/BonusAmount), không tính từ lô
                        if (batch.Unit == HlPointUnit.Point)
                            customer.BonusPoint = Math.Max(0, customer.BonusPoint - amount);
                        else
                            customer.BonusAmount = Math.Max(0, customer.BonusAmount - amount);
                        await customerRepo.UpdateAsync(customer, autoSave: false);

                        totalExpired += amount;

                        await txnRepo.InsertAsync(new HlPointTransaction(_guidGenerator.Create(), batch.TenantId)
                        {
                            CustomerId = customer.Id,
                            CustomerCode = batch.CustomerCode,
                            CustomerName = batch.CustomerName,
                            CustomerPhone = phone,
                            Type = HlPointTransactionType.Expire,
                            Unit = batch.Unit,
                            Value = -amount,
                            BalancePointAfter = customer.BonusPoint,
                            BalanceAmountAfter = customer.BonusAmount,
                            BatchId = batch.Id,
                            RefCode = batch.BatchCode,
                            Description = $"Điểm/tiền hết hạn từ lô {batch.BatchCode}"
                        }, autoSave: false);
                    }
                }

                // Lô không có SĐT vẫn phải đóng để không bị quét lại mỗi giờ.
                foreach (var batch in expiredBatches.Where(x => string.IsNullOrWhiteSpace(x.CustomerPhone)))
                {
                    _logger.LogWarning("[HlPointExpire] Lô {Code} không có SĐT — đóng lô, không trừ quỹ", batch.BatchCode);
                    batch.RemainingValue = 0;
                    batch.Status = HlPointBatchStatus.Expired;
                    await batchRepo.UpdateAsync(batch, autoSave: false);
                }

                await uow.CompleteAsync();

                _logger.LogInformation("[HlPointExpire] Đã xử lý {Count} lô hết hạn, trừ tổng {Total}",
                    expiredBatches.Count, totalExpired);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HlPointExpire] Lỗi khi quét điểm hết hạn");
            }
        }
    }
}
