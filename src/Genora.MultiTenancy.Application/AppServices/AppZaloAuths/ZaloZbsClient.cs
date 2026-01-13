using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloZbsClient : BaseZaloClient, IZaloZbsClient
{
    private readonly IZaloTokenProvider _tokenProvider;

    // Allowlist OA
    private static readonly HashSet<string> OA_ALLOW = new(StringComparer.OrdinalIgnoreCase)
    {
        "/v2.0/oa/getoa",
        "/v2.0/oa/getfollowers",
        "/v2.0/oa/message",
        "/v2.0/oa/message/status",
        "/v2.0/oa/tag/tagfollower",
        "/v2.0/oa/tag/gettagsofoa",

        // v3 (version mới nhất của Zalo hiện hành)
        "/v3.0/oa/getoa",
        "/v3.0/oa/getfollowers",
        "/v3.0/oa/message/cs",
        "/v3.0/oa/message/template",
        "/v3.0/oa/tag/gettagsofoa",
    };

    // Allowlist ZNS / ZBS phone
    private static readonly HashSet<string> ZNS_ALLOW = new(StringComparer.OrdinalIgnoreCase)
    {
        "/message/template", // POST
        "/message/status",   // GET
        "/message/quota"     // GET
    };

    public ZaloZbsClient(
        IHttpClientFactory factory,
        IConfiguration cfg,
        IRepository<ZaloLog, Guid> logRepo,
        ILogger<BaseZaloClient> logger,
        IZaloTokenProvider tokenProvider)
        : base(factory, cfg, logRepo, logger)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task<string> CallAsync(ZaloZbsCallRequest req, CancellationToken ct)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));

        var api = (req.Api ?? "oa").Trim().ToLowerInvariant();
        var path = (req.Path ?? "/").Trim();
        if (!path.StartsWith("/")) path = "/" + path;

        // base url, đang hadcode điều chỉnh lấy từ AppSetting sau
        string baseUrl = api switch
        {
            "oa" => "https://openapi.zalo.me",
            "zns" => "https://business.openapi.zalo.me",
            _ => throw new BusinessException("ZaloZbs:InvalidApi").WithData("Api", api)
        };

        // allowlist validate
        if (api == "oa" && !OA_ALLOW.Contains(path))
            throw new BusinessException("ZaloZbs:PathNotAllowed").WithData("Path", path);

        if (api == "zns" && !ZNS_ALLOW.Contains(path))
            throw new BusinessException("ZaloZbs:PathNotAllowed").WithData("Path", path);

        var method = (req.Method ?? "GET").Trim().ToUpperInvariant();
        var httpMethod = method == "POST" ? HttpMethod.Post : HttpMethod.Get;

        var token = await _tokenProvider.GetAccessTokenAsync();

        var url = BuildUrl(baseUrl, path, req.Query);

        // Zalo OA/ZNS truyền token qua header
        var headers = new Dictionary<string, string>
        {
            ["access_token"] = token
        };

        string? requestBody = null;
        if (req.Body != null && httpMethod != HttpMethod.Get)
            requestBody = JsonSerializer.Serialize(req.Body);

        // Log action theo enum ZaloLogActions
        var action = api == "zns" ? ZaloLogActions.SEND_ZNS : ZaloLogActions.SEND_OA_MSG;

        var resBody = await SendAsync(httpMethod, url, headers, action, requestBody, ct);

        // Nếu token invalid => refresh + retry 1 lần
        if (IsLikelyInvalidToken(resBody))
        {
            await _tokenProvider.RefreshNowAsync();
            token = await _tokenProvider.GetAccessTokenAsync();
            headers["access_token"] = token;

            resBody = await SendAsync(httpMethod, url, headers, action, requestBody, ct);
        }

        return resBody;
    }
}