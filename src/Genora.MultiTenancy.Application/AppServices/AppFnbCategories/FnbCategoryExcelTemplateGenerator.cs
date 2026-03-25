using ClosedXML.Excel;
using System;
using System.IO;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppFnbCategories;
public class FnbCategoryExcelTemplateGenerator : ITransientDependency
{
    public IRemoteStreamContent GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("FnbCategories");

        ws.Cell(1, 1).Value = "MÃ DANH MỤC";
        ws.Cell(1, 2).Value = "TÊN DANH MỤC (*)";
        ws.Cell(1, 3).Value = "THỨ TỰ HIỂN THỊ";
        ws.Cell(1, 4).Value = "ĐƯỢC SỬ DỤNG";

        var header = ws.Range(1, 1, 1, 4);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Cell(2, 1).Value = "VD: DM001";
        ws.Cell(2, 2).Value = "VD: Đồ uống";
        ws.Cell(2, 3).Value = "VD: 1";
        ws.Cell(2, 4).Value = "TRUE / FALSE";
        ws.Row(2).Style.Font.FontColor = XLColor.DarkGray;

        ws.Column(1).Width = 20;
        ws.Column(2).Width = 28;
        ws.Column(3).Width = 18;
        ws.Column(4).Width = 18;

        var dataRange = ws.Range(1, 1, 1000, 4);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.SheetView.FreezeRows(2);
        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            $"Template_Import_FnbCategories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
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
            $"Export_FnbCategories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}