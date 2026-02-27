namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public record ZaloRuntimeConfig(
    string AppId,
    string AppSecret,
    string RedirectUri,
    string? MiniAppId,
    string? OaId
);