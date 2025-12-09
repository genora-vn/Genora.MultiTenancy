using Microsoft.AspNetCore.Builder;
using Genora.MultiTenancy;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("Genora.MultiTenancy.Web.csproj"); 
await builder.RunAbpModuleAsync<MultiTenancyWebTestModule>(applicationName: "Genora.MultiTenancy.Web");

public partial class Program
{
}
