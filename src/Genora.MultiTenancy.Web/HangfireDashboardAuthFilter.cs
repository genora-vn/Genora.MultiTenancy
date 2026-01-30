using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Genora.MultiTenancy.Web;
public sealed class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();

        var env = http.RequestServices.GetRequiredService<IHostEnvironment>();
        var cfg = http.RequestServices.GetRequiredService<IConfiguration>();

        var enabled = cfg.GetValue("Hangfire:DashboardEnabled", true);
        if (!enabled) return false;

        // ✅ DEV cho vào thẳng
        if (env.IsDevelopment())
            return true;

        // ✅ STAGING / PROD: bắt buộc login
        if (http.User?.Identity?.IsAuthenticated != true)
            return false;

        // ✅ chỉ cho admin (đổi theo role thực tế)
        return http.User.IsInRole("admin")
            || http.User.IsInRole("Admin");
    }
}