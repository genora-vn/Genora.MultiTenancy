using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public class ZaloZbsCallRequest
{
    // "oa" | "zns"
    public string Api { get; set; } = "oa";

    // "GET" | "POST"
    public string Method { get; set; } = "GET";

    // ví dụ: "/v2.0/oa/getoa" hoặc "/message/template"
    public string Path { get; set; } = "/";

    public Dictionary<string, string?>? Query { get; set; }

    // object JSON (optional)
    public object? Body { get; set; }
}

public interface IZaloZbsClient
{
    Task<string> CallAsync(ZaloZbsCallRequest req, CancellationToken ct);
}