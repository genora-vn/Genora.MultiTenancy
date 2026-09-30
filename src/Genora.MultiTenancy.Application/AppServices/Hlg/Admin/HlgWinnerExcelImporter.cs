using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Hlg.Admin;

public class HlgWinnerExcelImporter : ITransientDependency
{
    public List<HlgWinnerExcelRow> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 2;
        var rows = new List<HlgWinnerExcelRow>();

        for (var rowNumber = 3; rowNumber <= lastRow; rowNumber++)
        {
            var values = Enumerable.Range(1, 4)
                .Select(column => worksheet.Cell(rowNumber, column).GetString().Trim())
                .ToArray();

            if (values.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(new HlgWinnerExcelRow(rowNumber, values[0], values[1], values[2], values[3]));
        }

        return rows;
    }
}

public record HlgWinnerExcelRow(int RowNumber, string EventId, string PrizeId, string CustomerPhone, string IsActive);
