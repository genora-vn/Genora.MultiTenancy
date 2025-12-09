using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.Data;

/* This is used if database provider does't define
 * IMultiTenancyDbSchemaMigrator implementation.
 */
public class NullMultiTenancyDbSchemaMigrator : IMultiTenancyDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
