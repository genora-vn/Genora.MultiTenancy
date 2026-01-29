namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public class MiniAppUserInfoRequest
{
    public string? Id { get; set; }          // SDK: userInfo.id
    public string? Name { get; set; }        // SDK: userInfo.name (sau permission)
    public string? AvatarUrl { get; set; }   // SDK: userInfo.avatar

    public string? IdByOa { get; set; }      // SDK: userInfo.idByOA
    public bool? FollowedOa { get; set; }    // SDK: userInfo.followedOA
    public bool? IsSensitive { get; set; }   // SDK: userInfo.isSensitive
}