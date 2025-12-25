using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppBookings;
using System.Collections.Generic;
using System.IO;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppBookings;
public class AppBookingExcelExporter : ITransientDependency
{
    public IRemoteStreamContent Export(List<AppBookingExcelRowDto> rows)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bookings");

        // Header
        ws.Cell(1, 1).Value = "BookingCode";
        ws.Cell(1, 2).Value = "CustomerName";
        ws.Cell(1, 3).Value = "CustomerPhone";
        ws.Cell(1, 4).Value = "GolfCourse";
        ws.Cell(1, 5).Value = "PlayDate";
        ws.Cell(1, 6).Value = "Golfers";
        ws.Cell(1, 7).Value = "TotalAmount";
        ws.Cell(1, 8).Value = "PaymentMethod";
        ws.Cell(1, 9).Value = "Status";
        ws.Cell(1, 10).Value = "Source";

        ws.Row(1).Style.Font.Bold = true;

        var rowIndex = 2;
        foreach (var r in rows)
        {
            ws.Cell(rowIndex, 1).Value = r.BookingCode;
            ws.Cell(rowIndex, 2).Value = r.CustomerName;
            ws.Cell(rowIndex, 3).Value = r.CustomerPhone;
            ws.Cell(rowIndex, 4).Value = r.GolfCourseName;

            ws.Cell(rowIndex, 5).Value = r.PlayDate;
            ws.Cell(rowIndex, 5).Style.DateFormat.Format = "dd/MM/yyyy";

            ws.Cell(rowIndex, 6).Value = r.NumberOfGolfers;
            ws.Cell(rowIndex, 7).Value = r.TotalAmount;
            ws.Cell(rowIndex, 8).Value = r.PaymentMethod;
            ws.Cell(rowIndex, 9).Value = r.Status;
            ws.Cell(rowIndex, 10).Value = r.Source;

            rowIndex++;
        }

        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            "Bookings.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}