using ClosedXML.Excel;
using Genora.MultiTenancy.DomainModels.AppHlg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Hlg.Admin;

public class HlgWinnerExcelTemplateGenerator : ITransientDependency
{
    public IRemoteStreamContent Generate(IEnumerable<HlgRankingEvent> events, IEnumerable<HlgRankingPrize> prizes)
    {
        var eventList = events.ToList();
        using var workbook = new XLWorkbook();
        var input = workbook.Worksheets.Add("TraoThuong");
        var headers = new[] { "SỰ KIỆN (*)", "GIẢI THƯỞNG (*)", "SỐ ĐIỆN THOẠI NGƯỜI TRÚNG (*)", "CÔNG BỐ" };
        var examples = new[] { "Chọn sự kiện ở danh sách", "Chọn giải thưởng ở danh sách", "VD: 0901234567", "TRUE / FALSE (mặc định FALSE)" };

        for (var column = 1; column <= headers.Length; column++)
        {
            input.Cell(1, column).Value = headers[column - 1];
            input.Cell(2, column).Value = examples[column - 1];
        }

        var header = input.Range(1, 1, 1, headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        input.Row(2).Style.Font.FontColor = XLColor.DarkGray;
        input.Column(1).Width = 65;
        input.Column(2).Width = 40;
        input.Column(3).Width = 35;
        input.Column(4).Width = 28;
        input.Column(3).Style.NumberFormat.Format = "@";
        input.Range(1, 1, 1000, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        input.Range(1, 1, 1000, headers.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        input.SheetView.FreezeRows(2);

        var eventReference = workbook.Worksheets.Add("DanhMucSuKien");
        eventReference.Cell(1, 1).Value = "GIÁ TRỊ CHỌN";
        eventReference.Cell(1, 2).Value = "TÊN SỰ KIỆN";
        eventReference.Cell(1, 4).Value = "TRẠNG THÁI CÔNG BỐ";
        eventReference.Cell(2, 4).Value = "TRUE";
        eventReference.Cell(3, 4).Value = "FALSE";
        var row = 2;
        foreach (var rankingEvent in eventList)
        {
            eventReference.Cell(row, 1).Value = $"{rankingEvent.Title} | {rankingEvent.Id}";
            eventReference.Cell(row, 2).Value = rankingEvent.Title;
            row++;
        }

        eventReference.Range(1, 1, 1, 4).Style.Font.Bold = true;
        eventReference.Range(1, 1, 1, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
        eventReference.Column(1).Width = 65;
        eventReference.Column(2).Width = 40;
        eventReference.Column(4).Width = 24;
        eventReference.SheetView.FreezeRows(1);

        eventReference.Range("D2:D3").AddToNamed("HlgWinnerPublishedValues");
        input.Range("D3:D1000").CreateDataValidation().List("=HlgWinnerPublishedValues", true);

        if (row > 2)
        {
            eventReference.Range(2, 1, row - 1, 1).AddToNamed("HlgWinnerEventIds");
            input.Range("A3:A1000").CreateDataValidation().List("=HlgWinnerEventIds", true);
        }

        var reference = workbook.Worksheets.Add("DanhMucGiai");
        reference.Cell(1, 1).Value = "GIÁ TRỊ CHỌN";
        reference.Cell(1, 2).Value = "TÊN GIẢI";
        reference.Cell(1, 3).Value = "SỰ KIỆN";
        reference.Cell(1, 4).Value = "SỐ LƯỢNG";
        row = 2;
        foreach (var prize in prizes)
        {
            reference.Cell(row, 1).Value = $"{prize.Title} | {prize.Id}";
            reference.Cell(row, 2).Value = prize.Title;
            reference.Cell(row, 3).Value = eventList.FirstOrDefault(x => x.Id == prize.EventId)?.Title ?? "";
            reference.Cell(row, 4).Value = prize.Quantity;
            row++;
        }

        reference.Range(1, 1, 1, 4).Style.Font.Bold = true;
        reference.Range(1, 1, 1, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
        reference.Column(1).Width = 65;
        reference.Column(2).Width = 40;
        reference.Column(3).Width = 40;
        reference.Column(4).Width = 15;
        reference.SheetView.FreezeRows(1);

        if (row > 2)
        {
            reference.Range(2, 1, row - 1, 1).AddToNamed("HlgWinnerPrizeIds");
            var prizeValidation = input.Range("B3:B1000").CreateDataValidation();
            prizeValidation.List("=HlgWinnerPrizeIds", true);
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return new RemoteStreamContent(
            stream,
            $"Template_Import_HlgWinners_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
