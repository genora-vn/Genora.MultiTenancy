using Volo.Abp.Modularity;

namespace Genora.MultiTenancy;

[DependsOn(
    typeof(MultiTenancyDomainModule),
    typeof(MultiTenancyTestBaseModule)
)]
public class MultiTenancyDomainTestModule : AbpModule
{

}
