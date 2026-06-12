using System.Collections.Generic;
using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.Caddies;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Caddies;

public class AppCaddieBookingExcelExporter : ITransientDependency
{
    public IRemoteStreamContent Export(List<CaddieBookingDto> items)
    {
        var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Booking History");

        // Headers
        ws.Cell(1, 1).Value = "Mã Booking";
        ws.Cell(1, 2).Value = "Khách hàng";
        ws.Cell(1, 3).Value = "SĐT";
        ws.Cell(1, 4).Value = "Caddie";
        ws.Cell(1, 5).Value = "Mã Caddie";
        ws.Cell(1, 6).Value = "Ngày chơi";
        ws.Cell(1, 7).Value = "Giờ chơi";
        ws.Cell(1, 8).Value = "Số hố";
        ws.Cell(1, 9).Value = "Trạng thái";
        ws.Cell(1, 10).Value = "TT Thanh toán";
        ws.Cell(1, 11).Value = "Phương thức TT";
        ws.Cell(1, 12).Value = "Phí Caddie";
        ws.Cell(1, 13).Value = "Ngày tạo";
        ws.Cell(1, 14).Value = "Ghi chú";

        // Style header
        var headerRange = ws.Range(1, 1, 1, 14);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Data
        for (int i = 0; i < items.Count; i++)
        {
            var row = i + 2;
            var item = items[i];
            ws.Cell(row, 1).Value = item.BookingCode;
            ws.Cell(row, 2).Value = item.CustomerName;
            ws.Cell(row, 3).Value = item.Phone;
            ws.Cell(row, 4).Value = item.CaddieName ?? "—";
            ws.Cell(row, 5).Value = item.CaddieCode ?? "—";
            ws.Cell(row, 6).Value = item.BookingDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 7).Value = item.StartTime.ToString(@"hh\:mm");
            ws.Cell(row, 8).Value = item.NumberOfHoles?.ToString() ?? "—";
            ws.Cell(row, 9).Value = item.StatusText ?? "—";
            ws.Cell(row, 10).Value = item.PaymentStatusText ?? "—";
            ws.Cell(row, 11).Value = item.PaymentMethodText ?? "—";
            ws.Cell(row, 12).Value = item.TotalCaddieFee;
            ws.Cell(row, 13).Value = item.CreationTime.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 14).Value = item.Note ?? "";
        }

        ws.Columns().AdjustToContents();

        var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(stream, $"CaddieBookings_{System.DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
