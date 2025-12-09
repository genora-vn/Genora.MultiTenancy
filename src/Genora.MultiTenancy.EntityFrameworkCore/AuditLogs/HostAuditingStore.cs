using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AuditLogs;

/// <summary>
/// Redirect toàn bộ AuditLogs của tenants về DB Host.
/// </summary>
public class HostRedirectAuditingStore : IAuditingStore, ITransientDependency
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly ICurrentTenant _currentTenant;

    public HostRedirectAuditingStore(
        IServiceScopeFactory scopeFactory,
        IUnitOfWorkManager uowManager,
        ICurrentTenant currentTenant)
    {
        _scopeFactory = scopeFactory;
        _uowManager = uowManager;
        _currentTenant = currentTenant;
    }

    public async Task SaveAsync(AuditLogInfo info)
    {
        try
        {
            Console.WriteLine($"[AUDIT] TenantId={info.TenantId} User={info.UserName}");
            // Bắt buộc: luôn mở UnitOfWork mới để tránh dính transaction của request hiện hành
            using var uow = _uowManager.Begin(requiresNew: true, isTransactional: false);

            // Chuyển sang Host tenant (null) => trỏ đến connection "Default" của Host DB
            using (_currentTenant.Change(null))
            using (var scope = _scopeFactory.CreateScope())
            {
                // Resolve converter & repo TRONG scope Host
                var converter = scope.ServiceProvider.GetRequiredService<IAuditLogInfoToAuditLogConverter>();
                var repo = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

                var entity = await converter.ConvertAsync(info);
                await repo.InsertAsync(entity, autoSave: true);
            }

            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuditLog Redirect Error] {ex.Message}");
        }
    }
}