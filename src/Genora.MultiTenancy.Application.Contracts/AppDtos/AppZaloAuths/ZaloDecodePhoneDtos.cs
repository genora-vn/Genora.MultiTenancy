using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public class ZaloDecodeRequest
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
public class ZaloDecodeLocationResponse : ZaloBaseResponse
{
    [JsonPropertyName("data")]
    public ZaloLocationData? Data { get; set; }
}

public class DecodePhoneData
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }
}

public class ZaloLocationData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}