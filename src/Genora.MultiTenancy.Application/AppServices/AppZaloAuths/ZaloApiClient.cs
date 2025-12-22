using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public class ZaloApiClient : BaseZaloClient, IZaloApiClient
{
    private readonly IZaloTokenProvider _tokenProvider;

    public ZaloApiClient(
        IHttpClientFactory factory,
        IZaloTokenProvider tokenProvider,
        IRepository<ZaloLog, Guid> logRepo,
        IConfiguration cfg)
        : base(factory, cfg, logRepo)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task<string> SendZnsAsync(object payload, CancellationToken ct)
    {
        var endpoint = "https://business.openapi.zalo.me/message/template";
        return await PostJsonWithTokenAsync("SEND_ZNS", endpoint, payload, ct);
    }

    public async Task<string> SendOaMessageAsync(object payload, CancellationToken ct)
    {
        var endpoint = "https://openapi.zalo.me/v3.0/oa/message/cs";
        return await PostJsonWithTokenAsync("SEND_OA_MSG", endpoint, payload, ct);
    }

    private async Task<string> PostJsonWithTokenAsync(string action, string url, object payload, CancellationToken ct)
    {
        var token = await _tokenProvider.GetAccessTokenAsync();

        var finalUrl = url.Contains("?")
            ? $"{url}&access_token={Uri.EscapeDataString(token)}"
            : $"{url}?access_token={Uri.EscapeDataString(token)}";

        var json = JsonSerializer.Serialize(payload);

        var headers = new Dictionary<string, string>(); // không cần thêm vì token đã nằm trong query string

        return await SendRequestAsync(
            url: finalUrl,
            headers: headers,
            action: action,
            requestBody: json,
            ct: ct
        );
    }

    public async Task<ZaloMeResponse> GetZaloMeAsync(string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("Missing accessToken", nameof(accessToken));

        var baseUrl = (_cfg["Zalo:GraphBaseUrl"] ?? "https://graph.zalo.me").TrimEnd('/');

        var fields = "id,name,user_id_by_oa,is_sensitive,picture";
        var miniAppId = _cfg["Zalo:MiniAppId"] ?? throw new ArgumentNullException("Zalo:MiniAppId");
        var zaloMePath = _cfg["Zalo:ZaloMePath"] ?? $"/v2.0/me?fields={fields}&miniapp_id={miniAppId}";

        // dựng path với tham số động
        var path = string.Format(zaloMePath, fields, miniAppId);

        // đảm bảo path bắt đầu bằng "/"
        if (!path.StartsWith("/")) path = "/" + path;

        var url = $"{baseUrl}{path}";

        if (!path.StartsWith("/")) path = "/" + path;

        var headers = new Dictionary<string, string>
        {
            { "access_token", accessToken }
        };

        var body = await SendRequestAsync(url, headers, "GET_ME", null, ct);

        try
        {
            using var doc = JsonDocument.Parse(body);

            // Nếu có field "error"
            if (doc.RootElement.TryGetProperty("error", out var errProp))
            {
                var errorCode = errProp.GetInt32();
                var message = doc.RootElement.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString()
                    : "Unknown error";

                if (errorCode != 0)
                {
                    // Trường hợp lỗi
                    return new ZaloMeResponse
                    {
                        Data = null,
                        Error = errorCode,
                        Message = message
                    };
                }
                // Nếu error=0 thì tiếp tục parse dữ liệu bên dưới
            }

            // Parse dữ liệu thành công
            var root = doc.RootElement;

            string id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
            string name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "" : "";
            string oaId = root.TryGetProperty("oa_id", out var oaIdEl) ? oaIdEl.GetString() ?? "" : "";
            string userIdByOa = root.TryGetProperty("user_id_by_oa", out var userIdEl) ? userIdEl.GetString() ?? "" : "";

            // bool thì nên kiểm tra tồn tại trước
            bool isFollower = root.TryGetProperty("is_follower", out var followerEl) && followerEl.ValueKind == JsonValueKind.True;
            bool isSensitive = root.TryGetProperty("is_sensitive", out var sensitiveEl) && sensitiveEl.GetBoolean();

            // avatarUrl lồng nhiều cấp
            string? avatarUrl = root.TryGetProperty("picture", out var pic)
                             && pic.TryGetProperty("data", out var data)
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
            { "access_token", accessToken },
            { "code", code },
            { "secret_key", appSecret }
        };

        var requestLog = JsonSerializer.Serialize(new
        {
            code = SecurityHelper.MaskCode(code),
            accessToken = SecurityHelper.MaskToken(accessToken),
            secret_key = "***"
        });

        var body = await SendRequestAsync(url, headers, "DECODE_PHONE", requestLog, ct);

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
