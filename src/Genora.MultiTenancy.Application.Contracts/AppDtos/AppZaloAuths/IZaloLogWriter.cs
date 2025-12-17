using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IZaloLogWriter
{
    Task WriteAsync(
        string action,
        string endpoint,
        int? httpStatus,
        long durationMs,
        string? requestBody,
        string? responseBody,
        string? error,
        Guid? tenantId = null
    );
}