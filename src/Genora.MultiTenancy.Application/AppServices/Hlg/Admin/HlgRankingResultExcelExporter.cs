using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Hlg.Admin;

public class HlgRankingResultExcelRow
{
    public int EventRank { get; set; }
    public string? CustomerCode { get; set; }
    public string PlayerName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? ZaloUserId { get; set; }
    public string GameName { get; set; } = "";
    public int PlayCount { get; set; }
    public int GameScore { get; set; }
    public int BestScore { get; set; }
    public int CorrectAnswerCount { get; set; }
    public int TotalQuestionCount { get; set; }
    public int EventScore { get; set; }
    public DateTime FirstPlayedAt { get; set; }
    public DateTime LastPlayedAt { get; set; }
}

public class HlgRankingResultExcelExporter : ITransientDependency
{
    public IRemoteStreamContent Export(string eventTitle, IReadOnlyList<HlgRankingResultExcelRow> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Kết quả sự kiện");
        var headers = new[]
        {
            "STT", "Xếp hạng sự kiện", "Mã người chơi", "Họ và tên", "Số điện thoại", "Zalo User ID",
            "Tên trò chơi", "Số lượt chơi", "Điểm trò chơi", "Điểm cao nhất/lượt", "Số câu đúng",
            "Tổng số câu", "Tổng điểm sự kiện", "Lượt đầu tiên", "Lượt gần nhất"
        };

        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var header = sheet.Range(1, 1, 1, headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.SheetView.FreezeRows(1);

        for (var i = 0; i < rows.Count; i++)
        {
            var row = i + 2;
            var item = rows[i];
            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = item.EventRank;
            SetText(sheet.Cell(row, 3), item.CustomerCode);
            SetText(sheet.Cell(row, 4), item.PlayerName);
            SetText(sheet.Cell(row, 5), item.PhoneNumber);
            SetText(sheet.Cell(row, 6), item.ZaloUserId);
            SetText(sheet.Cell(row, 7), item.GameName);
            sheet.Cell(row, 8).Value = item.PlayCount;
            sheet.Cell(row, 9).Value = item.GameScore;
            sheet.Cell(row, 10).Value = item.BestScore;
            sheet.Cell(row, 11).Value = item.CorrectAnswerCount;
            sheet.Cell(row, 12).Value = item.TotalQuestionCount;
            sheet.Cell(row, 13).Value = item.EventScore;
            sheet.Cell(row, 14).Value = item.FirstPlayedAt;
            sheet.Cell(row, 15).Value = item.LastPlayedAt;
        }

        sheet.Columns(8, 13).Style.NumberFormat.Format = "#,##0";
        sheet.Columns(14, 15).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
        sheet.Range(1, 1, Math.Max(1, rows.Count + 1), headers.Length).SetAutoFilter();
        sheet.Columns().AdjustToContents(10d, 45d);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return new RemoteStreamContent(
            stream,
            $"HLG_KetQua_{SafeFileName(eventTitle)}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static void SetText(IXLCell cell, string? value)
    {
        cell.Style.NumberFormat.Format = "@";
        cell.Value = value ?? "";
    }

    private static string SafeFileName(string value)
    {
        var result = value.Trim();
        foreach (var character in Path.GetInvalidFileNameChars()) result = result.Replace(character, '_');
        return string.IsNullOrWhiteSpace(result) ? "SuKien" : result;
    }
}
