using ClosedXML.Excel;
using System;
using System.IO;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Hlg.Admin;

public class HlgQuestionExcelTemplateGenerator : ITransientDependency
{
    public IRemoteStreamContent Generate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Questions");
        var headers = new[]
        {
            "THỨ TỰ (*)", "NỘI DUNG CÂU HỎI (*)", "IMAGE URL", "THỜI GIAN (GIÂY)",
            "HỆ SỐ ĐIỂM", "ĐÁP ÁN A (*)", "ĐÁP ÁN B (*)", "ĐÁP ÁN C", "ĐÁP ÁN D",
            "ĐÁP ÁN ĐÚNG (*)", "ĐƯỢC SỬ DỤNG"
        };
        var examples = new[]
        {
            "VD: 1", "VD: Nội dung câu hỏi", "", "VD: 30", "VD: 1", "Đáp án A",
            "Đáp án B", "Đáp án C", "Đáp án D", "A / B / C / D", "TRUE / FALSE"
        };

        for (var column = 1; column <= headers.Length; column++)
        {
            worksheet.Cell(1, column).Value = headers[column - 1];
            worksheet.Cell(2, column).Value = examples[column - 1];
        }

        var header = worksheet.Range(1, 1, 1, headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Row(2).Style.Font.FontColor = XLColor.DarkGray;

        worksheet.Column(1).Width = 14;
        worksheet.Column(2).Width = 45;
        worksheet.Column(3).Width = 30;
        worksheet.Columns(4, 5).Width = 18;
        worksheet.Columns(6, 9).Width = 28;
        worksheet.Columns(10, 11).Width = 20;
        worksheet.Range(1, 1, 1000, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range(1, 1, 1000, headers.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.SheetView.FreezeRows(2);

        var correctKeyValidation = worksheet.Range("J3:J1000").CreateDataValidation();
        correctKeyValidation.List("A,B,C,D", true);
        var activeValidation = worksheet.Range("K3:K1000").CreateDataValidation();
        activeValidation.List("TRUE,FALSE", true);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return new RemoteStreamContent(
            stream,
            $"Template_Import_HlgQuestions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
