using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;
public abstract class BaseZaloClient
{
    protected readonly IHttpClientFactory _factory;
    protected readonly IConfiguration _cfg;
    private readonly IRepository<ZaloLog, Guid> _logRepo;

    protected BaseZaloClient(IHttpClientFactory factory, IConfiguration cfg, IRepository<ZaloLog, Guid> logRepo)
    {
        _factory = factory;
        _cfg = cfg;
        _logRepo = logRepo;
    }

    protected async Task<string> SendRequestAsync(
        string url,
        Dictionary<string, string> headers,
        string action,
        string? requestBody,
        CancellationToken ct)
    {
        var client = _factory.CreateClient();
        var sw = Stopwatch.StartNew();
        int? httpStatus = null;
        string? body = null;
        string? err = null;

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (var kv in headers)
                req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

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
}