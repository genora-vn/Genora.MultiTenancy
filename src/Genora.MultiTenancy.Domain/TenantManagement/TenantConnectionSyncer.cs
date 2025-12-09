using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.TenantManagement;

namespace Genora.MultiTenancy.TenantManagement;

public class TenantConnectionSyncer :
    ILocalEventHandler<EntityCreatedEventData<Tenant>>,
    ILocalEventHandler<EntityUpdatedEventData<Tenant>>,
    ITransientDependency
{
    private readonly ITenantRepository _repo;

    public TenantConnectionSyncer(ITenantRepository repo) => _repo = repo;

    public async Task HandleEventAsync(EntityCreatedEventData<Tenant> e) => await SyncAsync(e.Entity.Id);
    public async Task HandleEventAsync(EntityUpdatedEventData<Tenant> e) => await SyncAsync(e.Entity.Id);

    private async Task SyncAsync(Guid tenantId)
    {
        var t = await _repo.GetAsync(tenantId, includeDetails: true); // ⭐ nạp details
        var cs = t.GetProperty<string>(Constant.ConnectionString);

        if (!string.IsNullOrWhiteSpace(cs)) t.SetDefaultConnectionString(cs);
        else t.RemoveDefaultConnectionString();

        await _repo.UpdateAsync(t, autoSave: true);
    }
}