using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using Genora.MultiTenancy.AppDtos.Caddies;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Caddies;

public class AppCaddieScheduleExcelExporter : ITransientDependency
{
    public IRemoteStreamContent Export(List<CaddieScheduleDto> items)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("CaddieSchedules");

        // Headers
        ws.Cell(1, 1).Value = "Mã Caddy";
        ws.Cell(1, 2).Value = "Tên Caddy";
        ws.Cell(1, 3).Value = "Ngày làm việc";
        ws.Cell(1, 4).Value = "Ca làm việc";
        ws.Cell(1, 5).Value = "Giờ bắt đầu";
        ws.Cell(1, 6).Value = "Giờ kết thúc";
        ws.Cell(1, 7).Value = "Trạng thái";
        ws.Cell(1, 8).Value = "Ca tối";
        ws.Cell(1, 9).Value = "Ghi chú";

        var header = ws.Range(1, 1, 1, 9);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Data rows
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = item.CaddieCode ?? "";
            ws.Cell(row, 2).Value = item.CaddieName ?? "";
            ws.Cell(row, 3).Value = item.WorkDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 4).Value = item.ShiftCodeText ?? "";
            ws.Cell(row, 5).Value = item.StartTime.ToString(@"hh\:mm");
            ws.Cell(row, 6).Value = item.EndTime.ToString(@"hh\:mm");
            ws.Cell(row, 7).Value = item.SlotStatusText ?? "";
            ws.Cell(row, 8).Value = item.IsNightShift ? "Có" : "Không";
            ws.Cell(row, 9).Value = item.Note ?? "";
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            $"CaddieSchedule_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}
