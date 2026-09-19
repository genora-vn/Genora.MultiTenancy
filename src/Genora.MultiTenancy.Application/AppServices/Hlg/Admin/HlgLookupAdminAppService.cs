using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;
[Authorize]
public class HlgLookupAdminAppService : ApplicationService, IHlgLookupAdminAppService
{
    private IRepository<T,Guid> Repo<T>() where T:class,Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T,Guid>>();
    public virtual async Task<PagedResultDto<HlgLookupDto>> GetListAsync(HlgLookupInput input)
    {
        if (CurrentTenant.IsAvailable && !await LazyServiceProvider.LazyGetRequiredService<IFeatureChecker>().IsEnabledAsync(AppHlgFeatures.Management)) throw new AbpAuthorizationException();
        var groups = input.Kind switch {
            "categories" or "brands" or "products" => new[]{ "Knowledge" },
            "games" => new[]{ "Games", "Knowledge.Create", "Knowledge.Edit", "Ranking.Create", "Ranking.Edit", "Content.Create", "Content.Edit" },
            "events" or "prizes" => new[]{ "Ranking" },
            "rewards" => new[]{ "Rewards", "Ranking.Create", "Ranking.Edit" },
            "customers" => new[]{ "Users", "Ranking.Create", "Ranking.Edit" },
            _ => Array.Empty<string>() };
        var granted=false;
        foreach(var group in groups) if(await AuthorizationService.IsGrantedAsync("MultiTenancy."+(CurrentTenant.IsAvailable?"AppHlg":"HostAppHlg")+group)) { granted=true; break; }
        if(!granted) throw new AbpAuthorizationException();
        IQueryable<HlgLookupDto> q;
        switch(input.Kind) {
            case "categories": q=(await Repo<HlgKnowledgeCategory>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.Name }); break;
            case "brands": q=(await Repo<HlgBrand>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id && (!input.ParentId.HasValue || x.CategoryId==input.ParentId)).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.Name }); break;
            case "products": q=(await Repo<HlgProduct>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.Name }); break;
            case "games": q=(await Repo<HlgGame>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.Name }); break;
            case "events": q=(await Repo<HlgRankingEvent>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.Title }); break;
            case "prizes": q=(await Repo<HlgRankingPrize>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id && (!input.ParentId.HasValue || x.EventId==input.ParentId)).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.Title }); break;
            case "rewards": q=(await Repo<HlgReward>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.Name }); break;
            default:
                var profiles=await Repo<HlgUserProfile>().GetQueryableAsync();
                q=(await Repo<Customer>().GetQueryableAsync()).Where(x=>x.TenantId==CurrentTenant.Id && profiles.Any(p=>p.CustomerId==x.Id)).Select(x=>new HlgLookupDto { Id=x.Id,Name=x.FullName+" ("+x.CustomerCode+")" }); break;
        }
        if(input.Id.HasValue) q=q.Where(x=>x.Id==input.Id);
        if(!string.IsNullOrWhiteSpace(input.FilterText)) { var term=input.FilterText.Trim(); q=q.Where(x=>x.Name.Contains(term)); }
        var count=await AsyncExecuter.CountAsync(q);
        return new(count,await AsyncExecuter.ToListAsync(q.OrderBy(x=>x.Name).ThenBy(x=>x.Id).Skip(Math.Max(0,input.SkipCount)).Take(Math.Clamp(input.MaxResultCount,1,50))));
    }
}
