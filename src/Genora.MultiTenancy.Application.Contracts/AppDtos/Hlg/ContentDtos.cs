using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Genora.MultiTenancy.Hlg;
namespace Genora.MultiTenancy.AppDtos.Hlg;
public class BrandDto { public Guid Id { get; set; } public Guid CategoryId { get; set; } public string Name { get; set; } = ""; public int DisplayOrder { get; set; } }
public class ContentItemDto
{
    public Guid Id { get; set; }
    public HlgContentSlot Slot { get; set; }
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public string? BadgeText { get; set; }
    public string? ImageUrl { get; set; }
    public string? TargetUrl { get; set; }
    public Guid? GameId { get; set; }
}
public class RankingPrizeDto { public Guid Id { get; set; } public string Title { get; set; } = ""; public Guid RewardId { get; set; } public string RewardName { get; set; } = ""; public string? ImageUrl { get; set; } public int Quantity { get; set; } }
public class RankingWinnerDto : RankingEntryDto { public Guid PrizeId { get; set; } }
public interface IHlgContentAppService : IApplicationService
{
    Task<List<ContentItemDto>> GetContentAsync();
    Task<List<BrandDto>> GetBrandsAsync(Guid? categoryId = null);
    Task<List<ProductDto>> SearchProductsAsync(Guid? categoryId = null, Guid? brandId = null, string? filter = null, int skip = 0, int take = 50, List<Guid>? productIds = null);
    Task<List<RankingEventDto>> GetEventsAsync(Guid? gameId = null);
    Task<List<RankingPrizeDto>> GetPrizesAsync(Guid eventId);
    Task<List<RankingWinnerDto>> GetWinnersAsync(Guid eventId);
}
