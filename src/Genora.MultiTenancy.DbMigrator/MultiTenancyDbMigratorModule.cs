using Genora.MultiTenancy.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Genora.MultiTenancy.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(MultiTenancyDomainModule),
    typeof(MultiTenancyEntityFrameworkCoreModule),
    typeof(MultiTenancyApplicationContractsModule)
)]
public class MultiTenancyDbMigratorModule : AbpModule
{
}
