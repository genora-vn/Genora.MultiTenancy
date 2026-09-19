using System;
namespace Genora.MultiTenancy.AppDtos.Hlg;
public class GameHistoryDto { public Guid Id { get; set; } public Guid GameId { get; set; } public string GameName { get; set; } = ""; public int Score { get; set; } public int DurationSeconds { get; set; } public DateTime FinishedAt { get; set; } }
