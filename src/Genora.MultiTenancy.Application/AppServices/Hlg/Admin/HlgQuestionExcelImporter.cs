using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Hlg.Admin;

public class HlgQuestionExcelImporter : ITransientDependency
{
    public List<HlgQuestionExcelRow> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 2;
        var rows = new List<HlgQuestionExcelRow>();

        for (var rowNumber = 3; rowNumber <= lastRow; rowNumber++)
        {
            var values = Enumerable.Range(1, 11)
                .Select(column => worksheet.Cell(rowNumber, column).GetString().Trim())
                .ToArray();

            if (values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            rows.Add(new HlgQuestionExcelRow(
                rowNumber,
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5],
                values[6],
                values[7],
                values[8],
                values[9],
                values[10]));
        }

        return rows;
    }
}

public record HlgQuestionExcelRow(
    int RowNumber,
    string Index,
    string Content,
    string ImageUrl,
    string TimeLimitSec,
    string ScoreMultiplier,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectKey,
    string IsActive);
