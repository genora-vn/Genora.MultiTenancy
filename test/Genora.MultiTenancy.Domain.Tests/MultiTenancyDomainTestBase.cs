using Volo.Abp.Modularity;

namespace Genora.MultiTenancy;

/* Inherit from this class for your domain layer tests. */
public abstract class MultiTenancyDomainTestBase<TStartupModule> : MultiTenancyTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
