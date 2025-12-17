using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

[Authorize(MultiTenancyPermissions.HostAppZaloLogs.Default)]
public class AppZaloLogAppService : ApplicationService, IAppZaloLogAppService
{
    private readonly IRepository<ZaloLog, Guid> _repo;
    private readonly ICurrentTenant _currentTenant;

    public AppZaloLogAppService(IRepository<ZaloLog, Guid> repo, ICurrentTenant currentTenant)
    {
        _repo = repo;
        _currentTenant = currentTenant;
    }

    public async Task<PagedResultDto<AppZaloLogDto>> GetListAsync(GetZaloLogListInput input)
    {
        using (_currentTenant.Change(null))
        {
            var query = await _repo.GetQueryableAsync();

            query = query.Where(x => x.TenantId == null);

            if (!string.IsNullOrWhiteSpace(input.LogAction))
                query = query.Where(x => x.Action == input.LogAction);

            if (input.HttpStatus.HasValue)
                query = query.Where(x => x.HttpStatus == input.HttpStatus.Value);

            if (input.From.HasValue)
            {
                query = query.Where(x => x.CreationTime >= input.From.Value);
            }

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
        using (_currentTenant.Change(null))
        {
            var entity = await _repo.GetAsync(id);
            return ObjectMapper.Map<ZaloLog, AppZaloLogDto>(entity);
        }
    }
}