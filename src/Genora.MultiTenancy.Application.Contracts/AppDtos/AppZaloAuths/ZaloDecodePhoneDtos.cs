using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public class ZaloDecodePhoneRequest
{   
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }
}

public class ZaloDecodePhoneResponse : ZaloBaseResponse
{
    [JsonPropertyName("data")]
    public DecodePhoneData? Data { get; set; }
}

public class DecodePhoneData
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }
}