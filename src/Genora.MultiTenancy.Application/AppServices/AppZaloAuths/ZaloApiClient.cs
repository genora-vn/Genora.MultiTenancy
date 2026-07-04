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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloApiClient : BaseZaloClient, IZaloApiClient
{
    private readonly IZaloTokenProvider _tokenProvider;
    private readonly IZaloRuntimeConfigProvider _zaloCfg;
    private readonly ILogger<BaseZaloClient> _apiLogger;

    public ZaloApiClient(
        IHttpClientFactory factory,
        IZaloTokenProvider tokenProvider,
        IZaloLogWriter logWriter,
        IConfiguration cfg,
        ILogger<BaseZaloClient> logger,
        IZaloRuntimeConfigProvider zaloCfg)
        : base(factory, cfg, logWriter, logger)
    {
        _tokenProvider = tokenProvider;
        _zaloCfg = zaloCfg;
        _apiLogger = logger;
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

    #region Articles (Zalo OA bài viết)

    public async Task<ZaloArticleListResponse> GetArticleListAsync(int offset, int limit, string type, CancellationToken ct)
    {
        var baseUrl = (_cfg["Zalo:OpenApiBaseUrl"] ?? "https://openapi.zalo.me").TrimEnd('/');
        var url = BuildUrl(baseUrl, "/v2.0/article/getslice", new Dictionary<string, string?>
        {
            ["offset"] = offset.ToString(),
            ["limit"] = limit.ToString(),
            ["type"] = string.IsNullOrWhiteSpace(type) ? "normal" : type
        });

        var body = await SendWithAccessTokenGetAsync(url, ZaloLogActions.GET_ARTICLE_LIST, ct);

        return SafeDeserialize<ZaloArticleListResponse>(body)
            ?? new ZaloArticleListResponse { Error = -1, Message = $"Parse error, raw: {body}" };
    }

    public async Task<ZaloArticleDetailResponse> GetArticleDetailAsync(string articleId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            throw new ArgumentException("Missing articleId", nameof(articleId));

        var baseUrl = (_cfg["Zalo:OpenApiBaseUrl"] ?? "https://openapi.zalo.me").TrimEnd('/');
        var url = BuildUrl(baseUrl, "/v2.0/article/getdetail", new Dictionary<string, string?>
        {
            ["id"] = articleId
        });

        var body = await SendWithAccessTokenGetAsync(url, ZaloLogActions.GET_ARTICLE_DETAIL, ct);

        return SafeDeserialize<ZaloArticleDetailResponse>(body)
            ?? new ZaloArticleDetailResponse { Error = -1, Message = $"Parse error, raw: {body}" };
    }

    /// <summary>
    /// GET có header access_token. Lấy token từ ZaloAuth active (theo tenant);
    /// nếu không có/lỗi (vd Host chưa cấp quyền) fallback sang config Zalo:TestAccessToken.
    /// Nếu token có dấu hiệu hết hạn/không hợp lệ → refresh 1 lần rồi thử lại.
    /// </summary>
    private async Task<string> SendWithAccessTokenGetAsync(string url, string action, CancellationToken ct)
    {
        var token = await ResolveAccessTokenAsync();

        var headers = new Dictionary<string, string> { ["access_token"] = token };
        var body = await SendAsync(HttpMethod.Get, url, headers, action, null, ct);

        if (IsLikelyInvalidToken(body))
        {
            try
            {
                await _tokenProvider.RefreshNowAsync();
                var token2 = await _tokenProvider.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token2))
                {
                    headers["access_token"] = token2;
                    body = await SendAsync(HttpMethod.Get, url, headers, action, null, ct);
                }
            }
            catch (Exception ex)
            {
                _apiLogger.LogWarning(ex, "Zalo article: refresh token failed, giữ nguyên response gốc");
            }
        }

        return body;
    }

    /// <summary>
    /// Lấy access token từ DB (ZaloAuth active theo tenant). Nếu lỗi hoặc rỗng
    /// (Host chưa cấp quyền), fallback sang config Zalo:TestAccessToken để test.
    /// </summary>
    private async Task<string> ResolveAccessTokenAsync()
    {
        try
        {
            var token = await _tokenProvider.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                return token;
        }
        catch (Exception ex)
        {
            _apiLogger.LogWarning(ex, "Zalo article: không lấy được token từ ZaloAuth, thử fallback test token");
        }

        var testToken = _cfg["Zalo:TestAccessToken"];
        if (!string.IsNullOrWhiteSpace(testToken))
            return testToken;

        throw new InvalidOperationException("Không có access token Zalo (ZaloAuth chưa cấu hình và thiếu Zalo:TestAccessToken)");
    }

    private static T? SafeDeserialize<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<T>(json, _zaloJsonOptions); }
        catch { return null; }
    }

    #endregion

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

        var config = await _zaloCfg.GetAsync();
        var appSecret = config.AppSecret;

        var baseUrl = (_cfg["Zalo:GraphBaseUrl"] ?? "https://graph.zalo.me").TrimEnd('/');
        var path = _cfg["Zalo:ZaloDecodePath"] ?? "/v2.0/me/info";
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

        return SafeDeserializePhoneNumber(body) ?? new ZaloDecodePhoneResponse
        {
            Error = 0,
            Message = "Success"
        };
    }

    public async Task<ZaloDecodeLocationResponse> DecodeLocationAsync(string code, string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Missing code", nameof(code));
        if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("Missing accessToken", nameof(accessToken));

        var config = await _zaloCfg.GetAsync();
        var appSecret = config.AppSecret;

        var baseUrl = (_cfg["Zalo:GraphBaseUrl"] ?? "https://graph.zalo.me").TrimEnd('/');
        // Zalo hướng dẫn decode location dùng đúng /v2.0/me/info như decode phone
        var path = _cfg["Zalo:ZaloDecodePath"] ?? "/v2.0/me/info";
        if (!path.StartsWith("/")) path = "/" + path;

        var url = $"{baseUrl}{path}";

        var headers = new Dictionary<string, string>
        {
            ["access_token"] = accessToken,
            ["code"] = code,          // token từ getLocation()
            ["secret_key"] = appSecret
        };

        var requestLog = JsonSerializer.Serialize(new
        {
            code = SecurityHelper.MaskCode(code),
            accessToken = SecurityHelper.MaskToken(accessToken),
            secret_key = "***"
        });

        // ✅ Dùng cùng SendAsync để log + retry (nếu bạn có)
        var body = await SendAsync(HttpMethod.Get, url, headers, "DECODE_LOCATION", requestLog, ct);

        // ✅ Parse lat/lon từ data
        var res = SafeDeserializeLocation(body);

        // fallback mềm (không crash)
        return res ?? new ZaloDecodeLocationResponse
        {
            Error = 0,
            Message = "Success",
            Data = null
        };
    }

    private static ZaloDecodePhoneResponse? SafeDeserializePhoneNumber(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<ZaloDecodePhoneResponse>(json); }
        catch { return null; }
    }
    private static readonly JsonSerializerOptions _zaloJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private static ZaloDecodeLocationResponse? SafeDeserializeLocation(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<ZaloDecodeLocationResponse>(json, _zaloJsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
