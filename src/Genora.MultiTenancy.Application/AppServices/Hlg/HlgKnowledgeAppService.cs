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
        var prodQ = await _productRepo.GetQueryableAsync();
        var counts = await AsyncExecuter.ToListAsync(
            prodQ.Where(p => p.IsActive)
                 .GroupBy(p => p.CategoryId)
                 .Select(g => new { CategoryId = g.Key, Count = g.Count() }), ct);
        var countByCat = counts.ToDictionary(x => x.CategoryId, x => x.Count);

        return categories.Select(c => new KnowledgeCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            ProductCount = countByCat.TryGetValue(c.Id, out var n) ? n : 0
        }).ToList();
    }

    public async Task<KnowledgeCategoryDto> GetCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _categoryRepo.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new UserFriendlyException("Không tìm thấy danh mục");

        var count = await _productRepo.CountAsync(p => p.CategoryId == id && p.IsActive, ct);

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
        var prodQ = await _productRepo.GetQueryableAsync();
        var products = await AsyncExecuter.ToListAsync(
            prodQ.Where(p => p.CategoryId == categoryId && p.IsActive)
                 .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name), ct);

        var completedIds = await GetCompletedProductIdsAsync(phone, ct);

        return products.Select(p => MapProduct(p, completedIds.Contains(p.Id))).ToList();
    }

    public async Task<ProductDto> GetProductAsync(Guid id, string? phone = null, CancellationToken ct = default)
    {
        var p = await _productRepo.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new UserFriendlyException("Không tìm thấy bài học");

        var completedIds = await GetCompletedProductIdsAsync(phone, ct);
        return MapProduct(p, completedIds.Contains(p.Id));
    }

    public async Task CompleteProductAsync(Guid productId, string phone, CancellationToken ct = default)
    {
        var customer = await ResolveCustomerAsync(phone, ct);

        var product = await _productRepo.FirstOrDefaultAsync(x => x.Id == productId, ct)
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

    private static ProductDto MapProduct(HlgProduct p, bool isCompleted)
    {
        return new ProductDto
        {
            Id = p.Id,
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
