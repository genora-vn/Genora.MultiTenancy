using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;

namespace Genora.MultiTenancy.AuditLogs;

public class AuditLogCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IAuditLogRepository _repo;
    private readonly IClock _clock;
    private readonly ILogger<AuditLogCleanupWorker> _logger;
    private readonly IDataFilter _dataFilter;
    private readonly ICurrentTenant _currentTenant;
    private readonly AuditLogCleanupOptions _options;

    public AuditLogCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory factory,
        IAuditLogRepository repo,
        IClock clock,
        IDataFilter dataFilter,
        ICurrentTenant currentTenant,
        IOptions<AuditLogCleanupOptions> opts,
        ILogger<AuditLogCleanupWorker> logger
    ) : base(timer, factory)
    {
        _repo = repo;
        _clock = clock;
        _dataFilter = dataFilter;
        _currentTenant = currentTenant;
        _options = opts.Value;
        _logger = logger;

        // Kỳ chạy dọn rác theo cấu hình
        Timer.Period = (int)_options.Period.TotalMilliseconds;
        // Không chồng lặp nếu tick trước chưa xong
        Timer.RunOnStart = false;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext context)
    {
        if (!_options.Enabled) return;

        var olderThan = _clock.Now - _options.Retention;

        using (_dataFilter.Disable<IMultiTenant>())
        using (_currentTenant.Change(null))
        {
            try
            {
                if (_options.BatchSize <= 0)
                {
                    // Xóa toàn bộ bản ghi cũ hơn thời hạn
                    await _repo.DeleteAsync(x => x.ExecutionTime < olderThan);

                    _logger.LogInformation(
                        "[AuditCleanup] Deleted logs older than {OlderThan:u}",
                        olderThan
                    );
                }
                else
                {
                    // Xóa theo batch
                    var totalDeleted = 0;

                    while (true)
                    {
                        // Lấy batch cũ nhất
                        var oldOnes = await _repo.GetListAsync(
                            sorting: nameof(AuditLog.ExecutionTime),
                            maxResultCount: _options.BatchSize,
                            skipCount: 0,
                            startTime: null,
                            endTime: olderThan
                        );

                        if (oldOnes.Count == 0)
                            break;

                        var ids = oldOnes.Select(x => x.Id).ToList();
                        await _repo.DeleteAsync(x => ids.Contains(x.Id));

                        totalDeleted += ids.Count;
                    }

                    _logger.LogInformation(
                        "[AuditCleanup] Batch deleted {Count} logs older than {OlderThan:u}",
                        totalDeleted, olderThan
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuditCleanup] Error while deleting logs older than {OlderThan:u}", olderThan);
            }
        }
    }
}