namespace Genora.MultiTenancy.Gateway;

// Compatibility entry points for the original single-tenant configuration/deployment.
public static class Hl25GatewayExtensions
{
    public const string GatewayKeyHeader = "X-Hl25-Gateway-Key";
    public static IServiceCollection AddHl25Gateway(this IServiceCollection services, IConfiguration configuration)
        => services.AddTenantGateway(configuration);
    public static WebApplication UseHl25Gateway(this WebApplication app) => app.UseTenantGateway();
}
