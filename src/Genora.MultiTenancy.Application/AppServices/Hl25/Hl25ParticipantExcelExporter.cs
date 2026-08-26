using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>Xuất Excel danh sách người tham gia chương trình Hoa Linh 25 Năm.</summary>
public class Hl25ParticipantExcelExporter : ITransientDependency
{
    public IRemoteStreamContent Export(List<Hl25ParticipantDto> items)
    {
        var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Participants");

        ws.Cell(1, 1).Value = "Họ và tên";
        ws.Cell(1, 2).Value = "Số điện thoại";
        ws.Cell(1, 3).Value = "Ngày sinh";
        ws.Cell(1, 4).Value = "Giới tính";
        ws.Cell(1, 5).Value = "Địa chỉ nhận quà";
        ws.Cell(1, 6).Value = "Ngày tham gia";
        ws.Cell(1, 7).Value = "Follow OA";
        ws.Cell(1, 8).Value = "Đồng ý chia sẻ";
        ws.Cell(1, 9).Value = "Lượt còn lại";
        ws.Cell(1, 10).Value = "Tổng lượt";
        ws.Cell(1, 11).Value = "Số quà đã trúng";

        var headerRange = ws.Range(1, 1, 1, 11);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        for (int i = 0; i < items.Count; i++)
        {
            var row = i + 2;
            var item = items[i];
            ws.Cell(row, 1).Value = item.FullName ?? "";
            ws.Cell(row, 2).Value = item.PhoneNumber ?? "";
            ws.Cell(row, 3).Value = item.BirthDate?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(row, 4).Value = GenderText(item.Gender);
            ws.Cell(row, 5).Value = item.ReceiveAddress ?? "";
            ws.Cell(row, 6).Value = item.JoinedTime.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 7).Value = item.IsFollowingOa ? "Có" : "Không";
            ws.Cell(row, 8).Value = item.HasConsent ? "Có" : "Không";
            ws.Cell(row, 9).Value = item.RemainingSpinTurns;
            ws.Cell(row, 10).Value = item.TotalSpinTurns;
            ws.Cell(row, 11).Value = item.TotalGiftsWon;
        }

        ws.Columns().AdjustToContents();

        var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(stream, $"Hl25Participants_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static string GenderText(Hl25Gender gender) => gender switch
    {
        Hl25Gender.Male => "Nam",
        Hl25Gender.Female => "Nữ",
        Hl25Gender.Other => "Khác",
        _ => "Không xác định"
    };
}
