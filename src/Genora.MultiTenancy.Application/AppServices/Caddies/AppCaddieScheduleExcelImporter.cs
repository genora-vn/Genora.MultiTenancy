using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.Caddies;

public class AppCaddieScheduleExcelImporter : ITransientDependency
{
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;

    public AppCaddieScheduleExcelImporter(IRepository<AppCaddie, Guid> caddieRepo)
    {
        _caddieRepo = caddieRepo;
    }

    public async Task<List<CreateUpdateCaddieScheduleDto>> ParseAsync(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheets.FirstOrDefault()
            ?? throw new UserFriendlyException("File Excel không có sheet nào.");

        var results = new List<CreateUpdateCaddieScheduleDto>();
        var errors = new List<string>();

        // Load caddie lookup by code
        var caddieQuery = await _caddieRepo.GetQueryableAsync();
        var caddies = caddieQuery.Select(c => new { c.Id, c.CaddieCode }).ToList();
        var caddieDict = caddies.ToDictionary(c => c.CaddieCode.ToUpper(), c => c.Id);

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 2;

        for (int row = 3; row <= lastRow; row++) // Start from row 3 (skip header + sample)
        {
            var caddieCode = ws.Cell(row, 1).GetString()?.Trim();
            var workDateStr = ws.Cell(row, 2).GetString()?.Trim();
            var shiftCodeStr = ws.Cell(row, 3).GetString()?.Trim();
            var startTimeStr = ws.Cell(row, 4).GetString()?.Trim();
            var endTimeStr = ws.Cell(row, 5).GetString()?.Trim();
            var statusStr = ws.Cell(row, 6).GetString()?.Trim();
            var nightShiftStr = ws.Cell(row, 7).GetString()?.Trim();
            var note = ws.Cell(row, 8).GetString()?.Trim();

            // Skip empty rows
            if (string.IsNullOrWhiteSpace(caddieCode) && string.IsNullOrWhiteSpace(workDateStr))
                continue;

            // Validate CaddieCode
            if (string.IsNullOrWhiteSpace(caddieCode))
            {
                errors.Add($"Dòng {row}: Thiếu Mã Caddy");
                continue;
            }

            if (!caddieDict.TryGetValue(caddieCode.ToUpper(), out var caddieId))
            {
                errors.Add($"Dòng {row}: Mã Caddy '{caddieCode}' không tồn tại");
                continue;
            }

            // Parse WorkDate
            if (!DateTime.TryParseExact(workDateStr, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd/MM/yyyy HH:mm:ss", "d/M/yyyy HH:mm:ss", "M/d/yyyy", "M/d/yyyy HH:mm:ss" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var workDate))
            {
                // Try parsing as Excel date number or general DateTime
                if (double.TryParse(workDateStr, out var excelDate))
                    workDate = DateTime.FromOADate(excelDate);
                else if (DateTime.TryParse(workDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    workDate = parsedDate;
                else if (DateTime.TryParse(workDateStr, new CultureInfo("vi-VN"), DateTimeStyles.None, out var parsedDateVi))
                    workDate = parsedDateVi;
                else
                {
                    errors.Add($"Dòng {row}: Ngày làm việc '{workDateStr}' không hợp lệ");
                    continue;
                }
            }

            // Parse ShiftCode (take first digit)
            var shiftDigit = shiftCodeStr?.FirstOrDefault(char.IsDigit);
            byte shiftCode = shiftDigit.HasValue && byte.TryParse(shiftDigit.Value.ToString(), out var sc) ? sc : (byte)1;

            // Parse StartTime / EndTime
            if (!TimeSpan.TryParse(startTimeStr, out var startTime))
            {
                errors.Add($"Dòng {row}: Giờ bắt đầu '{startTimeStr}' không hợp lệ (HH:mm)");
                continue;
            }
            if (!TimeSpan.TryParse(endTimeStr, out var endTime))
            {
                errors.Add($"Dòng {row}: Giờ kết thúc '{endTimeStr}' không hợp lệ (HH:mm)");
                continue;
            }

            // Parse status (take first digit, default 1)
            var statusDigit = statusStr?.FirstOrDefault(char.IsDigit);
            byte slotStatus = statusDigit.HasValue && byte.TryParse(statusDigit.Value.ToString(), out var ss) ? ss : (byte)1;

            // Parse night shift
            bool isNightShift = nightShiftStr == "1" || nightShiftStr?.ToLower() == "true" || nightShiftStr?.ToLower() == "có";

            results.Add(new CreateUpdateCaddieScheduleDto
            {
                CaddieId = caddieId,
                WorkDate = workDate,
                ShiftCode = shiftCode,
                StartTime = startTime,
                EndTime = endTime,
                SlotStatus = slotStatus,
                IsNightShift = isNightShift,
                Note = note
            });
        }

        if (errors.Any())
            throw new UserFriendlyException(string.Join("\n", errors));

        return results;
    }
}
