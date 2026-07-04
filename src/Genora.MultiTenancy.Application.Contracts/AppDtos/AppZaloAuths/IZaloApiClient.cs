using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IZaloApiClient
{
    Task<string> SendZnsAsync(object payload, CancellationToken ct);
    Task<string> SendOaMessageAsync(object payload, CancellationToken ct);

    Task<ZaloMeResponse> GetZaloMeAsync(string accessToken, CancellationToken ct);
    Task<ZaloDecodePhoneResponse> DecodePhoneAsync(string code, string accessToken, CancellationToken ct);
    Task<ZaloDecodeLocationResponse> DecodeLocationAsync(string code, string accessToken, CancellationToken ct);

    /// <summary>
    /// Lấy danh sách bài viết Zalo OA (GET /v2.0/article/getslice).
    /// Access token tự lấy từ ZaloAuth active theo tenant.
    /// </summary>
    Task<ZaloArticleListResponse> GetArticleListAsync(int offset, int limit, string type, CancellationToken ct);

    /// <summary>
    /// Lấy chi tiết 1 bài viết Zalo OA (GET /v2.0/article/getdetail).
    /// </summary>
    Task<ZaloArticleDetailResponse> GetArticleDetailAsync(string articleId, CancellationToken ct);
}