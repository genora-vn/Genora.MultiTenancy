using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public abstract class BaseZaloClient
{
    protected readonly IHttpClientFactory _factory;
    protected readonly IConfiguration _cfg;
    private readonly IRepository<ZaloLog, Guid> _logRepo;
    private readonly ILogger<BaseZaloClient> _logger;

    protected BaseZaloClient(
        IHttpClientFactory factory,
        IConfiguration cfg,
        IRepository<ZaloLog, Guid> logRepo,
        ILogger<BaseZaloClient> logger)
    {
        _factory = factory;
        _cfg = cfg;
        _logRepo = logRepo;
        _logger = logger;
    }

    // Function chuẩn query-string key=value&key=value (không dùng q=)
    protected static string BuildUrl(string baseUrl, string path, IDictionary<string, string?>? query = null)
    {
        baseUrl = (baseUrl ?? "").TrimEnd('/');
        path = string.IsNullOrWhiteSpace(path) ? "/" : (path.StartsWith("/") ? path : "/" + path);

        var sb = new StringBuilder();
        sb.Append(baseUrl).Append(path);

        if (query != null)
        {
            var first = true;
            foreach (var kv in query)
            {
                if (kv.Value == null) continue;

                sb.Append(first ? "?" : "&");
                sb.Append(Uri.EscapeDataString(kv.Key));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(kv.Value));
                first = false;
            }
        }

        return sb.ToString();
    }

    protected async Task<string> SendAsync(
        HttpMethod method,
        string url,
        Dictionary<string, string> headers,
        string action,
        string? requestBody,
        CancellationToken ct,
        string? contentType = "application/json")
    {
        var client = _factory.CreateClient();
        var sw = Stopwatch.StartNew();
        int? httpStatus = null;
        string? body = null;
        string? err = null;

        try
        {
            using var req = new HttpRequestMessage(method, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (var kv in headers)
                req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

            if (!string.IsNullOrWhiteSpace(requestBody) && method != HttpMethod.Get)
            {
                req.Content = new StringContent(requestBody, Encoding.UTF8, contentType ?? "application/json");
            }

            using var res = await client.SendAsync(req, ct);
            httpStatus = (int)res.StatusCode;
            body = await res.Content.ReadAsStringAsync(ct);

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

            // Seq (Serilog) sẽ nhận log từ ILogger
            _logger.LogInformation("Zalo {Action} {Method} {Url} -> {Status} in {Elapsed}ms",
                action, method.Method, ZaloLogHelper.MaskTokens(url), httpStatus, sw.ElapsedMilliseconds);

            await ZaloLogHelper.InsertLogAsync(
                _logRepo,
                action: action,
                endpoint: url,
                httpStatus: httpStatus,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: requestBody,
                responseBody: body,
                error: err,
                tenantId: null
            );
        }
    }

    protected static bool IsLikelyInvalidToken(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return false;

        // Message chứa "token" + "không đúng/invalid/expired"
        var s = responseBody.ToLowerInvariant();
        if (!s.Contains("token")) return false;

        return s.Contains("không đúng")
            || s.Contains("invalid")
            || s.Contains("expired")
            || s.Contains("hết hạn")
            || s.Contains("vui lòng lấy token mới");
    }
}