namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Payload cập nhật hồ sơ. Khớp contract UpdateProfilePayload.</summary>
public class UpdateProfilePayloadDto
{
    public string? FullName { get; set; }
    public string? Birthday { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

/// <summary>Thống kê hồ sơ. Khớp contract ProfileStats.</summary>
public class ProfileStatsDto
{
    public int Points { get; set; }
    public int KnowledgeLearned { get; set; }
    public int AccuracyPercent { get; set; }
}
