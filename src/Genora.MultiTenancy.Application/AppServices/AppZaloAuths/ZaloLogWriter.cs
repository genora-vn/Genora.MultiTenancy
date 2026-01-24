using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloLogWriter : IZaloLogWriter, ITransientDependency
{
    private readonly IRepository<ZaloLog, Guid> _logRepo;
    private readonly IGuidGenerator _guid;
    private readonly ICurrentTenant _currentTenant;

    public ZaloLogWriter(
        IRepository<ZaloLog, Guid> logRepo,
        IGuidGenerator guid,
        ICurrentTenant currentTenant)
    {
        _logRepo = logRepo;
        _guid = guid;
        _currentTenant = currentTenant;
    }

    public async Task WriteAsync(
        string action,
        string endpoint,
        int? httpStatus,
        long durationMs,
        string? requestBody,
        string? responseBody,
        string? error,
        Guid? tenantId = null)
    {
        var log = new ZaloLog(_guid.Create())
        {
            // ✅ tenantId truyền vào sẽ override; nếu không truyền thì lấy theo current tenant
            TenantId = tenantId ?? (_currentTenant.IsAvailable ? _currentTenant.Id : null),

            Action = action,
            Endpoint = endpoint,
            HttpStatus = httpStatus,
            DurationMs = durationMs,

            // ✅ thống nhất mask + truncate tại 1 nơi
            RequestBody = ZaloLogHelper.Truncate(ZaloLogHelper.MaskTokens(requestBody)),
            ResponseBody = ZaloLogHelper.Truncate(ZaloLogHelper.MaskTokens(responseBody)),
            Error = ZaloLogHelper.Truncate(error)
        };

        await _logRepo.InsertAsync(log, autoSave: true);
    }
}
