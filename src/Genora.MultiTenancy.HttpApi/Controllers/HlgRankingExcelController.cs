using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/hlg-ranking-excel")]
public class HlgRankingExcelController : AbpController
{
    private readonly IHlgRankingAdminAppService _service;

    public HlgRankingExcelController(IHlgRankingAdminAppService service)
    {
        _service = service;
    }

    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] Guid eventId)
    {
        return _service.ExportResultsAsync(eventId);
    }

    /// <summary>Xuất BÁO CÁO (A): chọn 1 hoặc nhiều game đã kết thúc; không reset điểm, không đóng băng snapshot.
    /// gameIds: danh sách Guid phân tách bằng dấu phẩy (rỗng = toàn bộ game đã kết thúc).</summary>
    [HttpGet("export-report")]
    [DisableValidation]
    public Task<IRemoteStreamContent> ExportReport([FromQuery] Guid eventId, [FromQuery] string? gameIds)
    {
        var ids = (gameIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();
        return _service.ExportReportAsync(eventId, ids);
    }
}
