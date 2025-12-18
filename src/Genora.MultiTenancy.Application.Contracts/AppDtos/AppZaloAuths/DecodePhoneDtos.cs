using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public class DecodePhoneRequest
{
    public string Code { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
}

public class ZaloPhoneResponse
{
    [JsonPropertyName("error")]
    public int Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ZaloPhoneData? Data { get; set; }
}

public class ZaloPhoneData
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }
}