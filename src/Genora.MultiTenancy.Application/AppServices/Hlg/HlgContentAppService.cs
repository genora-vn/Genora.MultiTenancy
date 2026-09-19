using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
namespace Genora.MultiTenancy.AppServices.Hlg;
[RemoteService(false), DisableValidation]
public class HlgContentAppService : ApplicationService, IHlgContentAppService
{
    private IRepository<T,Guid> Repo<T>() where T:class,Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T,Guid>>();
    public async Task<List<ContentItemDto>> GetContentAsync()
    {
        var q=await Repo<HlgContentItem>().GetQueryableAsync();
        var games=await Repo<HlgGame>().GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(q.Where(x=>x.IsActive && x.TenantId==CurrentTenant.Id && (x.GameId==null || games.Any(g=>g.Id==x.GameId && g.IsActive))).OrderBy(x=>x.DisplayOrder).ThenBy(x=>x.Id)
            .Select(x=>new ContentItemDto { Id=x.Id, Slot=x.Slot, Title=x.Title, Summary=x.Summary, BadgeText=x.BadgeText, ImageUrl=x.ImageUrl, TargetUrl=x.TargetUrl, GameId=x.GameId }));
    }
    public async Task<List<BrandDto>> GetBrandsAsync(Guid? categoryId=null)
    {
        var q=await Repo<HlgBrand>().GetQueryableAsync(); var cats=await Repo<HlgKnowledgeCategory>().GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(q.Where(x=>x.TenantId==CurrentTenant.Id && x.IsActive && (!categoryId.HasValue || x.CategoryId==categoryId) && cats.Any(c=>c.Id==x.CategoryId && c.IsActive && c.TenantId==x.TenantId))
            .OrderBy(x=>x.DisplayOrder).ThenBy(x=>x.Name).Select(x=>new BrandDto { Id=x.Id, CategoryId=x.CategoryId, Name=x.Name, DisplayOrder=x.DisplayOrder }));
    }
    public async Task<List<ProductDto>> SearchProductsAsync(Guid? categoryId=null, Guid? brandId=null, string? filter=null, int skip=0, int take=50, List<Guid>? productIds=null)
    {
        var q=await Repo<HlgProduct>().GetQueryableAsync(); var cats=await Repo<HlgKnowledgeCategory>().GetQueryableAsync(); var brands=await Repo<HlgBrand>().GetQueryableAsync();
        q=q.Where(x=>x.TenantId==CurrentTenant.Id && x.IsActive && cats.Any(c=>c.Id==x.CategoryId && c.IsActive && c.TenantId==x.TenantId)
            && (x.BrandId==null || brands.Any(b=>b.Id==x.BrandId && b.CategoryId==x.CategoryId && b.IsActive && b.TenantId==x.TenantId)));
        if(productIds is { Count: > 0 }) q=q.Where(x=>productIds.Contains(x.Id));
        if(categoryId.HasValue) q=q.Where(x=>x.CategoryId==categoryId);
        if(brandId.HasValue) q=q.Where(x=>x.BrandId==brandId);
        if(!string.IsNullOrWhiteSpace(filter)) { var term=filter.Trim(); q=q.Where(x=>x.Name.Contains(term) || (x.Summary!=null && x.Summary.Contains(term))); }
        var rows=await AsyncExecuter.ToListAsync(q.OrderBy(x=>x.DisplayOrder).ThenBy(x=>x.Id).Skip(Math.Max(0,skip)).Take(Math.Clamp(take,1,100)));
        return rows.Select(x=>HlgKnowledgeAppService.MapProduct(x,false)).ToList();
    }
    public async Task<List<RankingEventDto>> GetEventsAsync(Guid? gameId=null)
    {
        var q=await Repo<HlgRankingEvent>().GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(q.Where(x=>x.TenantId==CurrentTenant.Id && x.IsActive && (!gameId.HasValue || x.GameId==gameId)).OrderByDescending(x=>x.StartAt)
            .Select(x=>new RankingEventDto { Id=x.Id, GameId=x.GameId, Title=x.Title, Description=x.Description, StartAt=x.StartAt, EndAt=x.EndAt }));
    }
    public async Task<List<RankingPrizeDto>> GetPrizesAsync(Guid eventId)
    {
        var events=await Repo<HlgRankingEvent>().GetQueryableAsync(); var prizes=await Repo<HlgRankingPrize>().GetQueryableAsync(); var rewards=await Repo<HlgReward>().GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(from p in prizes join r in rewards on p.RewardId equals r.Id
            where p.TenantId==CurrentTenant.Id && p.EventId==eventId && p.IsActive && events.Any(e=>e.Id==eventId && e.IsActive)
            orderby p.DisplayOrder,p.Id select new RankingPrizeDto { Id=p.Id, Title=p.Title, RewardId=r.Id, RewardName=r.Name, ImageUrl=r.ImageUrl, Quantity=p.Quantity });
    }
    public async Task<List<RankingWinnerDto>> GetWinnersAsync(Guid eventId)
    {
        var events=await Repo<HlgRankingEvent>().GetQueryableAsync(); var q=await Repo<HlgRankingWinner>().GetQueryableAsync(); var customers=await Repo<Customer>().GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(from w in q join c in customers on w.CustomerId equals c.Id
            where w.TenantId==CurrentTenant.Id && w.EventId==eventId && w.IsActive && events.Any(e=>e.Id==eventId && e.IsActive)
            orderby w.Rank,w.Id select new RankingWinnerDto { UserId=c.Id, DisplayName=c.FullName, AvatarUrl=c.AvatarUrl, Rank=w.Rank, Score=w.Score, PrizeId=w.PrizeId });
    }
}
