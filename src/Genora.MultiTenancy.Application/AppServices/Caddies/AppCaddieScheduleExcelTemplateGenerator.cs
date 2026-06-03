using ClosedXML.Excel;
using System;
using System.IO;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Caddies;

public class AppCaddieScheduleExcelTemplateGenerator : ITransientDependency
{
    public IRemoteStreamContent GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("CaddieSchedules");

        ws.Cell(1, 1).Value = "MÃ CADDY (*)";
        ws.Cell(1, 2).Value = "NGÀY LÀM VIỆC (*)";
        ws.Cell(1, 3).Value = "CA LÀM VIỆC (*)";
        ws.Cell(1, 4).Value = "GIỜ BẮT ĐẦU (*)";
        ws.Cell(1, 5).Value = "GIỜ KẾT THÚC (*)";
        ws.Cell(1, 6).Value = "TRẠNG THÁI";
        ws.Cell(1, 7).Value = "CA TỐI";
        ws.Cell(1, 8).Value = "GHI CHÚ";

        var header = ws.Range(1, 1, 1, 8);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Sample row
        ws.Cell(2, 1).Value = "CD-001";
        ws.Cell(2, 2).Value = "15/06/2026";
        ws.Cell(2, 3).Value = "1 (1=Sáng, 2=Chiều, 3=Tối)";
        ws.Cell(2, 4).Value = "06:00";
        ws.Cell(2, 5).Value = "12:00";
        ws.Cell(2, 6).Value = "1 (1=Trống, 2=Phục vụ, 3=Nghỉ)";
        ws.Cell(2, 7).Value = "0 (0=Không, 1=Có)";
        ws.Cell(2, 8).Value = "Ghi chú tùy chọn";
        ws.Row(2).Style.Font.FontColor = XLColor.DarkGray;

        ws.Column(1).Width = 14;
        ws.Column(2).Width = 16;
        ws.Column(3).Width = 30;
        ws.Column(4).Width = 14;
        ws.Column(5).Width = 14;
        ws.Column(6).Width = 32;
        ws.Column(7).Width = 20;
        ws.Column(8).Width = 28;

        ws.Column(1).Style.NumberFormat.Format = "@";
        ws.Range("B3:B1000").Style.DateFormat.Format = "dd/MM/yyyy";

        ws.SheetView.FreezeRows(2);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            $"Template_Import_CaddieSchedule_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}
