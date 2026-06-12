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
    private readonly CaddieBookingAppService _bookingService;
    private readonly AppCaddieScheduleExcelTemplateGenerator _templateGenerator;
    private readonly AppCaddieScheduleExcelImporter _importer;
    private readonly AppCaddieScheduleExcelExporter _exporter;
    private readonly AppCaddieBookingExcelExporter _bookingExporter;

    public AppCaddieScheduleController(
        CaddieScheduleAppService scheduleService,
        CaddieBookingAppService bookingService,
        AppCaddieScheduleExcelTemplateGenerator templateGenerator,
        AppCaddieScheduleExcelImporter importer,
        AppCaddieScheduleExcelExporter exporter,
        AppCaddieBookingExcelExporter bookingExporter)
    {
        _scheduleService = scheduleService;
        _bookingService = bookingService;
        _templateGenerator = templateGenerator;
        _importer = importer;
        _exporter = exporter;
        _bookingExporter = bookingExporter;
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
    /// Upload file Excel để preview trước khi import (không lưu DB)
    /// </summary>
    [HttpPost("preview")]
    [DisableValidation]
    public async Task<PreviewCaddieScheduleResultDto> Preview(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("Vui lòng chọn file Excel.");

        var ext = System.IO.Path.GetExtension(file.FileName)?.ToLower();
        if (ext != ".xlsx" && ext != ".xls")
            throw new UserFriendlyException("Chỉ hỗ trợ file .xlsx hoặc .xls");

        using var stream = file.OpenReadStream();
        var dtos = await _importer.ParseAsync(stream);

        return new PreviewCaddieScheduleResultDto
        {
            TotalRows = dtos.Count,
            Items = dtos.Select(d => new PreviewCaddieScheduleItemDto
            {
                CaddieId = d.CaddieId,
                WorkDate = d.WorkDate,
                ShiftCode = d.ShiftCode,
                ShiftCodeText = d.ShiftCode switch { 1 => "Sáng", 2 => "Chiều", 3 => "Tối", _ => "Khác" },
                StartTime = d.StartTime.ToString(@"hh\:mm"),
                EndTime = d.EndTime.ToString(@"hh\:mm"),
                SlotStatus = d.SlotStatus,
                SlotStatusText = d.SlotStatus switch { 1 => "Trống lịch", 2 => "Đang phục vụ", 3 => "Nghỉ", _ => "Khác" },
                IsNightShift = d.IsNightShift,
                Note = d.Note
            }).ToList()
        };
    }

    /// <summary>
    /// Confirm import sau khi preview — lưu vào DB
    /// </summary>
    [HttpPost("confirm-import")]
    [DisableValidation]
    public async Task<ImportCaddieScheduleResultDto> ConfirmImport(IFormFile file)
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
                errors.Add($"Caddy {dto.CaddieId} ngày {dto.WorkDate:dd/MM/yyyy} {dto.StartTime:hh\\:mm}: {ex.Message}");
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
    /// Upload file Excel để import lịch làm việc Caddy hàng loạt (backward compat)
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

    // ── Schedule Template ─────────────────────────────────────────────

    /// <summary>
    /// Lưu lịch tuần hiện tại làm template cho caddie
    /// </summary>
    [HttpPost("save-template")]
    public async Task<List<CaddieScheduleTemplateDto>> SaveTemplate([FromBody] SaveScheduleTemplateInput input)
    {
        return await _scheduleService.SaveTemplateAsync(input);
    }

    /// <summary>
    /// Apply template vào tuần target (auto-generate lịch)
    /// </summary>
    [HttpPost("apply-template")]
    public async Task<ApplyScheduleTemplateResultDto> ApplyTemplate([FromBody] ApplyScheduleTemplateInput input)
    {
        return await _scheduleService.ApplyTemplateAsync(input);
    }

    /// <summary>
    /// Get template list cho caddie
    /// </summary>
    [HttpGet("templates")]
    public async Task<List<CaddieScheduleTemplateDto>> GetTemplates([FromQuery] System.Guid caddieId)
    {
        return await _scheduleService.GetTemplatesAsync(caddieId);
    }

    // ── Booking Export ─────────────────────────────────────────────────

    /// <summary>
    /// Export danh sách booking caddie ra Excel cho accounting
    /// </summary>
    [HttpGet("export-bookings")]
    [DisableValidation]
    public async Task<IRemoteStreamContent> ExportBookings([FromQuery] System.DateTime? fromDate, [FromQuery] System.DateTime? toDate)
    {
        var result = await _bookingService.GetListAsync(new GetCaddieBookingListInput
        {
            FromDate = fromDate,
            ToDate = toDate,
            MaxResultCount = 10000
        });

        return _bookingExporter.Export(result.Items.ToList());
    }
}

public class ImportCaddieScheduleResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PreviewCaddieScheduleResultDto
{
    public int TotalRows { get; set; }
    public List<PreviewCaddieScheduleItemDto> Items { get; set; } = new();
}

public class PreviewCaddieScheduleItemDto
{
    public System.Guid CaddieId { get; set; }
    public System.DateTime WorkDate { get; set; }
    public byte ShiftCode { get; set; }
    public string? ShiftCodeText { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public byte SlotStatus { get; set; }
    public string? SlotStatusText { get; set; }
    public bool IsNightShift { get; set; }
    public string? Note { get; set; }
}
