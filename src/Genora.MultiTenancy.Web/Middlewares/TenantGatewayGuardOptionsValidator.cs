using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Genora.MultiTenancy.Web.Middlewares;

// Return actionable configuration paths, never secret values.
public sealed class TenantGatewayGuardOptionsValidator : IValidateOptions<TenantGatewayGuardOptions>
{
    private readonly IConfiguration _configuration;

    public TenantGatewayGuardOptionsValidator(IConfiguration configuration) => _configuration = configuration;

    public ValidateOptionsResult Validate(string name, TenantGatewayGuardOptions options)
    {
        var errors = options.GetValidationErrors();
        if (options.Enabled && _configuration.GetValue<bool>("Hl25GatewayGuard:Enabled"))
            errors.Add("Set Hl25GatewayGuard:Enabled=false when TenantGatewayGuard:Enabled=true. The legacy and multi-tenant guards cannot be enabled together.");
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
