using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hlg;

/// <summary>
/// Knowledge base Gamification: danh mục + bài học + đánh dấu hoàn thành.
/// isCompleted tính per-user từ HlgLearningProgress theo phone.
/// Internal service — controller gọi trực tiếp.
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlgKnowledgeAppService : ApplicationService, IHlgKnowledgeAppService
{
    private readonly IRepository<HlgKnowledgeCategory, Guid> _categoryRepo;
    private readonly IRepository<HlgProduct, Guid> _productRepo;
    private readonly IRepository<HlgLearningProgress, Guid> _progressRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<HlgKnowledgeAppService> _logger;

    public HlgKnowledgeAppService(
        IRepository<HlgKnowledgeCategory, Guid> categoryRepo,
        IRepository<HlgProduct, Guid> productRepo,
        IRepository<HlgLearningProgress, Guid> progressRepo,
        IRepository<Customer, Guid> customerRepo,
        ICurrentTenant currentTenant,
        ILogger<HlgKnowledgeAppService> logger)
    {
        _categoryRepo = categoryRepo;
        _productRepo = productRepo;
        _progressRepo = progressRepo;
        LocalizationResource = typeof(Genora.MultiTenancy.Localization.MultiTenancyResource);
        _customerRepo = customerRepo;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<List<KnowledgeCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var catQ = await _categoryRepo.GetQueryableAsync();
        var categories = await AsyncExecuter.ToListAsync(
            catQ.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name), ct);

        // Đếm số bài học active theo từng danh mục (1 query gộp).
        var prodQ = await VisibleProductsAsync();
        var counts = await AsyncExecuter.ToListAsync(
            prodQ.Where(p => p.IsActive)
                 .GroupBy(p => p.CategoryId)
                 .Select(g => new { CategoryId = g.Key, Count = g.Count() }), ct);
        var countByCat = counts.ToDictionary(x => x.CategoryId, x => x.Count);
        var brands = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgBrand, Guid>>().GetQueryableAsync();
        var products = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgProduct, Guid>>().GetQueryableAsync();
        var result = categories.Select(c => new KnowledgeCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            ProductCount = countByCat.TryGetValue(c.Id, out var n) ? n : 0
        }).ToList();
        foreach (var c in result)
        {
            c.Brands = brands.Where(b => b.CategoryId == c.Id && b.IsActive).OrderBy(b => b.DisplayOrder).ThenBy(b => b.Name)
                .Select(b => new BrandKnowledgeDto { Id = b.Id, Name = b.Name, CategoryId = b.CategoryId }).ToList();
            foreach (var b in c.Brands)
            {
                b.Products = products.Where(p => p.BrandId == b.Id && p.IsActive).OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
                    .Select(p => new BrandProductDto { Id = p.Id, Name = p.Name, ImageUrl = p.ThumbnailUrl, Description = p.Summary }).ToList();
            }
        }
        return result;
    }

    public async Task<KnowledgeCategoryDto> GetCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _categoryRepo.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct)
            ?? throw new UserFriendlyException("Không tìm thấy danh mục");
        
        var count = await AsyncExecuter.CountAsync((await VisibleProductsAsync()).Where(p => p.CategoryId == id), ct);

        return new KnowledgeCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            ProductCount = count
        };
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, string? phone = null, CancellationToken ct = default)
    {
        var prodQ = await VisibleProductsAsync();
        var products = await AsyncExecuter.ToListAsync(
            prodQ.Where(p => p.CategoryId == categoryId && p.IsActive)
                 .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name), ct);

        var completedIds = await GetCompletedProductIdsAsync(phone, ct);

        return products.Select(p => MapProduct(p, completedIds.Contains(p.Id))).ToList();
    }

    public async Task<ProductDto> GetProductAsync(Guid id, string? phone = null, CancellationToken ct = default)
    {
        var p = await AsyncExecuter.FirstOrDefaultAsync((await VisibleProductsAsync()).Where(x => x.Id == id), ct)
            ?? throw new UserFriendlyException("Không tìm thấy bài học");

        var completedIds = await GetCompletedProductIdsAsync(phone, ct);
        var dto = MapProduct(p, completedIds.Contains(p.Id));
        if (dto.Details.GameId.HasValue && !await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame,Guid>>().AnyAsync(g=>g.Id==dto.Details.GameId && g.IsActive && g.TenantId==_currentTenant.Id,ct)) dto.Details.GameId=null;
        var ids = dto.Details.RelatedProductIds;
        var related = await AsyncExecuter.ToListAsync((await VisibleProductsAsync()).Where(x => ids.Contains(x.Id)), ct);
        dto.RelatedProducts = ids.Where(id => related.Any(x => x.Id == id)).Select(id => MapProduct(related.Single(x => x.Id == id), completedIds.Contains(id))).ToList();
        // Omit archived targets from public relations, while keeping the stored CMS configuration intact.
        dto.Details.RelatedProductIds = dto.RelatedProducts.Select(x => x.Id).ToList();
        return dto;
    }

    public async Task CompleteProductAsync(Guid productId, string phone, CancellationToken ct = default)
    {
        var customer = await ResolveCustomerAsync(phone, ct);

        var product = await AsyncExecuter.FirstOrDefaultAsync((await VisibleProductsAsync()).Where(x => x.Id == productId), ct)
            ?? throw new UserFriendlyException("Không tìm thấy bài học");

        var progress = await _progressRepo.FirstOrDefaultAsync(
            x => x.CustomerId == customer.Id && x.ProductId == productId, ct);

        if (progress == null)
        {
            progress = new HlgLearningProgress(GuidGenerator.Create(), customer.Id, productId, _currentTenant.Id)
            {
                ProgressPercent = 100,
                IsCompleted = true,
                CompletedAt = Clock.Now,
                LastViewedAt = Clock.Now
            };
            await _progressRepo.InsertAsync(progress, autoSave: true, cancellationToken: ct);
        }
        else
        {
            progress.ProgressPercent = 100;
            progress.IsCompleted = true;
            progress.CompletedAt ??= Clock.Now;
            progress.LastViewedAt = Clock.Now;
            await _progressRepo.UpdateAsync(progress, autoSave: true, cancellationToken: ct);
        }

        _logger.LogInformation("HLG: customer {CustomerId} hoàn thành bài học {ProductId}", customer.Id, productId);
    }


    private async Task<IQueryable<HlgProduct>> VisibleProductsAsync()
    {
        var products = await _productRepo.GetQueryableAsync();
        var categories = await _categoryRepo.GetQueryableAsync();
        var brands = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgBrand,Guid>>().GetQueryableAsync();
        return products.Where(p => p.IsActive && p.TenantId == _currentTenant.Id
            && categories.Any(c => c.Id == p.CategoryId && c.IsActive && c.TenantId == p.TenantId)
            && (p.BrandId == null || brands.Any(b => b.Id == p.BrandId && b.CategoryId == p.CategoryId && b.IsActive && b.TenantId == p.TenantId)));
    }
    /// <summary>Ba tab bắt buộc trên trang chi tiết bài học (khớp FE): Thông tin SP / Kiến thức SP / SP liên quan.</summary>
    private static readonly string[] RequiredTabs = { "info", "knowledge", "related" };

    /// <summary>Số giây tối thiểu ở trang để đủ điều kiện hoàn thành.</summary>
    private const int RequiredSecondsOnPage = 60;

    /// <summary>
    /// Ghi nhận tiến độ học 1 bài theo hành vi thực tế trên trang chi tiết. Server TỰ chấm % (chống gian lận,
    /// FE không gửi %): 3 tab (info/knowledge/related) = 60% (mỗi tab 20%) + thời gian ở trang = 40% (đủ 60s là tối đa).
    /// HOÀN THÀNH (100%) = ở >= 60s VÀ click đủ 3 tab. Model aggregate: mỗi (customer, product) 1 dòng,
    /// giữ % cao nhất, không hạ tiến độ/đảo trạng thái đã hoàn thành.
    /// </summary>
    public async Task<LearningProgressResultDto> UpdateProgressAsync(Guid productId, string phone, double timeSpentSec, IEnumerable<string>? viewedTabs)
    {
        // Xác thực bài học tồn tại & đang hiển thị (nhẹ, không nạp related/game như GetProductAsync).
        if (!await AsyncExecuter.AnyAsync((await VisibleProductsAsync()).Where(x => x.Id == productId)))
            throw new UserFriendlyException("Không tìm thấy bài học");

        var customer = await ResolveCustomerAsync(phone, default);

        var seconds = double.IsNaN(timeSpentSec) || timeSpentSec < 0 ? 0 : timeSpentSec;
        var distinctTabs = (viewedTabs ?? Enumerable.Empty<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => RequiredTabs.Contains(t))
            .Distinct()
            .Count();

        var completed = seconds >= RequiredSecondsOnPage && distinctTabs >= RequiredTabs.Length;
        var tabPercent = distinctTabs * (60 / RequiredTabs.Length);                                              // 3 tab × 20% = tối đa 60%
        var timePercent = (int)Math.Round(Math.Min(seconds / RequiredSecondsOnPage, 1.0) * 40, MidpointRounding.AwayFromZero); // tối đa 40%
        var percent = completed ? 100 : Math.Min(99, tabPercent + timePercent);                                 // chỉ đạt 100% khi thực sự hoàn thành

        var progress = await _progressRepo.FirstOrDefaultAsync(x => x.CustomerId == customer.Id && x.ProductId == productId);
        if (progress == null)
        {
            progress = new HlgLearningProgress(GuidGenerator.Create(), customer.Id, productId, _currentTenant.Id);
            await _progressRepo.InsertAsync(progress, autoSave: true);
        }

        progress.ProgressPercent = Math.Max(progress.ProgressPercent, percent);
        if (completed && !progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = Clock.Now;
        }
        progress.LastViewedAt = Clock.Now;
        await _progressRepo.UpdateAsync(progress, autoSave: true);

        _logger.LogInformation("HLG: tiến độ học customer {CustomerId} bài {ProductId} = {Percent}% (tab {Tabs}/3, {Seconds}s, completed={Completed})",
            customer.Id, productId, progress.ProgressPercent, distinctTabs, (int)seconds, progress.IsCompleted);

        return new LearningProgressResultDto { ProgressPercent = progress.ProgressPercent, IsCompleted = progress.IsCompleted };
    }
    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Lấy set ProductId đã hoàn thành của user theo phone. Trả rỗng nếu phone null/không tìm thấy.</summary>
    private async Task<HashSet<Guid>> GetCompletedProductIdsAsync(string? phone, CancellationToken ct)
    {
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized)) return new HashSet<Guid>();

        var customer = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct);
        if (customer == null) return new HashSet<Guid>();

        var q = await _progressRepo.GetQueryableAsync();
        var ids = await AsyncExecuter.ToListAsync(
            q.Where(x => x.CustomerId == customer.Id && x.IsCompleted).Select(x => x.ProductId), ct);
        return ids.ToHashSet();
    }

    private async Task<Customer> ResolveCustomerAsync(string phone, CancellationToken ct)
    {
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new UserFriendlyException("Thiếu số điện thoại");

        return await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct)
            ?? throw new UserFriendlyException("Không tìm thấy khách hàng. Vui lòng đăng ký trước.");
    }

    internal static ProductDto MapProduct(HlgProduct p, bool isCompleted)
    {
        return new ProductDto
        {
            Id = p.Id,
            BrandId = p.BrandId,
            Details = JsonSerializer.Deserialize<Genora.MultiTenancy.Hlg.HlgProductContent>(p.DetailsJson ?? "{}") ?? new(),
            CategoryId = p.CategoryId,
            Name = p.Name,
            ThumbnailUrl = p.ThumbnailUrl,
            Summary = p.Summary,
            Content = p.Content,
            Images = ParseImages(p.ImagesJson),
            IsCompleted = isCompleted
        };
    }

    /// <summary>Parse ImagesJson (JSON array) → List string. Trả rỗng nếu null/lỗi.</summary>
    private static List<string> ParseImages(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return Regex.Replace(phone.Trim(), @"\s+|-|\.", "");
    }
}
