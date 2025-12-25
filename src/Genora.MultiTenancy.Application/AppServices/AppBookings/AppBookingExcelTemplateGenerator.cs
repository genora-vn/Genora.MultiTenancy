using ClosedXML.Excel;
using System.IO;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppBookings;
public class AppBookingExcelTemplateGenerator : ITransientDependency
{
    public IRemoteStreamContent GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Danh sách booking");

        // ===== TIÊU ĐỀ =====
        ws.Cell(1, 2).Value = "Ngày chơi (*)";
        ws.Cell(1, 3).Value = "Số golfer (*)";
        ws.Cell(1, 4).Value = "Tổng tiền (*)";
        ws.Cell(1, 5).Value = "Hình thức thanh toán";
        ws.Cell(1, 6).Value = "Trạng thái booking (*)";
        ws.Cell(1, 7).Value = "Nguồn booking";

        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

        // ===== MÔ TẢ =====
        ws.Cell(2, 2).Value = "dd/MM/yyyy";
        ws.Cell(2, 3).Value = "Số nguyên > 0";
        ws.Cell(2, 4).Value = "Ví dụ: 2500000";
        ws.Cell(2, 5).Value = "COD | Online | BankTransfer";
        ws.Cell(2, 6).Value = "Processing | Confirmed | Paid | Completed | CancelledRefund | CancelledNoRefund";
        ws.Cell(2, 7).Value = "MiniApp | Hotline | Agent";

        ws.Row(2).Style.Font.Italic = true;
        ws.Row(2).Style.Font.FontColor = XLColor.DarkGray;

        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(
            stream,
            "Template_Import_Booking.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}
