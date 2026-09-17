using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Realtime;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hlg;

/// <summary>
/// Game engine Gamification.
/// ⚠️ CHẤM ĐIỂM SERVER-SIDE (BD-2): /answer tự chấm dựa CorrectKey (bí mật server),
/// /finish đối soát tổng điểm từ HlgSessionAnswer đã ghi — KHÔNG tin totalScore client gửi.
/// Điểm cộng vào Customer.BonusPoint (AD-2). Internal service — controller gọi trực tiếp.
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlgGameAppService : ApplicationService, IHlgGameAppService
{
    private readonly IRepository<HlgGame, Guid> _gameRepo;
    private readonly IRepository<HlgQuestion, Guid> _questionRepo;
    private readonly IRepository<HlgAnswerOption, Guid> _optionRepo;
    private readonly IRepository<HlgGameSession, Guid> _sessionRepo;
    private readonly IRepository<HlgSessionAnswer, Guid> _answerRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IHlgLiveFeedNotifier _liveFeedNotifier;
    private readonly ILogger<HlgGameAppService> _logger;

    public HlgGameAppService(
        IRepository<HlgGame, Guid> gameRepo,
        IRepository<HlgQuestion, Guid> questionRepo,
        IRepository<HlgAnswerOption, Guid> optionRepo,
        IRepository<HlgGameSession, Guid> sessionRepo,
        IRepository<HlgSessionAnswer, Guid> answerRepo,
        IRepository<Customer, Guid> customerRepo,
        ICurrentTenant currentTenant,
        IHlgLiveFeedNotifier liveFeedNotifier,
        ILogger<HlgGameAppService> logger)
    {
        _gameRepo = gameRepo;
        _questionRepo = questionRepo;
        _optionRepo = optionRepo;
        _sessionRepo = sessionRepo;
        _answerRepo = answerRepo;
        _customerRepo = customerRepo;
        _currentTenant = currentTenant;
        _liveFeedNotifier = liveFeedNotifier;
        _logger = logger;
    }

    public async Task<List<GameDto>> GetGamesAsync(CancellationToken ct = default)
    {
        var gameQ = await _gameRepo.GetQueryableAsync();
        var games = await AsyncExecuter.ToListAsync(
            gameQ.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.CreationTime), ct);

        var counts = await GetQuestionCountsAsync(games.Select(g => g.Id).ToList(), ct);
        return games.Select(g => MapGame(g, counts.TryGetValue(g.Id, out var n) ? n : 0)).ToList();
    }

    public async Task<GameDto> GetGameAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _gameRepo.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new UserFriendlyException("Không tìm thấy game");

        var count = await _questionRepo.CountAsync(q => q.GameId == id && q.IsActive, ct);
        return MapGame(game, count);
    }

    public async Task<StartGameResultDto> StartGameAsync(Guid gameId, string phone, CancellationToken ct = default)
    {
        var customer = await ResolveCustomerAsync(phone, ct);

        var game = await _gameRepo.FirstOrDefaultAsync(x => x.Id == gameId && x.IsActive, ct)
            ?? throw new UserFriendlyException("Không tìm thấy game");

        if (game.Status == HlgGameStatus.Ended)
            throw new UserFriendlyException("Game đã kết thúc");

        // Lấy câu hỏi + options (KHÔNG kèm CorrectKey ra client — BD-2).
        var questionQ = await _questionRepo.GetQueryableAsync();
        var questions = await AsyncExecuter.ToListAsync(
            questionQ.Where(q => q.GameId == gameId && q.IsActive).OrderBy(q => q.Index), ct);

        var questionIds = questions.Select(q => q.Id).ToList();
        var optionQ = await _optionRepo.GetQueryableAsync();
        var options = await AsyncExecuter.ToListAsync(
            optionQ.Where(o => questionIds.Contains(o.QuestionId)), ct);
        var optionsByQuestion = options
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(o => o.Key).ToList());

        // Tạo session.
        var session = new HlgGameSession(GuidGenerator.Create(), gameId, customer.Id, _currentTenant.Id)
        {
            CurrentIndex = 0,
            Score = 0,
            CorrectCount = 0,
            TotalQuestions = questions.Count,
            StartedAt = Clock.Now
        };
        session = await _sessionRepo.InsertAsync(session, autoSave: true, cancellationToken: ct);

        _logger.LogInformation("HLG: bắt đầu game {GameId} session {SessionId} customer {CustomerId}",
            gameId, session.Id, customer.Id);

        return new StartGameResultDto
        {
            Session = MapSession(session),
            Questions = questions.Select(q => MapQuestion(q,
                optionsByQuestion.TryGetValue(q.Id, out var opts) ? opts : new List<HlgAnswerOption>())).ToList()
        };
    }

    public async Task<AnswerResultDto> AnswerAsync(AnswerQuestionPayloadDto payload, CancellationToken ct = default)
    {
        var session = await _sessionRepo.FirstOrDefaultAsync(x => x.Id == payload.SessionId, ct)
            ?? throw new UserFriendlyException("Không tìm thấy phiên chơi");

        if (session.IsFinished)
            throw new UserFriendlyException("Phiên chơi đã kết thúc");

        var question = await _questionRepo.FirstOrDefaultAsync(x => x.Id == payload.QuestionId, ct)
            ?? throw new UserFriendlyException("Không tìm thấy câu hỏi");

        if (question.GameId != session.GameId)
            throw new UserFriendlyException("Câu hỏi không thuộc game của phiên chơi");

        // Chống trả lời trùng câu (idempotent theo session+question).
        var existing = await _answerRepo.FirstOrDefaultAsync(
            x => x.SessionId == session.Id && x.QuestionId == question.Id, ct);
        if (existing != null)
            return new AnswerResultDto { Correct = existing.IsCorrect, ScoreGained = existing.ScoreGained };

        // ===== CHẤM ĐIỂM SERVER-SIDE (BD-2) =====
        var selectedKey = HlgEnumMapper.AnswerKeyFromString(payload.SelectedKey);
        var isCorrect = selectedKey.HasValue && selectedKey.Value == question.CorrectKey;

        var game = await _gameRepo.GetAsync(session.GameId, cancellationToken: ct);
        var scoreGained = isCorrect ? ComputeScore(game.BaseScorePerQuestion, question.ScoreMultiplier, payload.TimeSpentSec, question.TimeLimitSec) : 0;

        var answer = new HlgSessionAnswer(GuidGenerator.Create(), session.Id, question.Id, _currentTenant.Id)
        {
            SelectedKey = selectedKey ?? default,
            IsCorrect = isCorrect,
            ScoreGained = scoreGained,
            TimeSpentSec = payload.TimeSpentSec
        };
        await _answerRepo.InsertAsync(answer, autoSave: true, cancellationToken: ct);

        // Cập nhật điểm tích lũy server-side.
        session.Score += scoreGained;
        if (isCorrect) session.CorrectCount += 1;
        session.CurrentIndex += 1;
        await _sessionRepo.UpdateAsync(session, autoSave: true, cancellationToken: ct);

        // Broadcast live-feed khi trả lời đúng (BD-4). Bọc try/catch để không fail luồng chơi.
        if (isCorrect)
            await BroadcastActivityAsync(session.GameId, session.CustomerId, $"vừa ghi {scoreGained} điểm", ct);

        return new AnswerResultDto { Correct = isCorrect, ScoreGained = scoreGained };
    }

    public async Task<GameResultDto> FinishAsync(Guid sessionId, FinishGamePayloadDto payload, CancellationToken ct = default)
    {
        var session = await _sessionRepo.FirstOrDefaultAsync(x => x.Id == sessionId, ct)
            ?? throw new UserFriendlyException("Không tìm thấy phiên chơi");

        // ===== ĐỐI SOÁT SERVER-SIDE (BD-2): tổng từ answer đã ghi, BỎ QUA totalScore client =====
        var answerQ = await _answerRepo.GetQueryableAsync();
        var answers = await AsyncExecuter.ToListAsync(answerQ.Where(a => a.SessionId == sessionId), ct);

        var serverTotalScore = answers.Sum(a => a.ScoreGained);
        var serverCorrectCount = answers.Count(a => a.IsCorrect);

        // Cảnh báo nếu client gửi điểm khác server (dấu hiệu gian lận).
        if (payload.TotalScore != serverTotalScore)
        {
            _logger.LogWarning(
                "HLG anti-cheat: session {SessionId} client totalScore={ClientScore} != server={ServerScore}",
                sessionId, payload.TotalScore, serverTotalScore);
        }

        if (!session.IsFinished)
        {
            session.Score = serverTotalScore;
            session.CorrectCount = serverCorrectCount;
            session.IsFinished = true;
            session.FinishedAt = Clock.Now;
            await _sessionRepo.UpdateAsync(session, autoSave: true, cancellationToken: ct);

            // Cộng điểm vào Customer.BonusPoint (AD-2). Chỉ cộng 1 lần khi finish.
            var customer = await _customerRepo.FirstOrDefaultAsync(x => x.Id == session.CustomerId, ct);
            if (customer != null && serverTotalScore > 0)
            {
                customer.BonusPoint += serverTotalScore;
                await _customerRepo.UpdateAsync(customer, autoSave: true, cancellationToken: ct);
            }

            _logger.LogInformation("HLG: finish session {SessionId} score={Score} correct={Correct}/{Total}",
                sessionId, serverTotalScore, serverCorrectCount, session.TotalQuestions);

            // Broadcast live-feed khi hoàn thành game (BD-4). Bọc try/catch để không fail luồng chơi.
            await BroadcastActivityAsync(session.GameId, session.CustomerId, $"vừa hoàn thành với {serverTotalScore} điểm", ct);
        }

        // reward + requiresShippingAddress nối dây ở Phase 4 (Rewards & Shipping).
        return new GameResultDto
        {
            SessionId = session.Id,
            TotalScore = session.Score,
            CorrectCount = session.CorrectCount,
            TotalQuestions = session.TotalQuestions,
            Reward = null,
            RequiresShippingAddress = false
        };
    }

    public async Task<List<LivePlayerActivityDto>> GetLiveFeedAsync(Guid gameId, CancellationToken ct = default)
    {
        // Lấy các phiên gần đây của game → hoạt động người chơi. Realtime SignalR ở Phase 6.
        var sessionQ = await _sessionRepo.GetQueryableAsync();
        var recent = await AsyncExecuter.ToListAsync(
            sessionQ.Where(s => s.GameId == gameId)
                    .OrderByDescending(s => s.LastModificationTime ?? s.CreationTime)
                    .Take(20), ct);

        if (recent.Count == 0) return new List<LivePlayerActivityDto>();

        var customerIds = recent.Select(s => s.CustomerId).Distinct().ToList();
        var custQ = await _customerRepo.GetQueryableAsync();
        var customers = await AsyncExecuter.ToListAsync(
            custQ.Where(c => customerIds.Contains(c.Id)).Select(c => new { c.Id, c.FullName, c.AvatarUrl }), ct);
        var custById = customers.ToDictionary(x => x.Id, x => x);

        return recent.Select(s =>
        {
            custById.TryGetValue(s.CustomerId, out var c);
            var action = s.IsFinished ? $"đạt {s.Score} điểm" : "đang chơi";
            return new LivePlayerActivityDto
            {
                UserId = s.CustomerId,
                DisplayName = c?.FullName ?? "Người chơi",
                AvatarUrl = c?.AvatarUrl,
                Action = action
            };
        }).ToList();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Công thức chấm điểm server-side (BD-2). base × multiplier × timeFactor.
    /// timeFactor: trả lời tức thì = 1.0, hết giờ = 0.5 (thưởng tốc độ). Frontend tạm dùng 100×2.5.
    /// </summary>
    private static int ComputeScore(int baseScore, decimal multiplier, int timeSpentSec, int timeLimitSec)
    {
        double timeFactor = 1.0;
        if (timeLimitSec > 0)
        {
            var ratio = Math.Clamp(timeSpentSec / (double)timeLimitSec, 0.0, 1.0);
            timeFactor = 1.0 - ratio * 0.5; // 1.0 → 0.5
        }
        var raw = baseScore * (double)multiplier * timeFactor;
        return (int)Math.Round(raw, MidpointRounding.AwayFromZero);
    }

    private async Task<Dictionary<Guid, int>> GetQuestionCountsAsync(List<Guid> gameIds, CancellationToken ct)
    {
        if (gameIds.Count == 0) return new Dictionary<Guid, int>();
        var q = await _questionRepo.GetQueryableAsync();
        var counts = await AsyncExecuter.ToListAsync(
            q.Where(x => gameIds.Contains(x.GameId) && x.IsActive)
             .GroupBy(x => x.GameId)
             .Select(g => new { GameId = g.Key, Count = g.Count() }), ct);
        return counts.ToDictionary(x => x.GameId, x => x.Count);
    }

    private async Task<Customer> ResolveCustomerAsync(string phone, CancellationToken ct)
    {
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new UserFriendlyException("Thiếu số điện thoại");

        return await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct)
            ?? throw new UserFriendlyException("Không tìm thấy khách hàng. Vui lòng đăng ký trước.");
    }

    private static GameDto MapGame(HlgGame g, int totalQuestions) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Type = HlgEnumMapper.GameTypeToString(g.Type),
        ImageUrl = g.ImageUrl,
        Description = g.Description,
        Rules = g.Rules,
        RewardDescription = g.RewardDescription,
        Status = HlgEnumMapper.GameStatusToString(g.Status),
        StartAt = g.StartAt,
        EndAt = g.EndAt,
        TotalQuestions = totalQuestions
    };

    private static QuestionDto MapQuestion(HlgQuestion q, List<HlgAnswerOption> options) => new()
    {
        Id = q.Id,
        GameId = q.GameId,
        Index = q.Index,
        Content = q.Content,
        ImageUrl = q.ImageUrl,
        Options = options.Select(o => new AnswerOptionDto
        {
            Key = HlgEnumMapper.AnswerKeyToString(o.Key),
            Content = o.Content
        }).ToList(),
        TimeLimitSec = q.TimeLimitSec,
        ScoreMultiplier = q.ScoreMultiplier
        // KHÔNG map CorrectKey — bí mật server-side (BD-2).
    };

    private static GameSessionDto MapSession(HlgGameSession s) => new()
    {
        SessionId = s.Id,
        GameId = s.GameId,
        CurrentIndex = s.CurrentIndex,
        Answers = new List<object>(),
        Score = s.Score,
        StartedAt = s.StartedAt
    };

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return Regex.Replace(phone.Trim(), @"\s+|-|\.", "");
    }

    /// <summary>
    /// Broadcast hoạt động người chơi tới live-feed (BD-4). Bọc try/catch để lỗi SignalR
    /// KHÔNG làm fail luồng chơi chính (theo lesson feedback_signalr_try_catch).
    /// </summary>
    private async Task BroadcastActivityAsync(Guid gameId, Guid customerId, string action, CancellationToken ct)
    {
        try
        {
            var customer = await _customerRepo.FirstOrDefaultAsync(x => x.Id == customerId, ct);
            var activity = new LivePlayerActivityDto
            {
                UserId = customerId,
                DisplayName = customer?.FullName ?? "Người chơi",
                AvatarUrl = customer?.AvatarUrl,
                Action = action
            };
            await _liveFeedNotifier.PlayerActivityAsync(gameId, activity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
    }
}
