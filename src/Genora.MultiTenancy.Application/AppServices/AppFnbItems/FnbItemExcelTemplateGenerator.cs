using ClosedXML.Excel;
using System;
using System.IO;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppFnbItems;
public class FnbItemExcelTemplateGenerator : ITransientDependency
{
    public IRemoteStreamContent GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("FnbItems");

        ws.Cell(1, 1).Value = "MÃ DANH MỤC (*)";
        ws.Cell(1, 2).Value = "TÊN MÓN (*)";
        ws.Cell(1, 3).Value = "GIÁ";
        ws.Cell(1, 4).Value = "IMAGE URL";
        ws.Cell(1, 5).Value = "MÔ TẢ";
        ws.Cell(1, 6).Value = "THỨ TỰ HIỂN THỊ";
        ws.Cell(1, 7).Value = "ĐƯỢC SỬ DỤNG";
        ws.Cell(1, 8).Value = "CÒN PHỤC VỤ";

        var header = ws.Range(1, 1, 1, 8);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Cell(2, 1).Value = "VD: DM001";
        ws.Cell(2, 2).Value = "VD: Cà phê sữa";
        ws.Cell(2, 3).Value = "35000";
        ws.Cell(2, 4).Value = "https://...";
        ws.Cell(2, 5).Value = "Mô tả ngắn";
        ws.Cell(2, 6).Value = "1";
        ws.Cell(2, 7).Value = "TRUE / FALSE";
        ws.Cell(2, 8).Value = "TRUE / FALSE";
        ws.Row(2).Style.Font.FontColor = XLColor.DarkGray;

        var dataRange = ws.Range(1, 1, 1000, 8);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.SheetView.FreezeRows(2);
        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            $"Template_Import_FnbItems_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }

    public IRemoteStreamContent GenerateExport(XLWorkbook workbook)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            $"Export_FnbItems_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}
