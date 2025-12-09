using Genora.MultiTenancy.Samples;
using Xunit;

namespace Genora.MultiTenancy.EntityFrameworkCore.Domains;

[Collection(MultiTenancyTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<MultiTenancyEntityFrameworkCoreTestModule>
{

}
