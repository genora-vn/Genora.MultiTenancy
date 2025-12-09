using Volo.Abp.Modularity;

namespace Genora.MultiTenancy;

public abstract class MultiTenancyApplicationTestBase<TStartupModule> : MultiTenancyTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
