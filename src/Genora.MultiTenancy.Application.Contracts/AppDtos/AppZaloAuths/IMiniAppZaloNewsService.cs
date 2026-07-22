using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;

/// <summary>
/// Service lấy tin tức (bài viết Zalo OA) cho Mini App — gom logic gọi <see cref="IZaloApiClient"/>,
/// cache kết quả theo tenant để giảm số lần gọi Zalo OpenAPI.
///
/// Đây là NGUỒN TIN "Zalo OA", KHÁC với <c>IMiniAppNewsService</c> (tin nội bộ bảng AppNews).
/// Dùng chung cho cả MiniAppController (generic/golf) lẫn HoaLinhMiniAppController.
/// </summary>
public interface IMiniAppZaloNewsService : IApplicationService
{
    /// <summary>Lấy danh sách bài viết Zalo OA (có cache per-tenant).</summary>
    Task<ZaloArticleListResponse> GetArticleListAsync(int offset, int limit, string type, CancellationToken ct = default);

    /// <summary>Lấy chi tiết 1 bài viết Zalo OA theo id (có cache per-tenant).</summary>
    Task<ZaloArticleDetailResponse> GetArticleDetailAsync(string articleId, CancellationToken ct = default);
}
