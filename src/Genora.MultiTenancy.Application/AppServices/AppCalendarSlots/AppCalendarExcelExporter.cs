using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using System.Collections.Generic;
using System.IO;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots
{
    public class AppCalendarExcelExporter
    {
        public IRemoteStreamContent Export(List<AppCalendarSlotExcelRowDto> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("CalendarSlot.xlsx");

            // Header
            ws.Cell(1, 1).Value = "GolfCourseName";
            ws.Cell(1, 2).Value = "PlayDate";
            ws.Cell(1, 3).Value = "StartTime";
            ws.Cell(1, 4).Value = "EndTime";
            ws.Cell(1, 5).Value = "MaxSlots";
            ws.Cell(1, 6).Value = "PromotionType";

            ws.Row(1).Style.Font.Bold = true;

            var rowIndex = 2;
            foreach (var r in rows)
            {
                ws.Cell(rowIndex, 1).Value = r.GolfCourseName;
                ws.Cell(rowIndex, 2).Value = r.FromDate;
                ws.Cell(rowIndex, 3).Value = r.ToDate;
                ws.Cell(rowIndex, 2).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(rowIndex, 3).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(rowIndex, 4).Value = r.StartTime;
                ws.Cell(rowIndex, 5).Value = r.EndTime;
                ws.Cell(rowIndex, 6).Value = r.MaxSlots;
                ws.Cell(rowIndex, 7).Value = r.PromotionType.ToString();

                rowIndex++;
            }

            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new RemoteStreamContent(
                stream,
                "CalendarSlot.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );
        }
    }
}
