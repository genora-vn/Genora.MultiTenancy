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

        ws.Cell(1, 1).Value = "Mã booking";
        ws.Cell(1, 2).Value = "Khách hàng";
        ws.Cell(1, 3).Value = "Ngày chơi";
        ws.Cell(1, 4).Value = "Giờ chơi";
        ws.Cell(1, 5).Value = "Số golfer";
        ws.Cell(1, 6).Value = "Tổng giá trị booking";
        ws.Cell(1, 7).Value = "Xuất hóa đơn";
        ws.Cell(1, 8).Value = "Thanh toán";
        ws.Cell(1, 9).Value = "Trạng thái";
        ws.Cell(1, 10).Value = "Nguồn";
        ws.Cell(1, 11).Value = "Tên công ty";
        ws.Cell(1, 12).Value = "Mã số thuế";
        ws.Cell(1, 13).Value = "Địa chỉ";
        ws.Cell(1, 14).Value = "Email nhận hóa đơn";

        ws.Row(1).Style.Font.Bold = true;

        var rowIndex = 2;
        foreach (var r in rows)
        {
            ws.Cell(rowIndex, 1).Value = r.BookingCode;
            ws.Cell(rowIndex, 2).Value = r.Customer;

            ws.Cell(rowIndex, 3).Value = r.PlayDate;
            ws.Cell(rowIndex, 3).Style.DateFormat.Format = "dd/MM/yyyy";

            ws.Cell(rowIndex, 4).Value = r.PlayTime;

            ws.Cell(rowIndex, 5).Value = r.NumberOfGolfers;

            ws.Cell(rowIndex, 6).Value = r.TotalAmount;
            ws.Cell(rowIndex, 6).Style.NumberFormat.Format = "#,##0";

            ws.Cell(rowIndex, 7).Value = r.IsExportInvoice ? "Có" : "Không";

            ws.Cell(rowIndex, 8).Value = r.PaymentMethod;
            ws.Cell(rowIndex, 9).Value = r.Status;
            ws.Cell(rowIndex, 10).Value = r.Source;

            ws.Cell(rowIndex, 11).Value = r.CompanyName;
            ws.Cell(rowIndex, 12).Value = r.TaxCode;
            ws.Cell(rowIndex, 13).Value = r.CompanyAddress;
            ws.Cell(rowIndex, 14).Value = r.InvoiceEmail;

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
