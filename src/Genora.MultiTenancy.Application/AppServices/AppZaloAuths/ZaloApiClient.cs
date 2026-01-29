using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloApiClient : BaseZaloClient, IZaloApiClient
{
    private readonly IZaloTokenProvider _tokenProvider;

    public ZaloApiClient(
        IHttpClientFactory factory,
        IZaloTokenProvider tokenProvider,
        IZaloLogWriter logWriter,
        IConfiguration cfg,
        ILogger<BaseZaloClient> logger)
        : base(factory, cfg, logWriter, logger)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task<string> SendZnsAsync(object payload, CancellationToken ct)
    {
        var url = "https://business.openapi.zalo.me/message/template";
        return await PostJsonWithAccessTokenHeaderAsync(ZaloLogActions.SEND_ZNS, url, payload, ct);
    }

    public async Task<string> SendOaMessageAsync(object payload, CancellationToken ct)
    {
        var url = "https://openapi.zalo.me/v3.0/oa/message/cs";
        return await PostJsonWithAccessTokenHeaderAsync(ZaloLogActions.SEND_OA_MSG, url, payload, ct);
    }

    private async Task<string> PostJsonWithAccessTokenHeaderAsync(string action, string url, object payload, CancellationToken ct)
    {
        var token = await _tokenProvider.GetAccessTokenAsync();
        var json = JsonSerializer.Serialize(payload);

        var headers = new Dictionary<string, string>
        {
            ["access_token"] = token
        };

        var body = await SendAsync(HttpMethod.Post, url, headers, action, json, ct);

        if (IsLikelyInvalidToken(body))
        {
            await _tokenProvider.RefreshNowAsync();

            var token2 = await _tokenProvider.GetAccessTokenAsync();
            headers["access_token"] = token2;

            body = await SendAsync(HttpMethod.Post, url, headers, action, json, ct);
        }

        return body;
    }

    public async Task<ZaloMeResponse> GetZaloMeAsync(string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Missing accessToken", nameof(accessToken));

        var baseUrl = (_cfg["Zalo:GraphBaseUrl"] ?? "https://graph.zalo.me").TrimEnd('/');

        var fields = "id,name,picture,oa_id,user_id_by_app,user_id_by_app,followedOA,is_sensitive";

        var url = BuildUrl(baseUrl, "/v2.0/me", new Dictionary<string, string?>
        {
            ["fields"] = fields
        });

        var headers = new Dictionary<string, string>
        {
            ["access_token"] = accessToken
        };

        var body = await SendAsync(HttpMethod.Get, url, headers, ZaloLogActions.GET_ME, null, ct);

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Nếu có field "error"
            if (root.TryGetProperty("error", out var errProp))
            {
                var errorCode = errProp.ValueKind switch
                {
                    JsonValueKind.Number => errProp.GetInt32(),
                    JsonValueKind.String when int.TryParse(errProp.GetString(), out var n) => n,
                    _ => 0
                };

                var message = root.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString()
                    : "Unknown error";

                if (errorCode != 0)
                {
                    return new ZaloMeResponse
                    {
                        Data = null,
                        Error = errorCode,
                        Message = message ?? "Error"
                    };
                }
            }

            string id = root.TryGetProperty("id", out var idEl) ? (idEl.GetString() ?? "") : "";
            string name = root.TryGetProperty("name", out var nameEl) ? (nameEl.GetString() ?? "") : "";

            string oaId = root.TryGetProperty("oa_id", out var oaIdEl) ? (oaIdEl.GetString() ?? "") : "";
            string userIdByOa = root.TryGetProperty("user_id_by_oa", out var userIdEl) ? (userIdEl.GetString() ?? "") : "";

            bool isFollower = root.TryGetProperty("is_follower", out var followerEl) && followerEl.ValueKind == JsonValueKind.True;

            bool isSensitive = false;
            if (root.TryGetProperty("is_sensitive", out var sensitiveEl))
            {
                isSensitive = sensitiveEl.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Number => sensitiveEl.GetInt32() != 0,
                    JsonValueKind.String when bool.TryParse(sensitiveEl.GetString(), out var b) => b,
                    JsonValueKind.String when int.TryParse(sensitiveEl.GetString(), out var n) => n != 0,
                    _ => false
                };
            }

            string? avatarUrl =
                root.TryGetProperty("picture", out var pic)
                && pic.ValueKind == JsonValueKind.Object
                && pic.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Object
                && data.TryGetProperty("url", out var urlEl)
                ? urlEl.GetString()
                : null;

            return new ZaloMeResponse
            {
                Data = new()
                {
                    Id = id,
                    Name = name,
                    AvatarUrl = avatarUrl,
                    OaId = oaId,
                    UserIdByOa = userIdByOa,
                    IsFollower = isFollower,
                    IsSensitive = isSensitive
                },
                Error = 0,
                Message = "Success"
            };
        }
        catch (Exception ex)
        {
            return new ZaloMeResponse
            {
                Data = null,
                Error = -1,
                Message = $"Parse error: {ex.Message}, raw: {body}"
            };
        }
    }


    public async Task<ZaloDecodePhoneResponse> DecodePhoneAsync(string code, string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Missing code", nameof(code));
        if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("Missing accessToken", nameof(accessToken));

        var appSecret = _cfg["Zalo:AppSecret"] ?? throw new ArgumentNullException("Zalo:AppSecret");

        var baseUrl = (_cfg["Zalo:GraphBaseUrl"] ?? "https://graph.zalo.me").TrimEnd('/');
        var path = _cfg["Zalo:DecodePhonePath"] ?? "/v2.0/me/info";
        if (!path.StartsWith("/")) path = "/" + path;

        var url = $"{baseUrl}{path}";

        var headers = new Dictionary<string, string>
        {
            ["access_token"] = accessToken,
            ["code"] = code,
            ["secret_key"] = appSecret
        };

        var requestLog = JsonSerializer.Serialize(new
        {
            code = SecurityHelper.MaskCode(code),
            accessToken = SecurityHelper.MaskToken(accessToken),
            secret_key = "***"
        });

        var body = await SendAsync(HttpMethod.Get, url, headers, ZaloLogActions.DECODE_PHONE, requestLog, ct);

        return SafeDeserializePhone(body) ?? new ZaloDecodePhoneResponse
        {
            Error = 0,
            Message = "Success"
        };
    }

    private static ZaloDecodePhoneResponse? SafeDeserializePhone(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<ZaloDecodePhoneResponse>(json); }
        catch { return null; }
    }
}
