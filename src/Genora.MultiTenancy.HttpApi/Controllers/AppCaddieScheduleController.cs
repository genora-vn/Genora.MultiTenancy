using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[Route("api/app/caddie-schedule-excel")]
[Authorize]
public class AppCaddieScheduleController : AbpController
{
    private readonly CaddieScheduleAppService _scheduleService;
    private readonly AppCaddieScheduleExcelTemplateGenerator _templateGenerator;
    private readonly AppCaddieScheduleExcelImporter _importer;
    private readonly AppCaddieScheduleExcelExporter _exporter;

    public AppCaddieScheduleController(
        CaddieScheduleAppService scheduleService,
        AppCaddieScheduleExcelTemplateGenerator templateGenerator,
        AppCaddieScheduleExcelImporter importer,
        AppCaddieScheduleExcelExporter exporter)
    {
        _scheduleService = scheduleService;
        _templateGenerator = templateGenerator;
        _importer = importer;
        _exporter = exporter;
    }

    /// <summary>
    /// Tải file mẫu Excel để import lịch làm việc Caddy
    /// </summary>
    [HttpGet("download-template")]
    [DisableValidation]
    public IRemoteStreamContent DownloadTemplate()
    {
        return _templateGenerator.GenerateTemplate();
    }

    /// <summary>
    /// Upload file Excel để import lịch làm việc Caddy hàng loạt
    /// </summary>
    [HttpPost("upload")]
    [DisableValidation]
    public async Task<ImportCaddieScheduleResultDto> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("Vui lòng chọn file Excel.");

        var ext = System.IO.Path.GetExtension(file.FileName)?.ToLower();
        if (ext != ".xlsx" && ext != ".xls")
            throw new UserFriendlyException("Chỉ hỗ trợ file .xlsx hoặc .xls");

        using var stream = file.OpenReadStream();
        var dtos = await _importer.ParseAsync(stream);

        int successCount = 0;
        var errors = new List<string>();

        foreach (var dto in dtos)
        {
            try
            {
                await _scheduleService.CreateAsync(dto);
                successCount++;
            }
            catch (System.Exception ex)
            {
                errors.Add($"Caddy {dto.CaddieId} ngày {dto.WorkDate:dd/MM/yyyy}: {ex.Message}");
            }
        }

        return new ImportCaddieScheduleResultDto
        {
            TotalRows = dtos.Count,
            SuccessCount = successCount,
            ErrorCount = errors.Count,
            Errors = errors.Take(20).ToList()
        };
    }

    /// <summary>
    /// Export toàn bộ lịch làm việc Caddy ra file Excel
    /// </summary>
    [HttpGet("export")]
    [DisableValidation]
    public async Task<IRemoteStreamContent> Export([FromQuery] System.DateTime? fromDate, [FromQuery] System.DateTime? toDate)
    {
        var result = await _scheduleService.GetListAsync(new GetCaddieScheduleListInput
        {
            FromDate = fromDate,
            ToDate = toDate,
            MaxResultCount = 10000
        });

        return _exporter.Export(result.Items.ToList());
    }

    /// <summary>
    /// Xóa lịch làm việc theo khoảng ngày/giờ. Bỏ qua ca đã có booking.
    /// </summary>
    [HttpPost("delete-range")]
    public async Task<DeleteCaddieScheduleRangeResultDto> DeleteRange([FromBody] DeleteCaddieScheduleRangeInput input)
    {
        return await _scheduleService.DeleteRangeAsync(input);
    }
}

public class ImportCaddieScheduleResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
