using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace Genora.MultiTenancy.HttpApi.Controllers;

/// <summary>
/// Controller cho Zalo Mini App "Hoa Linh Gamification".
/// Tách biệt hoàn toàn với HoaLinhMiniAppController (mini app Hoa Linh hiện tại).
/// Mọi response bọc trong envelope { error?, message?, data } (HlgApiResult).
/// </summary>
[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Area("MultiTenancy")]
[Route("api/mini-app/hlg")]
[AllowAnonymous]
public class HoaLinhGamificationController : MultiTenancyController
{
    private readonly IZaloApiClient _zaloApiClient;
    private readonly IHlgProfileAppService _profileService;
    private readonly IHlgKnowledgeAppService _knowledgeService;
    private readonly IHlgGameAppService _gameService;
    private readonly IHlgRewardAppService _rewardService;
    private readonly IHlgRankingAppService _rankingService;
    private readonly ILogger<HoaLinhGamificationController> _logger;

    public HoaLinhGamificationController(
        IZaloApiClient zaloApiClient,
        IHlgProfileAppService profileService,
        IHlgKnowledgeAppService knowledgeService,
        IHlgGameAppService gameService,
        IHlgRewardAppService rewardService,
        IHlgRankingAppService rankingService,
        ILogger<HoaLinhGamificationController> logger)
    {
        _zaloApiClient = zaloApiClient;
        _profileService = profileService;
        _knowledgeService = knowledgeService;
        _gameService = gameService;
        _rewardService = rewardService;
        _rankingService = rankingService;
        _logger = logger;
    }

    #region Auth

