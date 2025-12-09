using Xunit;

namespace Genora.MultiTenancy.EntityFrameworkCore;

[CollectionDefinition(MultiTenancyTestConsts.CollectionDefinitionName)]
public class MultiTenancyEntityFrameworkCoreCollection : ICollectionFixture<MultiTenancyEntityFrameworkCoreFixture>
{

}
