using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Service knowledge base Gamification. Danh mục + bài học + đánh dấu hoàn thành.
/// isCompleted tính per-user theo phone (nếu có).
/// </summary>
public interface IHlgKnowledgeAppService : IApplicationService
{
    /// <summary>
    /// Ghi nhận tiến độ học 1 bài (server tự chấm %, chống gian lận):
    /// timeSpentSec = số giây ở trang chi tiết; viewedTabs = các tab đã click (info/knowledge/related).
    /// Hoàn thành (100%) = ở >= 60s VÀ click đủ 3 tab. Trả về % + trạng thái hoàn thành.
    /// </summary>
    Task<LearningProgressResultDto> UpdateProgressAsync(Guid productId, string phone, double timeSpentSec, IEnumerable<string>? viewedTabs);
    /// <summary>Danh sách danh mục kiến thức (kèm productCount).</summary>
    Task<List<KnowledgeCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);

    /// <summary>Chi tiết một danh mục.</summary>
    Task<KnowledgeCategoryDto> GetCategoryAsync(Guid id, CancellationToken ct = default);

    /// <summary>Danh sách bài học trong một danh mục. isCompleted theo phone (optional).</summary>
    Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, string? phone = null, CancellationToken ct = default);

    /// <summary>Chi tiết một bài học. isCompleted theo phone (optional).</summary>
    Task<ProductDto> GetProductAsync(Guid id, string? phone = null, CancellationToken ct = default);

    /// <summary>Đánh dấu bài học đã hoàn thành cho người dùng (theo phone).</summary>
    Task CompleteProductAsync(Guid productId, string phone, CancellationToken ct = default);
}
