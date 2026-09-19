using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;

[Authorize]
public class HlgUserAdminAppService : ApplicationService, IHlgUserAdminAppService
{
    private readonly IRepository<HlgUserProfile, Guid> _profiles;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly IFeatureChecker _features;
    public HlgUserAdminAppService(IRepository<HlgUserProfile, Guid> profiles, IRepository<Customer, Guid> customers, IFeatureChecker features)
    { _profiles = profiles; _customers = customers; _features = features; }
    public virtual async Task<PagedResultDto<HlgUserAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await AuthorizationService.CheckAsync(CurrentTenant.IsAvailable ? MultiTenancyPermissions.AppHlgUsers.Default : MultiTenancyPermissions.HostAppHlgUsers.Default);
        if (CurrentTenant.IsAvailable && !await _features.IsEnabledAsync(AppHlgFeatures.Management)) throw new AbpAuthorizationException();
        var profiles = await _profiles.GetQueryableAsync();
        var customers = await _customers.GetQueryableAsync();
        var query = from p in profiles join c in customers on p.CustomerId equals c.Id
                    select new HlgUserAdminDto { Id = p.Id, CustomerId = c.Id, CustomerCode = c.CustomerCode,
                        FullName = c.FullName, PhoneNumber = c.PhoneNumber, ZaloId = p.ZaloId, CustomerType = (byte?)p.CustomerType,
                        BonusPoint = c.BonusPoint, IsRegistered = p.IsRegistered, IsActive = c.IsActive };
        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var term = input.FilterText.Trim();
            query = query.Where(x => x.FullName.Contains(term) || x.PhoneNumber.Contains(term) || (x.CustomerCode != null && x.CustomerCode.Contains(term)) || (x.ZaloId != null && x.ZaloId.Contains(term)));
        }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        var count = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.FullName).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlgUserAdminDto>(count, items);
    }
}
