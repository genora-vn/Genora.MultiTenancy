namespace Genora.MultiTenancy.Enums.Hlg;

/// <summary>Trạng thái game. Map contract: "upcoming" | "ongoing" | "ended".</summary>
public enum HlgGameStatus : byte
{
    Upcoming = 1,
    Ongoing = 2,
    Ended = 3
}
