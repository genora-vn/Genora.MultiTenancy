using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.Data;

public class MultiTenancyDbMigrationService : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMultiTenancyDbSchemaMigrator _migrator;
    private readonly IUnitOfWorkManager _uow;
    private readonly IDataSeeder _dataSeeder;
    private readonly IIdentityDataSeeder _identityDataSeeder;

    public MultiTenancyDbMigrationService(
        ICurrentTenant currentTenant,
        ITenantRepository tenantRepository,
        IMultiTenancyDbSchemaMigrator migrator,
        IUnitOfWorkManager uow,
        IDataSeeder dataSeeder,
        IIdentityDataSeeder identityDataSeeder)
    {
        _currentTenant = currentTenant;
        _tenantRepository = tenantRepository;
        _migrator = migrator;
        _uow = uow;
        _dataSeeder = dataSeeder;
        _identityDataSeeder = identityDataSeeder;
    }
    public async Task MigrateAsync()
    {
        // ===== HOST =====
        using (_currentTenant.Change(null))
        {
            await _migrator.MigrateAsync();

            // seed admin + quyền host
            await _dataSeeder.SeedAsync(
                new DataSeedContext(null)
                    .WithProperty("AdminEmail", "xinchao@genora.vn")
                    .WithProperty("AdminPassword", "1q2w3E*")
            );
        }

        // ===== TENANTS =====
        using (_currentTenant.Change(null))
        using (var uow = _uow.Begin(requiresNew: true, isTransactional: false))
        {
            var tenants = await _tenantRepository.GetListAsync();
            await uow.CompleteAsync();

            foreach (var t in tenants)
            {
                using (_currentTenant.Change(t.Id, t.Name))
                {
                    await _migrator.MigrateAsync();

                    // Nếu muốn seed admin user cho tenant (chỉ khi thiếu):
                    // await _identityDataSeeder.SeedAsync("admin@" + t.Name + ".local", "Admin@123", t.Id);

                    // Gán quyền full cho role 'admin' trong tenant
                    await _dataSeeder.SeedAsync(new DataSeedContext(t.Id));
                }
            }
        }
    }
}