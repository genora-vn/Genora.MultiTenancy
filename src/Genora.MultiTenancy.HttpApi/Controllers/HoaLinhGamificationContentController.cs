using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
namespace Genora.MultiTenancy.HttpApi.Controllers;
[AllowAnonymous, IgnoreAntiforgeryToken, RemoteService(false)]
[Route("api/mini-app/hlg")]
public class HoaLinhGamificationContentController : MultiTenancyController
{
    private readonly IHlgContentAppService _content;
    private readonly IHlgKnowledgeAppService _knowledge;
    private readonly IHlgRankingAppService _ranking;
    public HoaLinhGamificationContentController(IHlgContentAppService content, IHlgKnowledgeAppService knowledge, IHlgRankingAppService ranking) { _content=content; _knowledge=knowledge; _ranking=ranking; }
    [HttpGet("content")]
    public async Task<object> Content() => HlgApiResult<List<ContentItemDto>>.Ok(await _content.GetContentAsync());
    [HttpGet("knowledge/brands")]
    public async Task<object> Brands(Guid? categoryId=null) => HlgApiResult<List<BrandDto>>.Ok(await _content.GetBrandsAsync(categoryId));
    [HttpGet("knowledge/products")]
    public async Task<object> Products(Guid? categoryId=null, Guid? brandId=null, string? filter=null, int skip=0, int take=50, [FromQuery] List<Guid>? productIds=null) => HlgApiResult<List<ProductDto>>.Ok(await _content.SearchProductsAsync(categoryId,brandId,filter,skip,take,productIds));
    [HttpGet("ranking/events")]
    public async Task<object> Events(Guid? gameId=null) => HlgApiResult<List<RankingEventDto>>.Ok(await _content.GetEventsAsync(gameId));
    [HttpGet("ranking/events/{id}/prizes")]
    public async Task<object> Prizes(Guid id) => HlgApiResult<List<RankingPrizeDto>>.Ok(await _content.GetPrizesAsync(id));
    [HttpGet("ranking/events/{id}/winners")]
    public async Task<object> Winners(Guid id) => HlgApiResult<List<RankingWinnerDto>>.Ok(await _content.GetWinnersAsync(id));
    [HttpGet("ranking/events/{id}/entries")]
    public async Task<object> Entries(Guid id, string? phone=null, int top=50) => HlgApiResult<List<RankingEntryDto>>.Ok(await _ranking.GetEventEntriesAsync(id,phone,top));
    [HttpPost("knowledge/products/{id}/progress")]
    public async Task<object> Progress(Guid id, [Required] string phone, [FromBody] HlgProgressPayload payload)
    {
        var result = await _knowledge.UpdateProgressAsync(id, phone, payload?.TimeSpentSec ?? 0, payload?.ViewedTabs);
        return HlgApiResult<LearningProgressResultDto>.Ok(result);
    }
}
/// <summary>Payload ghi nhận tiến độ học. FE gửi tổng thời gian ở trang + tập tab đã click.</summary>
public class HlgProgressPayload
{
    /// <summary>Tổng số giây người dùng đã ở trên trang chi tiết bài học (tích lũy).</summary>
    public double TimeSpentSec { get; set; }
    /// <summary>Các tab đã click: "info" (Thông tin SP), "knowledge" (Kiến thức SP), "related" (SP liên quan).</summary>
    public List<string>? ViewedTabs { get; set; }
}
