using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Game. Khớp contract Game. type/status là string contract.</summary>
public class GameDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public string? RewardDescription { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int TotalQuestions { get; set; }
}

/// <summary>Lựa chọn đáp án. Khớp contract Question.options[]. key là "A".."D".</summary>
public class AnswerOptionDto
{
    public string Key { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>Câu hỏi. Khớp contract Question. KHÔNG chứa đáp án đúng (BD-2).</summary>
public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public int Index { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<AnswerOptionDto> Options { get; set; } = new();
    public int TimeLimitSec { get; set; }
    public decimal ScoreMultiplier { get; set; }
}

/// <summary>Phiên chơi. Khớp contract GameSession. score là điểm server-side.</summary>
public class GameSessionDto
{
    public Guid SessionId { get; set; }
    public Guid GameId { get; set; }
    public int CurrentIndex { get; set; }
    public List<object> Answers { get; set; } = new();
    public int Score { get; set; }
    public DateTime StartedAt { get; set; }
}

/// <summary>Kết quả bắt đầu game. Khớp contract {session, questions}.</summary>
public class StartGameResultDto
{
    public GameSessionDto Session { get; set; } = new();
    public List<QuestionDto> Questions { get; set; } = new();
}

/// <summary>Payload trả lời câu hỏi. Khớp contract {sessionId, questionId, selectedKey, timeSpentSec}.</summary>
public class AnswerQuestionPayloadDto
{
    public Guid SessionId { get; set; }
    public Guid QuestionId { get; set; }
    public string? SelectedKey { get; set; }
    public int TimeSpentSec { get; set; }
}

/// <summary>Kết quả trả lời (server chấm). Khớp contract {correct, scoreGained}.</summary>
public class AnswerResultDto
{
    public bool Correct { get; set; }
    public int ScoreGained { get; set; }
}

/// <summary>Payload finish game (client gửi — server KHÔNG tin totalScore, chỉ dùng đối soát log). BD-2.</summary>
public class FinishGamePayloadDto
{
    public int TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
}

/// <summary>Phần thưởng. Khớp contract Reward. type: "physical" | "voucher".</summary>
public class RewardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int PointCost { get; set; }
    public string Type { get; set; } = string.Empty;
}

/// <summary>Kết quả game. Khớp contract GameResult. Điểm từ server (BD-2).</summary>
public class GameResultDto
{
    public Guid SessionId { get; set; }
    public int TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public RewardDto? Reward { get; set; }
    public bool RequiresShippingAddress { get; set; }
}

/// <summary>Hoạt động người chơi (live-feed). Khớp contract LivePlayerActivity.</summary>
public class LivePlayerActivityDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Action { get; set; } = string.Empty;
}
