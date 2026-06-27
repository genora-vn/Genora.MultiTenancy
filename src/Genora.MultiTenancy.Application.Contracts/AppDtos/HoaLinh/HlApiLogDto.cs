using System;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO hiển thị log gọi API Hoa Linh
/// </summary>
public class HlApiLogDto
{
    public Guid Id { get; set; }
    public string HttpMethod { get; set; } = null!;
    public string RequestUrl { get; set; } = null!;
    public int? ResponseStatusCode { get; set; }
    public long DurationMs { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DataType { get; set; }
    public string? CallerSource { get; set; }
    public DateTime CreationTime { get; set; }
}
