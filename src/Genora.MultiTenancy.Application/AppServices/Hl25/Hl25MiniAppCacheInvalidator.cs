using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>Invalidate only after a successful commit; rollback keeps the previous catalog cache.</summary>
public class Hl25MiniAppCacheInvalidator : ITransientDependency
{
    private readonly Hl25MiniAppCache _cache;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public Hl25MiniAppCacheInvalidator(Hl25MiniAppCache cache, IUnitOfWorkManager unitOfWorkManager)
    {
        _cache = cache;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public Task AfterCommitAsync(Guid? tenantId, params Hl25CacheArea[] areas)
    {
        // Capture the tenant explicitly: CurrentTenant may have changed by the time callbacks execute.
        var uow = _unitOfWorkManager.Current;
        if (uow == null)
            return _cache.InvalidateAsync(tenantId, areas);

        uow.OnCompleted(() => _cache.InvalidateAsync(tenantId, areas));
        return Task.CompletedTask;
    }
}
