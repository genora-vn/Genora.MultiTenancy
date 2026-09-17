using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Service game engine Gamification.
/// QUAN TRỌNG (BD-2): /answer và /finish CHẤM ĐIỂM SERVER-SIDE, không tin totalScore client gửi.
/// </summary>
public interface IHlgGameAppService : IApplicationService
{
    /// <summary>Danh sách game (kèm totalQuestions).</summary>
    Task<List<GameDto>> GetGamesAsync(CancellationToken ct = default);

    /// <summary>Chi tiết một game.</summary>
    Task<GameDto> GetGameAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Bắt đầu chơi game: tạo session + trả câu hỏi (KHÔNG kèm đáp án đúng).
    /// phone để định danh người chơi.
    /// </summary>
    Task<StartGameResultDto> StartGameAsync(Guid gameId, string phone, CancellationToken ct = default);

    /// <summary>
    /// Trả lời 1 câu — server tự chấm điểm (BD-2). Trả {correct, scoreGained}.
    /// Bỏ qua mọi điểm client tự tính.
    /// </summary>
    Task<AnswerResultDto> AnswerAsync(AnswerQuestionPayloadDto payload, CancellationToken ct = default);

    /// <summary>
    /// Kết thúc phiên — server đối soát tổng điểm từ các answer đã ghi (BD-2), bỏ qua totalScore client.
    /// Trả GameResult (reward nối dây ở Phase 4).
    /// </summary>
    Task<GameResultDto> FinishAsync(Guid sessionId, FinishGamePayloadDto payload, CancellationToken ct = default);

    /// <summary>Live-feed người chơi (polling). Realtime SignalR ở Phase 6.</summary>
    Task<List<LivePlayerActivityDto>> GetLiveFeedAsync(Guid gameId, CancellationToken ct = default);
}
