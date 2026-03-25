using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppFnbItems;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppFnbItems;
public class FnbItemExcelImporter : ITransientDependency
{
    public List<(int Row, AppFnbItemExcelRowDto Data)> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var results = new List<(int Row, AppFnbItemExcelRowDto Data)>();
        var row = 3;

        while (!ws.Cell(row, 1).IsEmpty() || !ws.Cell(row, 2).IsEmpty())
        {
            try
            {
                var dto = new AppFnbItemExcelRowDto
                {
                    CategoryCode = ws.Cell(row, 1).GetString()?.Trim(),
                    Name = ws.Cell(row, 2).GetString()?.Trim(),
                    Price = TryParseDecimal(ws.Cell(row, 3).GetString()),
                    ImageUrl = ws.Cell(row, 4).GetString()?.Trim(),
                    Description = ws.Cell(row, 5).GetString()?.Trim(),
                    SortOrder = TryParseInt(ws.Cell(row, 6).GetString()),
                    IsActive = TryParseBool(ws.Cell(row, 7).GetString()),
                    IsAvailable = TryParseBool(ws.Cell(row, 8).GetString())
                };

                results.Add((row, dto));
            }
            catch (Exception ex)
            {
                throw new BusinessException("FnbItem:ImportUnknownRowError")
                    .WithData("RowNumber", row)
                    .WithData("ExceptionMessage", ex.Message);
            }

            row++;
        }

        return results;
    }

    private static int? TryParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value.Trim(), out var result) ? result : null;
    }

    private static bool? TryParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return bool.TryParse(value.Trim(), out var result) ? result : null;
    }

    private static decimal? TryParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var raw = value.Trim().Replace(",", "");
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }
}