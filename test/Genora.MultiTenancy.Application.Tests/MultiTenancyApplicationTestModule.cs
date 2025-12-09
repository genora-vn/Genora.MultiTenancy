using Volo.Abp.Modularity;

namespace Genora.MultiTenancy;

[DependsOn(
    typeof(MultiTenancyApplicationModule),
    typeof(MultiTenancyDomainTestModule)
)]
public class MultiTenancyApplicationTestModule : AbpModule
{

}
