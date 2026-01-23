using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Features.AppZaloLogs;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class AppZaloLogAppService : ApplicationService, IAppZaloLogAppService
{
    private readonly IRepository<ZaloLog, Guid> _repo;
    private readonly ICurrentTenant _currentTenant;

    public AppZaloLogAppService(IRepository<ZaloLog, Guid> repo, ICurrentTenant currentTenant)
    {
        _repo = repo;
        _currentTenant = currentTenant;
    }

    private async Task CheckViewPolicyAsync()
    {
        if (_currentTenant.IsAvailable)
        {
            await AuthorizationService.CheckAsync(MultiTenancyPermissions.AppZaloLogs.Default);
        }
        else
        {
            await AuthorizationService.CheckAsync(MultiTenancyPermissions.HostAppZaloLogs.Default);
        }
    }

    private Guid? GetScopeTenantId()
        => _currentTenant.IsAvailable ? _currentTenant.Id : null;

    public async Task<PagedResultDto<AppZaloLogDto>> GetListAsync(GetZaloLogListInput input)
    {
        await CheckViewPolicyAsync();

        var scopeTenantId = GetScopeTenantId();

        using (_currentTenant.Change(scopeTenantId))
        {
            var query = await _repo.GetQueryableAsync();

            // ✅ Tenant: TenantId == tenant
            // ✅ Host: TenantId == null
            query = query.Where(x => x.TenantId == scopeTenantId);

            if (!string.IsNullOrWhiteSpace(input.LogAction))
                query = query.Where(x => x.Action == input.LogAction);

            if (input.HttpStatus.HasValue)
                query = query.Where(x => x.HttpStatus == input.HttpStatus.Value);

            if (input.From.HasValue)
                query = query.Where(x => x.CreationTime >= input.From.Value);

            if (input.To.HasValue)
            {
                var to = input.To.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreationTime <= to);
            }

            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                var ft = input.FilterText.Trim();
                query = query.Where(x =>
                    x.Endpoint.Contains(ft) ||
                    (x.Error != null && x.Error.Contains(ft))
                );
            }

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? "CreationTime desc"
                : input.Sorting;

            query = query.OrderBy(sorting);

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            return new PagedResultDto<AppZaloLogDto>(
                total,
                ObjectMapper.Map<System.Collections.Generic.List<ZaloLog>, System.Collections.Generic.List<AppZaloLogDto>>(items)
            );
        }
    }

    public async Task<AppZaloLogDto> GetAsync(Guid id)
    {
        await CheckViewPolicyAsync();

        var scopeTenantId = GetScopeTenantId();

        using (_currentTenant.Change(scopeTenantId))
        {
            var entity = await _repo.GetAsync(id);

            if (entity.TenantId != scopeTenantId)
                throw new AbpAuthorizationException("Not allowed.");

            return ObjectMapper.Map<ZaloLog, AppZaloLogDto>(entity);
        }
    }
}
