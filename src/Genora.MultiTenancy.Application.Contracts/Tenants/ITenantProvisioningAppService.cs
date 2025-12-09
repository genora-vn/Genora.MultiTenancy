using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.Tenants;

public interface ITenantProvisioningAppService : IApplicationService
{
    Task<Guid> CreateAndProvisionAsync(CreateTenantProvisionDto input, CancellationToken cancellationToken = default);
}