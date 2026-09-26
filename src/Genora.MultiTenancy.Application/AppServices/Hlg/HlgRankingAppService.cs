using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Hlg;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hlg;

/// <summary>
/// Xếp hạng Gamification (BD-5). Ranking reset theo sự kiện:
/// điểm tính từ các phiên game đã finish trong khoảng [StartAt, EndAt] của sự kiện hiện tại.
/// Internal service — controller gọi trực tiếp.
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlgRankingAppService : ApplicationService, IHlgRankingAppService
{
    private readonly IRepository<HlgRankingEvent, Guid> _eventRepo;
    private readonly IRepository<HlgGameSession, Guid> _sessionRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ILogger<HlgRankingAppService> _logger;

    public HlgRankingAppService(
        IRepository<HlgRankingEvent, Guid> eventRepo,
        IRepository<HlgGameSession, Guid> sessionRepo,
        IRepository<Customer, Guid> customerRepo,
        ILogger<HlgRankingAppService> logger)
    {
        _eventRepo = eventRepo;
        _sessionRepo = sessionRepo;
        _customerRepo = customerRepo;
        _logger = logger;
    }

    public async Task<RankingEventDto?> GetCurrentEventAsync(CancellationToken ct = default)
    {
        var ev = await GetActiveEventAsync(ct);
        return ev == null ? null : MapEvent(ev);
    }

    public async Task<List<RankingEntryDto>> GetEntriesAsync(string? phone = null, int top = 50, CancellationToken ct = default)
    {
        var ev = await GetActiveEventAsync(ct);
        if (ev == null) return new List<RankingEntryDto>();

        return await BuildEntriesAsync(ev, phone, top, ct);
    }
    public async Task<List<RankingEntryDto>> GetEventEntriesAsync(Guid eventId, string? phone = null, int top = 50, CancellationToken ct = default)
    {
        var ev = await AsyncExecuter.FirstOrDefaultAsync((await _eventRepo.GetQueryableAsync()).Where(x => x.Id == eventId && x.IsActive && x.TenantId == CurrentTenant.Id));
        if (ev == null) return new();
        return await BuildEntriesAsync(ev, phone, top, ct);
    }
    private async Task<List<RankingEntryDto>> BuildEntriesAsync(HlgRankingEvent ev, string? phone, int top, CancellationToken ct)
    {
        top = Math.Clamp(top, 1, 1000);
        // Tổng điểm mỗi người chơi = sum(Score) các phiên finish trong khoảng sự kiện (BD-5).
        var sessionQ = await _sessionRepo.GetQueryableAsync();
        var finished = sessionQ.Where(s =>
            s.TenantId == CurrentTenant.Id && (ev.GameId == null || s.GameId == ev.GameId) && s.IsFinished
            && s.FinishedAt != null
            && s.FinishedAt >= ev.StartAt
            && s.FinishedAt <= ev.EndAt);

        var aggregated = await AsyncExecuter.ToListAsync(
            finished.GroupBy(s => s.CustomerId)
                    .Select(g => new { CustomerId = g.Key, Score = g.Sum(x => x.Score) }), ct);

        if (aggregated.Count == 0) return new List<RankingEntryDto>();

        // Xác định customer hiện tại (nếu có phone).
        Guid? currentCustomerId = null;
        var normalized = NormalizePhone(phone);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            var current = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct);
            currentCustomerId = current?.Id;
        }

        // Sắp xếp giảm dần theo điểm → gán rank (tuple, tránh dynamic/anonymous-type footgun).
        var ranked = aggregated
            .OrderByDescending(x => x.Score).ThenBy(x => x.CustomerId)
            .Select((x, i) => (CustomerId: x.CustomerId, Score: x.Score, Rank: i + 1))
            .ToList();

        // Lấy thông tin hiển thị của các customer liên quan (top + current).
        var neededIds = ranked.Take(top).Select(x => x.CustomerId).ToList();
        if (currentCustomerId.HasValue && !neededIds.Contains(currentCustomerId.Value))
            neededIds.Add(currentCustomerId.Value);

        var custQ = await _customerRepo.GetQueryableAsync();
        var customers = await AsyncExecuter.ToListAsync(
            custQ.Where(c => neededIds.Contains(c.Id))
                 .Select(c => new { c.Id, c.FullName, c.AvatarUrl }), ct);
        var custById = customers.ToDictionary(x => x.Id, x => x);

        RankingEntryDto ToDto((Guid CustomerId, int Score, int Rank) r)
        {
            custById.TryGetValue(r.CustomerId, out var c);
            return new RankingEntryDto
            {
                Rank = r.Rank,
                UserId = r.CustomerId,
                DisplayName = c?.FullName ?? "Người chơi",
                AvatarUrl = c?.AvatarUrl,
                Score = r.Score,
                IsCurrentUser = currentCustomerId.HasValue && r.CustomerId == currentCustomerId.Value
            };
        }

        var result = ranked.Take(top).Select(ToDto).ToList();

        // Luôn kèm dòng của user hiện tại nếu ngoài top (để mini app hiển thị vị trí).
        if (currentCustomerId.HasValue && result.All(e => e.UserId != currentCustomerId.Value))
        {
            var mine = ranked.FirstOrDefault(x => x.CustomerId == currentCustomerId.Value);
            if (mine.CustomerId != Guid.Empty) result.Add(ToDto(mine));
        }

        return result;
    }

    public async Task<HlgRankingShareImageResultDto> SaveShareImageAsync(string phone, byte[]? content, CancellationToken ct = default)
    {
        // phone là ĐỊNH DANH khách hàng (không phải chứng cứ auth) — dùng cơ chế phân giải khách hàng hiện có.
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new UserFriendlyException(message: "Thiếu hoặc sai số điện thoại.", code: "400");

        if (content == null || content.Length == 0)
            throw new UserFriendlyException(message: "Thiếu file ảnh.", code: "400");

        if (content.Length > HlgRankingShareImageConsts.MaxBytes)
            throw new UserFriendlyException(
                message: $"Ảnh vượt quá dung lượng cho phép ({HlgRankingShareImageConsts.MaxBytes / (1024 * 1024)}MB).",
                code: "413");

        var custQ = await _customerRepo.GetQueryableAsync();
        var customer = await AsyncExecuter.FirstOrDefaultAsync(custQ.Where(x => x.PhoneNumber == normalized), ct)
            ?? throw new UserFriendlyException(message: "Không tìm thấy khách hàng.", code: "404");

        // Kiểm tra SIGNATURE (magic bytes) để xác định kiểu THẬT, KHÔNG tin extension/MIME client.
        string ext;
        if (HasPngSignature(content)) ext = ".png";
        else if (HasJpegSignature(content)) ext = ".jpg";
        else throw new UserFriendlyException(message: "Ảnh không hợp lệ. Chỉ hỗ trợ PNG hoặc JPEG.", code: "400");

        // Giải mã bằng decoder để chắc chắn là ảnh hợp lệ + lấy kích thước pixel (chặn vượt giới hạn).
        try
        {
            using var probe = new MemoryStream(content, writable: false);
            var info = Image.Identify(probe);
            if (info.Width <= 0 || info.Height <= 0
                || info.Width > HlgRankingShareImageConsts.MaxSide
                || info.Height > HlgRankingShareImageConsts.MaxSide
                || (long)info.Width * info.Height > HlgRankingShareImageConsts.MaxPixels)
                throw new UserFriendlyException(
                    message: $"Kích thước ảnh vượt giới hạn ({HlgRankingShareImageConsts.MaxSide}px mỗi cạnh, tối đa 16 megapixel).",
                    code: "400");
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new UserFriendlyException(message: "Ảnh không hợp lệ. Chỉ hỗ trợ PNG hoặc JPEG.", code: "400");
        }

        // Lưu ảnh: UUID server sinh (KHÔNG dùng filename/phone làm path công khai, không ghi đè ảnh người khác);
        // ghi nguyên bytes gốc để giữ đầy đủ danh sách (không crop/re-encode).
        string relativePath;
        try
        {
            var dir = Path.Combine("wwwroot", HlgRankingShareImageConsts.PublicSubPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(dir);
            var fileName = Guid.NewGuid().ToString("N") + ext;
            await File.WriteAllBytesAsync(Path.Combine(dir, fileName), content, ct);
            relativePath = $"/{HlgRankingShareImageConsts.PublicSubPath}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HLG: lưu ảnh chia sẻ ranking thất bại (customer {CustomerId})", customer.Id);
            throw new UserFriendlyException(message: "Lưu ảnh thất bại. Vui lòng thử lại.", code: "500");
        }

        _logger.LogInformation("HLG: đã lưu ảnh chia sẻ ranking cho customer {CustomerId} tại {Path}", customer.Id, relativePath);
        return new HlgRankingShareImageResultDto { Url = ToFullUrl(relativePath) };
    }

    /// <summary>Dựng URL tuyệt đối (scheme+host+path) từ path tương đối; không có HTTP context (test) → trả nguyên path.</summary>
    private string ToFullUrl(string relativePath)
    {
        var request = LazyServiceProvider.LazyGetService<IHttpContextAccessor>()?.HttpContext?.Request;
        if (request == null) return relativePath;
        var baseUrl = $"{request.Scheme}://{request.Host.Value}";
        return relativePath.StartsWith("/") ? baseUrl + relativePath : baseUrl + "/" + relativePath;
    }

    /// <summary>Signature PNG: 89 50 4E 47 0D 0A 1A 0A.</summary>
    private static bool HasPngSignature(byte[] b) =>
        b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
        && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A;

    /// <summary>Signature JPEG: FF D8 FF.</summary>
    private static bool HasJpegSignature(byte[] b) =>
        b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Sự kiện đang hiệu lực: IsActive + đang trong khoảng thời gian; ưu tiên mới nhất.</summary>
    private async Task<HlgRankingEvent?> GetActiveEventAsync(CancellationToken ct)
    {
        var now = Clock.Now;
        var q = await _eventRepo.GetQueryableAsync();
        var events = await AsyncExecuter.ToListAsync(
            q.Where(e => e.TenantId == CurrentTenant.Id && e.IsActive && e.StartAt <= now && e.EndAt >= now)
             .OrderByDescending(e => e.StartAt), ct);

        return events.FirstOrDefault();
    }

    private static RankingEventDto MapEvent(HlgRankingEvent e) => new()
    {
        Id = e.Id,
        GameId = e.GameId,
        Title = e.Title,
        Description = e.Description,
        StartAt = e.StartAt,
        EndAt = e.EndAt
    };

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return Regex.Replace(phone.Trim(), @"\s+|-|\.", "");
    }
}
