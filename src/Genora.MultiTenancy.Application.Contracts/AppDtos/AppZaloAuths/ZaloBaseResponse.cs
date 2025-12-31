using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;

public class ZaloBaseResponse
{
    [JsonPropertyName("error")]
    public int Error { get; set; } = 0;

    [JsonPropertyName("message")]
    public string? Message { get; set; } = "Success";
}

