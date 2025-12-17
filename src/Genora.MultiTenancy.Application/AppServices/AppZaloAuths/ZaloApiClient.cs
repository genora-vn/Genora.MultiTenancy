using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public class ZaloApiClient : IZaloApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IZaloTokenProvider _tokenProvider;
    private readonly IRepository<ZaloLog, Guid> _logRepo;

    public ZaloApiClient(IHttpClientFactory factory, IZaloTokenProvider tokenProvider, IRepository<ZaloLog, Guid> logRepo)
    {
        _factory = factory;
        _tokenProvider = tokenProvider;
        _logRepo = logRepo;
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
}
