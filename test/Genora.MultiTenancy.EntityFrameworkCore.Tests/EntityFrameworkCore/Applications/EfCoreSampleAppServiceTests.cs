using Genora.MultiTenancy.Samples;
using Xunit;

namespace Genora.MultiTenancy.EntityFrameworkCore.Applications;

[Collection(MultiTenancyTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<MultiTenancyEntityFrameworkCoreTestModule>
{

}
