using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public class ZaloApiClient : IZaloApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IZaloTokenProvider _tokenProvider;
    private readonly IRepository<ZaloLog, Guid> _logRepo;
    private readonly IConfiguration _cfg;

    public ZaloApiClient(IHttpClientFactory factory, IZaloTokenProvider tokenProvider, IRepository<ZaloLog, Guid> logRepo, IConfiguration cfg)
    {
        _factory = factory;
        _tokenProvider = tokenProvider;
        _logRepo = logRepo;
        _cfg = cfg;
    }

    public async Task<string> SendZnsAsync(object payload)
    {
        var endpoint = "https://business.openapi.zalo.me/message/template"; // Cấu hình enpoint của SendZns sau, tạm thời hardcode
        return await PostJsonWithTokenAsync("SEND_ZNS", endpoint, payload);
    }

    public async Task<string> SendOaMessageAsync(object payload)
    {
        var endpoint = "https://openapi.zalo.me/v3.0/oa/message/cs"; //  Cấu hình enpoint của MessageOa sau, tạm thời hardcode
        return await PostJsonWithTokenAsync("SEND_OA_MSG", endpoint, payload);
    }

    private async Task<string> PostJsonWithTokenAsync(string action, string url, object payload)
    {
        var token = await _tokenProvider.GetAccessTokenAsync();
        var client = _factory.CreateClient();

        var finalUrl = url.Contains("?")
            ? $"{url}&access_token={Uri.EscapeDataString(token)}"
            : $"{url}?access_token={Uri.EscapeDataString(token)}";

        var json = JsonSerializer.Serialize(payload);

        var req = new HttpRequestMessage(HttpMethod.Post, finalUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

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
            return body;
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
                action: action,
                endpoint: url,              // lưu base url (không kèm token)
                httpStatus: httpStatus,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: json,
                responseBody: body,
                error: err,
                tenantId: null
            );
        }
    }

    public async Task<ZaloMeResponse> GetMeAsync()
    {
        var token = await _tokenProvider.GetAccessTokenAsync();
        var client = _factory.CreateClient();

        var url = "https://graph.zalo.me/v2.0/me?fields=id%2Cname%2Cpicture";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("access_token", token); // Zalo Graph dùng header

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

            var id = doc.RootElement.GetProperty("id").GetString() ?? "";
            var name = doc.RootElement.GetProperty("name").GetString() ?? "";

            string? pictureUrl = null;
            if (doc.RootElement.TryGetProperty("picture", out var pic)
                && pic.TryGetProperty("data", out var data)
                && data.TryGetProperty("url", out var urlEl))
            {
                pictureUrl = urlEl.GetString();
            }

            return new ZaloMeResponse(id, name, pictureUrl);
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
                action: "GET_ME",
                endpoint: url,
                httpStatus: httpStatus,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: null,
                responseBody: body,
                error: err,
                tenantId: null
            );
        }
    }

    public async Task<ZaloPhoneResponse> DecodePhoneAsync(string code, string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Missing code", nameof(code));
        if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("Missing accessToken", nameof(accessToken));

        var appSecret = _cfg["Zalo:AppSecret"] ?? throw new ArgumentNullException("Zalo:AppSecret");

        var baseUrl = (_cfg["Zalo:GraphBaseUrl"] ?? "https://graph.zalo.me").TrimEnd('/');
        var path = _cfg["Zalo:DecodePhonePath"] ?? "/v2.0/me/info";
        if (!path.StartsWith("/")) path = "/" + path;

        var url = $"{baseUrl}{path}"; // https://graph.zalo.me/v2.0/me/info

        var client = _factory.CreateClient();

        var sw = Stopwatch.StartNew();
        int? httpStatus = null;
        string? body = null;
        string? err = null;

        // log request (mask bớt)
        var requestLog = JsonSerializer.Serialize(new
        {
            code = SecurityHelper.MaskCode(code),
            accessToken = SecurityHelper.MaskToken(accessToken),
            secret_key = "***"
        });

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Zalo yêu cầu headers
            req.Headers.TryAddWithoutValidation("access_token", accessToken);
            req.Headers.TryAddWithoutValidation("code", code);
            req.Headers.TryAddWithoutValidation("secret_key", appSecret);

            using var res = await client.SendAsync(req, ct);
            httpStatus = (int)res.StatusCode;
            body = await res.Content.ReadAsStringAsync(ct);

            // Không throw trước khi parse vì Zalo thường trả JSON error
            if (!res.IsSuccessStatusCode)
            {
                // vẫn trả body, service/controller sẽ quyết định BadRequest
                return SafeDeserializePhone(body) ?? new ZaloPhoneResponse
                {
                    Error = (int)res.StatusCode,
                    Message = body
                };
            }

            return SafeDeserializePhone(body) ?? new ZaloPhoneResponse
            {
                Error = 0,
                Message = "Success"
            };
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
                action: "DECODE_PHONE",
                endpoint: url,
                httpStatus: httpStatus,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: requestLog,
                responseBody: SecurityHelper.MaskPhoneInResponse(body),
                error: err,
                tenantId: null
            );
        }
    }

    private static ZaloPhoneResponse? SafeDeserializePhone(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<ZaloPhoneResponse>(json);
        }
        catch
        {
            return null;
        }
    }
}