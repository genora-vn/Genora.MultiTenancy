using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloOAuthClient : IZaloOAuthClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IZaloLogWriter _logWriter;

    public ZaloOAuthClient(IHttpClientFactory httpClientFactory, IZaloLogWriter logWriter)
    {
        _httpClientFactory = httpClientFactory;
        _logWriter = logWriter;
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

            if (doc.RootElement.TryGetProperty("error", out var e) &&
                e.ValueKind == JsonValueKind.Number &&
                e.GetInt32() != 0)
            {
                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "Zalo error";
                throw new BusinessException("ZaloOAuth:ExchangeFailed")
                    .WithData("Message", msg)
                    .WithData("Body", body);
            }

            var access = doc.RootElement.GetProperty("access_token").GetString()!;
            var refresh = doc.RootElement.GetProperty("refresh_token").GetString()!;

            var expires = JsonHelper.ReadLongFlexible(doc.RootElement, "expires_in", 0);

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

            await _logWriter.WriteAsync(
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
            var errCode = JsonHelper.ReadLongFlexible(doc.RootElement, "error", 0);
            if (errCode != 0)
            {
                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "Zalo error";
                throw new BusinessException("ZaloOAuth:RefreshFailed")
                    .WithData("Message", msg)
                    .WithData("Body", body);
            }

            var access = doc.RootElement.GetProperty("access_token").GetString()!;
            var refresh = doc.RootElement.GetProperty("refresh_token").GetString()!;
            var expires = JsonHelper.ReadLongFlexible(doc.RootElement, "expires_in", 0);

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

            await _logWriter.WriteAsync(
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
