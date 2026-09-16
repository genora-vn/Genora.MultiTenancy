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

        // Headers — mở rộng thêm các cột: ZaloUserId, Ảnh thiệp, Lời chúc, Lịch sử quay.
        ws.Cell(1, 1).Value = "Họ và tên";
        ws.Cell(1, 2).Value = "Số điện thoại";
        ws.Cell(1, 3).Value = "ZaloUserId";
        ws.Cell(1, 4).Value = "Nhóm tuổi";
        ws.Cell(1, 5).Value = "Giới tính";
        ws.Cell(1, 6).Value = "Địa chỉ nhận quà";
        ws.Cell(1, 7).Value = "Ngày tham gia";
        ws.Cell(1, 8).Value = "Follow OA";
        ws.Cell(1, 9).Value = "Đồng ý chia sẻ";
        ws.Cell(1, 10).Value = "Lượt còn lại";
        ws.Cell(1, 11).Value = "Tổng lượt";
        ws.Cell(1, 12).Value = "Số quà đã trúng";
        ws.Cell(1, 13).Value = "Ảnh thiệp (URL)";
        ws.Cell(1, 14).Value = "Lời chúc";
        ws.Cell(1, 15).Value = "Lịch sử quay";

        var headerRange = ws.Range(1, 1, 1, 15);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        for (int i = 0; i < items.Count; i++)
        {
            var row = i + 2;
            var item = items[i];
            ws.Cell(row, 1).Value = item.FullName ?? "";
            ws.Cell(row, 2).Value = item.PhoneNumber ?? "";
            ws.Cell(row, 3).Value = item.ZaloUserId ?? "";
            ws.Cell(row, 4).Value = AgeGroupText(item.AgeGroup);
            ws.Cell(row, 5).Value = GenderText(item.Gender);
            ws.Cell(row, 6).Value = item.ReceiveAddress ?? "";
            ws.Cell(row, 7).Value = item.JoinedTime.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 8).Value = item.IsFollowingOa ? "Có" : "Không";
            ws.Cell(row, 9).Value = item.HasConsent ? "Có" : "Không";
            ws.Cell(row, 10).Value = item.RemainingSpinTurns;
            ws.Cell(row, 11).Value = item.TotalSpinTurns;
            ws.Cell(row, 12).Value = item.TotalGiftsWon;
            ws.Cell(row, 13).Value = item.LatestFrameImageUrl ?? "";
            ws.Cell(row, 14).Value = item.LatestWishMessage ?? "";
            ws.Cell(row, 15).Value = FormatSpinLogs(item.RecentSpinLogs);
        }

        ws.Columns().AdjustToContents();

        var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new RemoteStreamContent(stream, $"Hl25Participants_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static string FormatSpinLogs(List<Hl25ParticipantSpinLogSummary>? logs)
    {
        if (logs == null || logs.Count == 0) return "";
        var parts = new List<string>();
        foreach (var s in logs)
        {
            var status = s.RewardStatus switch
            {
                Hl25RewardStatus.Won => "Trúng",
                Hl25RewardStatus.NotWon => "Không trúng",
                Hl25RewardStatus.Delivered => "Đã trao",
                Hl25RewardStatus.Cancelled => "Đã hủy",
                _ => "Chờ"
            };
            var gift = !string.IsNullOrEmpty(s.GiftName) ? $" - {s.GiftName}" : "";
            parts.Add($"{s.SpinTime:dd/MM/yyyy HH:mm}{gift} ({status})");
        }
        return string.Join("; ", parts);
    }

    private static string GenderText(Hl25Gender gender) => gender switch
    {
        Hl25Gender.Male => "Nam",
        Hl25Gender.Female => "Nữ",
        Hl25Gender.Other => "Khác",
        _ => "Không xác định"
    };

    private static string AgeGroupText(Hl25AgeGroup ageGroup) => ageGroup switch
    {
        Hl25AgeGroup.Age18To25 => "18 - 25",
        Hl25AgeGroup.Age26To35 => "26 - 35",
        Hl25AgeGroup.Age36To44 => "36 - 44",
        _ => "Không xác định"
    };
}
