using Genora.MultiTenancy.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace Genora.MultiTenancy.DbMigrator;

public class TenantDbMigrationService : ITransientDependency
{
    private readonly ICurrentTenant _current;
    private readonly ITenantRepository _repo;
    private readonly IDbContextProvider<MultiTenancyDbContext> _db;

    public TenantDbMigrationService(ICurrentTenant c, ITenantRepository r, IDbContextProvider<MultiTenancyDbContext> d)
    { _current = c; _repo = r; _db = d; }

    public async Task MigrateAsync()
    {
        using (_current.Change(null))
        {
            var host = await _db.GetDbContextAsync();
            await host.Database.MigrateAsync();
        }

        var tenants = await _repo.GetListAsync();
        foreach (var t in tenants)
        {
            using (_current.Change(t.Id, t.Name))
            {
                var ctx = await _db.GetDbContextAsync();
                await ctx.Database.MigrateAsync();
            }
        }
    }
}