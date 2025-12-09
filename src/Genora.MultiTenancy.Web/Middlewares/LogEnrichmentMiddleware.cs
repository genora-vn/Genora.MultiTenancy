using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Tracing;
using Volo.Abp.Users;

namespace Genora.MultiTenancy.Web.Middlewares;
public class LogEnrichmentMiddleware : IMiddleware, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly ICorrelationIdProvider _correlation;

    public LogEnrichmentMiddleware(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationIdProvider correlation)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _correlation = correlation;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        using (LogContext.PushProperty("TenantId", _currentTenant.Id, destructureObjects: false))
        using (LogContext.PushProperty("TenantName", _currentTenant.Name, false))
        using (LogContext.PushProperty("UserId", _currentUser.Id, false))
        using (LogContext.PushProperty("UserName", _currentUser.UserName, false))
        using (LogContext.PushProperty("CorrelationId", _correlation.Get(), false))
        {
            await next(context);
        }
    }
}
