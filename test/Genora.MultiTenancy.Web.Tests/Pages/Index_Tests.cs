using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Genora.MultiTenancy.Pages;

[Collection(MultiTenancyTestConsts.CollectionDefinitionName)]
public class Index_Tests : MultiTenancyWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
