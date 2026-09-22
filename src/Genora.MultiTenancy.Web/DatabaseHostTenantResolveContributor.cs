using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Web;

public class DatabaseHostTenantResolveContributor : TenantResolveContributorBase
{
    public override string Name => "DatabaseHost";

    public override async Task ResolveAsync(ITenantResolveContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null) return;

        // Lấy Host string từ Request
        var hostName = httpContext.Request.Host.Host; // .Host sẽ tự động bỏ Port (ví dụ: "hoalinh-staging.genora.vn:443" -> "hoalinh-staging.genora.vn")

        if (!string.IsNullOrEmpty(hostName))
        {
            var tenantStore = context.ServiceProvider.GetRequiredService<ITenantStore>();

            // Tìm Tenant theo Host
            var tenant = await tenantStore.FindAsync(hostName);
            if (tenant != null)
            {
                context.TenantIdOrName = tenant.Id.ToString();
            }
        }
    }
}