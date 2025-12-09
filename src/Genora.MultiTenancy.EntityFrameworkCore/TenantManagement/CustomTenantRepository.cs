using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.TenantManagement;

public class CustomTenantRepository
    : EfCoreRepository<TenantManagementDbContext, Tenant, Guid>, ICustomTenantRepository
{
    private readonly IUnitOfWorkManager _uowManager;

    public CustomTenantRepository(
        IDbContextProvider<TenantManagementDbContext> dbContextProvider,
        IUnitOfWorkManager uowManager)
        : base(dbContextProvider)
    {
        _uowManager = uowManager;
    }

    public async Task<Tenant> GetTenantByHost(string host, CancellationToken cancellationToken = default)
    {
        // Nếu đang có ambient UoW transactional → tách ra 1 scope không transactional
        if (_uowManager.Current?.Options?.IsTransactional == true)
        {
            using (var uow = _uowManager.Begin(requiresNew: true, isTransactional: false))
            {
                var ctx = await GetDbContextAsync();
                var tenant = await ctx.Tenants
                    .Where(u => EF.Property<string>(u, "Host") == host)
                    .FirstOrDefaultAsync(cancellationToken);

                await uow.CompleteAsync();
                return tenant;
            }
        }
        else
        {
            var ctx = await GetDbContextAsync();
           return await ctx.Tenants
                .Where(u => EF.Property<string>(u, "Host") == host)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}