using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;

public class ZaloBaseResponse
{
    [JsonPropertyName("error")]
    public int Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

