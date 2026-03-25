using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using System;
using System.Collections.Generic;
using System.IO;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppFnbCategories;
public class FnbCategoryExcelImporter : ITransientDependency
{
    public List<(int Row, AppFnbCategoryExcelRowDto Data)> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var results = new List<(int Row, AppFnbCategoryExcelRowDto Data)>();
        var row = 3;

        while (!ws.Cell(row, 1).IsEmpty() || !ws.Cell(row, 2).IsEmpty())
        {
            try
            {
                var dto = new AppFnbCategoryExcelRowDto
                {
                    Code = ws.Cell(row, 1).GetString()?.Trim(),
                    Name = ws.Cell(row, 2).GetString()?.Trim(),
                    SortOrder = TryParseInt(ws.Cell(row, 3).GetString()),
                    IsActive = TryParseBool(ws.Cell(row, 4).GetString())
                };

                results.Add((row, dto));
            }
            catch (Exception ex)
            {
                throw new BusinessException("FnbCategory:ImportUnknownRowError")
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
}