    /// <summary>Giải mã số điện thoại từ Zalo code + accessToken.</summary>
    [HttpPost("decode-phone")]
    public async Task<IActionResult> DecodePhone([FromBody] ZaloDecodeRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.AccessToken))
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu code hoặc accessToken"));

        var result = await _zaloApiClient.DecodePhoneAsync(request.Code, request.AccessToken, ct);
        return Ok(HlgApiResult<ZaloDecodePhoneResponse>.Ok(result));
    }

    /// <summary>Đăng ký/đồng bộ khách hàng Gamification (gọi sau decode-phone). customerType gán khi register.</summary>
    [HttpPost("customer/upsert")]
    public async Task<IActionResult> UpsertCustomer([FromBody] HlgCustomerUpsertPayloadDto payload, CancellationToken ct)
    {
        if (payload == null || string.IsNullOrWhiteSpace(payload.Phone))
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu số điện thoại"));

        var dto = await _profileService.UpsertCustomerAsync(payload, ct);
        return Ok(HlgApiResult<GamificationUserDto>.Ok(dto));
    }

    /// <summary>Lấy thông tin người chơi Gamification theo số điện thoại.</summary>
    [HttpGet("customer/by-phone")]
    public async Task<IActionResult> GetCustomerByPhone([FromQuery] string phone, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu số điện thoại"));

        try
        {
            var dto = await _profileService.GetByPhoneAsync(phone, ct);
            return Ok(HlgApiResult<GamificationUserDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    #endregion

    #region Profile

    /// <summary>Cập nhật hồ sơ người chơi. Trả về GamificationUser.</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromQuery] string phone, [FromBody] UpdateProfilePayloadDto payload, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu số điện thoại"));

        try
        {
            var dto = await _profileService.UpdateProfileAsync(phone, payload, ct);
            return Ok(HlgApiResult<GamificationUserDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(400, ex.Message));
        }
    }

    /// <summary>Thống kê hồ sơ: điểm, số kiến thức đã học, độ chính xác.</summary>
    [HttpGet("profile/stats")]
    public async Task<IActionResult> GetStats([FromQuery] string phone, CancellationToken ct)
    {
        try
        {
            var dto = await _profileService.GetStatsAsync(phone, ct);
            return Ok(HlgApiResult<ProfileStatsDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    /// <summary>Lịch sử học kiến thức.</summary>
    [HttpGet("profile/learning-history")]
    public async Task<IActionResult> GetLearningHistory([FromQuery] string phone, CancellationToken ct)
    {
        try
        {
            var list = await _profileService.GetLearningHistoryAsync(phone, ct);
            return Ok(HlgApiResult<System.Collections.Generic.List<LearningHistoryItemDto>>.Ok(list));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    /// <summary>Lịch sử biến động điểm.</summary>
    [HttpGet("profile/point-history")]
    public async Task<IActionResult> GetPointHistory([FromQuery] string phone, CancellationToken ct)
    {
        try
        {
            var list = await _profileService.GetPointHistoryAsync(phone, ct);
            return Ok(HlgApiResult<System.Collections.Generic.List<PointHistoryItemDto>>.Ok(list));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    /// <summary>Lịch sử đổi quà.</summary>
    [HttpGet("profile/reward-history")]
    public async Task<IActionResult> GetRewardHistory([FromQuery] string phone, CancellationToken ct)
    {
        try
        {
            var list = await _profileService.GetRewardHistoryAsync(phone, ct);
            return Ok(HlgApiResult<System.Collections.Generic.List<RewardHistoryItemDto>>.Ok(list));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    #endregion

    #region Knowledge

    /// <summary>Danh sách danh mục kiến thức.</summary>
    [HttpGet("knowledge/categories")]
    public async Task<IActionResult> GetKnowledgeCategories(CancellationToken ct)
    {
        var list = await _knowledgeService.GetCategoriesAsync(ct);
        return Ok(HlgApiResult<System.Collections.Generic.List<KnowledgeCategoryDto>>.Ok(list));
    }

    /// <summary>Chi tiết một danh mục kiến thức.</summary>
    [HttpGet("knowledge/categories/{id}")]
    public async Task<IActionResult> GetKnowledgeCategory(Guid id, CancellationToken ct)
    {
        try
        {
            var dto = await _knowledgeService.GetCategoryAsync(id, ct);
            return Ok(HlgApiResult<KnowledgeCategoryDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    /// <summary>Danh sách bài học trong một danh mục. isCompleted theo phone (optional).</summary>
    [HttpGet("knowledge/categories/{id}/products")]
    public async Task<IActionResult> GetKnowledgeProducts(Guid id, [FromQuery] string? phone, CancellationToken ct)
    {
        var list = await _knowledgeService.GetProductsByCategoryAsync(id, phone, ct);
        return Ok(HlgApiResult<System.Collections.Generic.List<ProductDto>>.Ok(list));
    }

    /// <summary>Chi tiết một bài học. isCompleted theo phone (optional).</summary>
    [HttpGet("knowledge/products/{id}")]
    public async Task<IActionResult> GetKnowledgeProduct(Guid id, [FromQuery] string? phone, CancellationToken ct)
    {
        try
        {
            var dto = await _knowledgeService.GetProductAsync(id, phone, ct);
            return Ok(HlgApiResult<ProductDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    /// <summary>Đánh dấu bài học đã hoàn thành cho người dùng (theo phone).</summary>
    [HttpPost("knowledge/products/{id}/complete")]
    public async Task<IActionResult> CompleteKnowledgeProduct(Guid id, [FromQuery] string phone, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu số điện thoại"));

        try
        {
            await _knowledgeService.CompleteProductAsync(id, phone, ct);
            return Ok(HlgApiResult<object>.Ok(null!));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    #endregion

    #region Games

    /// <summary>Danh sách game.</summary>
    [HttpGet("games")]
    public async Task<IActionResult> GetGames(CancellationToken ct)
    {
        var list = await _gameService.GetGamesAsync(ct);
        return Ok(HlgApiResult<System.Collections.Generic.List<GameDto>>.Ok(list));
    }

    /// <summary>Chi tiết một game.</summary>
    [HttpGet("games/{id}")]
    public async Task<IActionResult> GetGame(Guid id, CancellationToken ct)
    {
        try
        {
            var dto = await _gameService.GetGameAsync(id, ct);
            return Ok(HlgApiResult<GameDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(404, ex.Message));
        }
    }

    /// <summary>Bắt đầu chơi game: tạo session + trả câu hỏi (KHÔNG kèm đáp án đúng).</summary>
    [HttpPost("games/{id}/start")]
    public async Task<IActionResult> StartGame(Guid id, [FromQuery] string phone, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu số điện thoại"));

        try
        {
            var dto = await _gameService.StartGameAsync(id, phone, ct);
            return Ok(HlgApiResult<StartGameResultDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(400, ex.Message));
        }
    }

    /// <summary>Trả lời 1 câu — server tự chấm điểm (chống gian lận). Trả {correct, scoreGained}.</summary>
    [HttpPost("games/answer")]
    public async Task<IActionResult> Answer([FromBody] AnswerQuestionPayloadDto payload, CancellationToken ct)
    {
        if (payload == null)
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu dữ liệu"));

        try
        {
            var dto = await _gameService.AnswerAsync(payload, ct);
            return Ok(HlgApiResult<AnswerResultDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(400, ex.Message));
        }
    }

    /// <summary>Kết thúc phiên — server đối soát tổng điểm từ answer đã ghi (bỏ qua totalScore client).</summary>
    [HttpPost("games/sessions/{sessionId}/finish")]
    public async Task<IActionResult> Finish(Guid sessionId, [FromBody] FinishGamePayloadDto payload, CancellationToken ct)
    {
        try
        {
            var dto = await _gameService.FinishAsync(sessionId, payload ?? new FinishGamePayloadDto(), ct);
            return Ok(HlgApiResult<GameResultDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(400, ex.Message));
        }
    }

    /// <summary>Live-feed người chơi (polling). Realtime SignalR ở Phase 6.</summary>
    [HttpGet("games/{id}/live-feed")]
    public async Task<IActionResult> GetLiveFeed(Guid id, CancellationToken ct)
    {
        var list = await _gameService.GetLiveFeedAsync(id, ct);
        return Ok(HlgApiResult<System.Collections.Generic.List<LivePlayerActivityDto>>.Ok(list));
    }

    /// <summary>Lưu địa chỉ giao hàng cho phiên (luồng consumer nhận quà vật lý sau game). Endpoint MỚI.</summary>
    [HttpPost("games/sessions/{sessionId}/shipping-address")]
    public async Task<IActionResult> SetSessionShippingAddress(Guid sessionId, [FromBody] ShippingAddressPayloadDto payload, CancellationToken ct)
    {
        try
        {
            await _rewardService.SetSessionShippingAddressAsync(sessionId, payload, ct);
            return Ok(HlgApiResult<object>.Ok(null!));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(400, ex.Message));
        }
    }

    #endregion

    #region Rewards

    /// <summary>Danh sách quà có thể đổi.</summary>
    [HttpGet("rewards")]
    public async Task<IActionResult> GetRewards(CancellationToken ct)
    {
        var list = await _rewardService.GetRewardsAsync(ct);
        return Ok(HlgApiResult<System.Collections.Generic.List<RewardDto>>.Ok(list));
    }

    /// <summary>Đổi điểm lấy quà (trừ BonusPoint). shippingAddressId cho quà vật lý của consumer.</summary>
    [HttpPost("rewards/{id}/redeem")]
    public async Task<IActionResult> RedeemReward(Guid id, [FromQuery] string phone, [FromQuery] Guid? shippingAddressId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Ok(HlgApiResult<object>.Fail(400, "Thiếu số điện thoại"));

        try
        {
            var dto = await _rewardService.RedeemAsync(id, phone, shippingAddressId, ct);
            return Ok(HlgApiResult<RewardHistoryItemDto>.Ok(dto));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlgApiResult<object>.Fail(400, ex.Message));
        }
    }

    #endregion

    #region Ranking

    /// <summary>Sự kiện xếp hạng hiện tại.</summary>
    [HttpGet("ranking/event")]
    public async Task<IActionResult> GetRankingEvent(CancellationToken ct)
    {
        var dto = await _rankingService.GetCurrentEventAsync(ct);
        return Ok(HlgApiResult<RankingEventDto?>.Ok(dto));
    }

    /// <summary>Bảng xếp hạng của sự kiện hiện tại. isCurrentUser đánh dấu theo phone (optional).</summary>
    [HttpGet("ranking/entries")]
    public async Task<IActionResult> GetRankingEntries([FromQuery] string? phone, [FromQuery] int top, CancellationToken ct)
    {
        var list = await _rankingService.GetEntriesAsync(phone, top <= 0 ? 50 : top, ct);
        return Ok(HlgApiResult<System.Collections.Generic.List<RankingEntryDto>>.Ok(list));
    }

    #endregion
}
