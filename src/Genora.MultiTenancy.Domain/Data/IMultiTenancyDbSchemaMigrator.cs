using System.Threading.Tasks;

namespace Genora.MultiTenancy.Data;

public interface IMultiTenancyDbSchemaMigrator
{
    Task MigrateAsync();
}
