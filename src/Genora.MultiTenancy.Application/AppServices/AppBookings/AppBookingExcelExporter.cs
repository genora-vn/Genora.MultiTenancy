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
        ws.Cell(1, 1).Value = "Mã booking";
        ws.Cell(1, 2).Value = "Khách hàng";
        ws.Cell(1, 3).Value = "Loại khách hàng";
        ws.Cell(1, 4).Value = "Loại ưu đãi";
        ws.Cell(1, 5).Value = "Ngày chơi";
        ws.Cell(1, 6).Value = "Giờ chơi";
        ws.Cell(1, 7).Value = "Số golfer";
        ws.Cell(1, 8).Value = "Tổng giá trị booking";
        ws.Cell(1, 9).Value = "Xuất hóa đơn";
        ws.Cell(1, 10).Value = "Thanh toán";
        ws.Cell(1, 11).Value = "Trạng thái";
        ws.Cell(1, 12).Value = "Nguồn";
        ws.Cell(1, 13).Value = "Tên công ty";
        ws.Cell(1, 14).Value = "Mã số thuế";
        ws.Cell(1, 15).Value = "Địa chỉ";
        ws.Cell(1, 16).Value = "Email nhận hóa đơn";

        ws.Row(1).Style.Font.Bold = true;

        var rowIndex = 2;
        foreach (var r in rows)
        {
            ws.Cell(rowIndex, 1).Value = r.BookingCode;
            ws.Cell(rowIndex, 2).Value = r.Customer;
            ws.Cell(rowIndex, 3).Value = r.CustomerType;
            ws.Cell(rowIndex, 4).Value = r.PromotionType;

            ws.Cell(rowIndex, 5).Value = r.PlayDate;
            ws.Cell(rowIndex, 5).Style.DateFormat.Format = "dd/MM/yyyy";

            ws.Cell(rowIndex, 6).Value = r.PlayTime;

            ws.Cell(rowIndex, 7).Value = r.NumberOfGolfers;

            ws.Cell(rowIndex, 8).Value = r.TotalAmount;
            ws.Cell(rowIndex, 8).Style.NumberFormat.Format = "#,##0";

            ws.Cell(rowIndex, 9).Value = r.IsExportInvoice ? "Có" : "Không";

            ws.Cell(rowIndex, 10).Value = r.PaymentMethod;
            ws.Cell(rowIndex, 11).Value = r.Status;
            ws.Cell(rowIndex, 12).Value = r.Source;

            ws.Cell(rowIndex, 13).Value = r.CompanyName;
            ws.Cell(rowIndex, 14).Value = r.TaxCode;
            ws.Cell(rowIndex, 15).Value = r.CompanyAddress;
            ws.Cell(rowIndex, 16).Value = r.InvoiceEmail;

            rowIndex++;
        }

        // Style nhẹ cho header
        var headerRange = ws.Range(1, 1, 1, 16);
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        // Border cho toàn bộ data nếu có dòng
        if (rows.Count > 0)
        {
            var dataRange = ws.Range(1, 1, rowIndex - 1, 16);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
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