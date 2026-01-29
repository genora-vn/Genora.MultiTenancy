using ClosedXML.Excel;
using System;
using System.IO;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppCustomers;

public class AppCustomerExcelTemplateGenerator : ITransientDependency
{
    public IRemoteStreamContent GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Customers");

        ws.Cell(1, 1).Value = "TÊN HỘI VIÊN (*)";
        ws.Cell(1, 2).Value = "MÃ HỘI VIÊN";
        ws.Cell(1, 3).Value = "NGÀY THÁNG NĂM SINH";
        ws.Cell(1, 4).Value = "SỐ ĐIỆN THOẠI (*)";
        ws.Cell(1, 5).Value = "EMAIL";

        var header = ws.Range(1, 1, 1, 5);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Cell(2, 1).Value = "VD: Nguyễn Văn A";
        ws.Cell(2, 2).Value = "VD: KH000001 (có thể để trống)";
        ws.Cell(2, 3).Value = "dd/MM/yyyy";
        ws.Cell(2, 4).Value = "'0901234567";
        ws.Cell(2, 5).Value = "VD: a@gmail.com";
        ws.Row(2).Style.Font.FontColor = XLColor.DarkGray;

        ws.Column(1).Width = 28;
        ws.Column(2).Width = 18;
        ws.Column(3).Width = 18;
        ws.Column(4).Width = 20;
        ws.Column(5).Width = 26;

        ws.Range("C3:C1000").Style.DateFormat.Format = "dd/MM/yyyy";

        ws.Column(4).Style.NumberFormat.Format = "@";
        ws.Range("D3:D1000").Style.NumberFormat.Format = "@";

        var dataRange = ws.Range(1, 1, 1000, 5);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.SheetView.FreezeRows(2);

        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            $"Template_Import_Customers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}
