using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;

namespace Genora.MultiTenancy.TenantManagement
{
    public interface ICustomTenantRepository : IBasicRepository<Tenant, Guid>
    {
        Task<Tenant> GetTenantByHost(string host, CancellationToken cancellationToken = default);
    }
}
