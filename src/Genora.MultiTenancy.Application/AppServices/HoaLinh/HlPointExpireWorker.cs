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
                foreach (var batch in expiredBatches)
                {
                    var remaining = batch.RemainingValue;

                    if (remaining > 0 && !string.IsNullOrWhiteSpace(batch.CustomerCode))
                    {
                        var customer = await customerRepo.FirstOrDefaultAsync(x => x.CustomerCode == batch.CustomerCode);
                        if (customer != null)
                        {
                            if (batch.Unit == HlPointUnit.Point)
                                customer.BonusPoint = Math.Max(0, customer.BonusPoint - remaining);
                            else
                                customer.BonusAmount = Math.Max(0, customer.BonusAmount - remaining);

                            await customerRepo.UpdateAsync(customer, autoSave: false);

                            await txnRepo.InsertAsync(new HlPointTransaction(_guidGenerator.Create(), batch.TenantId)
                            {
                                CustomerId = customer.Id,
                                CustomerCode = batch.CustomerCode,
                                CustomerName = batch.CustomerName,
                                CustomerPhone = batch.CustomerPhone,
                                Type = HlPointTransactionType.Expire,
                                Unit = batch.Unit,
                                Value = -remaining,
                                BalancePointAfter = customer.BonusPoint,
                                BalanceAmountAfter = customer.BonusAmount,
                                BatchId = batch.Id,
                                RefCode = batch.BatchCode,
                                Description = $"Điểm/tiền hết hạn từ lô {batch.BatchCode}"
                            }, autoSave: false);

                            totalExpired += remaining;
                        }
                    }

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
