using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public class ZaloOAuthClient : IZaloOAuthClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<ZaloLog, Guid> _logRepo;

    public ZaloOAuthClient(IHttpClientFactory httpClientFactory, IRepository<ZaloLog, Guid> logRepo)
    {
        _httpClientFactory = httpClientFactory;
        _logRepo = logRepo;
    }

    public async Task<ZaloTokenResponse> ExchangeCodeAsync(
    string appId, string appSecret, string code, string codeVerifier, string redirectUri, string? oaId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = "https://oauth.zaloapp.com/v4/oa/access_token";

        var form = new Dictionary<string, string>
        {
            ["app_id"] = appId,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier,
            ["redirect_uri"] = redirectUri
        };

        // ✅ OA OAuth: bổ sung oa_id nếu có
        if (!string.IsNullOrWhiteSpace(oaId))
            form["oa_id"] = oaId;

        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(form)
        };
        req.Headers.Add("secret_key", appSecret);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int? httpStatus = null;
        string? body = null;
        string? err = null;

        try
        {
            var res = await client.SendAsync(req);
            httpStatus = (int)res.StatusCode;
            body = await res.Content.ReadAsStringAsync();

            res.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);

            // Handle khi zalo trả lỗi json
            if (doc.RootElement.TryGetProperty("error", out var e) && e.GetInt32() != 0)
            {
                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "Zalo error";
                throw new BusinessException("ZaloOAuth:ExchangeFailed").WithData("Message", msg).WithData("Body", body);
            }

            var access = doc.RootElement.GetProperty("access_token").GetString()!;
            var refresh = doc.RootElement.GetProperty("refresh_token").GetString()!;
            var expires = doc.RootElement.GetProperty("expires_in").GetInt32();

            return new ZaloTokenResponse(access, refresh, expires);
        }
        catch (Exception ex)
        {
            err = ex.ToString();
            throw;
        }
        finally
        {
            sw.Stop();

            await ZaloLogHelper.InsertLogAsync(
                _logRepo,
                action: "EXCHANGE_CODE",
                endpoint: url,
                httpStatus: httpStatus,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: JsonSerializer.Serialize(new
                {
                    app_id = appId,
                    oa_id = oaId,
                    grant_type = "authorization_code",
                    code = "***",
                    code_verifier = "***",
                    redirect_uri = redirectUri
                }),
                responseBody: body,
                error: err,
                tenantId: null
            );
        }
    }

    public async Task<ZaloTokenResponse> RefreshTokenAsync(
        string appId, string appSecret, string refreshToken, string? oaId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = "https://oauth.zaloapp.com/v4/oa/access_token";

        var form = new Dictionary<string, string>
        {
            ["app_id"] = appId,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };

        // Bắt trường hợp OA refresh cũng cần oa_id
        if (!string.IsNullOrWhiteSpace(oaId))
            form["oa_id"] = oaId;

        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(form)
        };
        req.Headers.Add("secret_key", appSecret);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        int? httpStatus = null;
        string? body = null;
        string? err = null;

        try
        {
            var res = await client.SendAsync(req);
            httpStatus = (int)res.StatusCode;
            body = await res.Content.ReadAsStringAsync();

            res.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var e) && e.GetInt32() != 0)
            {
                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "Zalo error";
                throw new BusinessException("ZaloOAuth:RefreshFailed").WithData("Message", msg).WithData("Body", body);
            }

            var access = doc.RootElement.GetProperty("access_token").GetString()!;
            var refresh = doc.RootElement.GetProperty("refresh_token").GetString()!;
            var expires = doc.RootElement.GetProperty("expires_in").GetInt32();

            return new ZaloTokenResponse(access, refresh, expires);
        }
        catch (Exception ex)
        {
            err = ex.ToString();
            throw;
        }
        finally
        {
            sw.Stop();

            await ZaloLogHelper.InsertLogAsync(
                _logRepo,
                action: "REFRESH_TOKEN",
                endpoint: url,
                httpStatus: httpStatus,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: JsonSerializer.Serialize(new
                {
                    app_id = appId,
                    oa_id = oaId,
                    grant_type = "refresh_token",
                    refresh_token = "***"
                }),
                responseBody: body,
                error: err,
                tenantId: null
            );
        }
    }
}