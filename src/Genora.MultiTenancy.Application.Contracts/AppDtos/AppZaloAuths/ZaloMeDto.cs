namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;

public class ZaloMeRequest
{
    public string AccessToken { get; set; } = null!;
}

public class ZaloMeResponse : ZaloBaseResponse
{
    public ZaloMeData Data { get; set; }
}
public class ZaloMeData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string OaId { get; set; }
    public string UserIdByOa { get; set; }
    public bool IsFollower { get; set; }
    public bool IsSensitive { get; set; }
}