using ClosedXML.Excel;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots
{
    public class AppCalendarExcelTemplateGenerator : ITransientDependency
    {
        public IRemoteStreamContent GenerateTemplate(
            List<CustomerType> customerTypes,
            List<PromotionType> promotions,
            List<SpecialDate> specialDates)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Danh sách booking");
            var lookup = workbook.Worksheets.Add("Lookup");
            lookup.Visibility = XLWorksheetVisibility.VeryHidden;

            const int headerTopRow = 1;
            const int headerBottomRow = 3;
            const int hintRow = 4;
            const int dataStartRow = 5;

            const int colGolfCode = 1;
            const int colDayType = 2;
            const int colFromDate = 3;
            const int colToDate = 4;
            const int colStartTime = 5;
            const int colEndTime = 6;
            const int colPromotion = 7;
            const int colMaxSlots = 8;
            const int colNote = 9;
            const int colGap = 10;

            int priceStartCol = 11;

            ws.Cell(headerTopRow, colGolfCode).Value = "Mã sân golf";
            ws.Cell(headerTopRow, colDayType).Value = "Loại ngày (*)";
            ws.Cell(headerTopRow, colFromDate).Value = "Ngày bắt đầu(*)";
            ws.Cell(headerTopRow, colToDate).Value = "Ngày kết thúc(*)";
            ws.Cell(headerTopRow, colStartTime).Value = "Giờ bắt đầu (*)";
            ws.Cell(headerTopRow, colEndTime).Value = "Giờ kết thúc";
            ws.Cell(headerTopRow, colPromotion).Value = "Loại ưu đãi (*)";
            ws.Cell(headerTopRow, colMaxSlots).Value = "Số slot tối đa";
            ws.Cell(headerTopRow, colNote).Value = "Ghi chú";
            ws.Cell(headerTopRow, colGap).Value = "Gap (Tần suất)";

            for (int c = colGolfCode; c <= colGap; c++)
                ws.Range(headerTopRow, c, headerBottomRow, c).Merge();

            ws.Column(colGolfCode).Width = 15;
            ws.Column(colDayType).Width = 20;
            ws.Column(colFromDate).Width = 16;
            ws.Column(colToDate).Width = 16;
            ws.Column(colStartTime).Width = 14;
            ws.Column(colEndTime).Width = 14;
            ws.Column(colPromotion).Width = 18;
            ws.Column(colMaxSlots).Width = 14;
            ws.Column(colNote).Width = 22;
            ws.Column(colGap).Width = 14;

            var totalCustomerTypes = customerTypes?.Count ?? 0;
            var priceEndCol = (totalCustomerTypes > 0)
                ? priceStartCol + (totalCustomerTypes * 4) - 1
                : colGap;

            if (totalCustomerTypes > 0)
            {
                ws.Range(headerTopRow, priceStartCol, headerTopRow, priceEndCol).Merge();
                ws.Cell(headerTopRow, priceStartCol).Value = "Bảng giá";

                for (int i = 0; i < totalCustomerTypes; i++)
                {
                    int groupStart = priceStartCol + (i * 4);
                    int groupEnd = groupStart + 3;

                    ws.Range(2, groupStart, 2, groupEnd).Merge();
                    ws.Cell(2, groupStart).Value = customerTypes[i].Name;

                    ws.Cell(3, groupStart + 0).Value = "Giá 9 hố";
                    ws.Cell(3, groupStart + 1).Value = "Giá 18 hố (*)";
                    ws.Cell(3, groupStart + 2).Value = "Giá 27 hố";
                    ws.Cell(3, groupStart + 3).Value = "Giá 36 hố";

                    ws.Column(groupStart + 0).Width = 14;
                    ws.Column(groupStart + 1).Width = 14;
                    ws.Column(groupStart + 2).Width = 14;
                    ws.Column(groupStart + 3).Width = 14;
                }
            }

            var headerRange = ws.Range(headerTopRow, 1, headerBottomRow, Math.Max(priceEndCol, colGap));
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var dayTypes = (specialDates ?? new List<SpecialDate>())
                .Where(x => x.IsActive)
                .Select(x => (x.Name ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            if (dayTypes.Count == 0)
            {
                dayTypes = new List<string> { "Ngày trong tuần", "Ngày cuối tuần", "Ngày lễ" };
            }

            ws.Cell(hintRow, colGolfCode).Value = "VD: MONT";
            ws.Cell(hintRow, colDayType).Value = string.Join("/", dayTypes);
            ws.Cell(hintRow, colFromDate).Value = "dd/MM/yyyy";
            ws.Cell(hintRow, colToDate).Value = "dd/MM/yyyy";
            ws.Cell(hintRow, colStartTime).Value = "HH:mm (vd 06:30)";
            ws.Cell(hintRow, colEndTime).Value = "HH:mm (vd 07:00)";
            ws.Cell(hintRow, colPromotion).Value = "Chọn từ dropdown";
            ws.Cell(hintRow, colMaxSlots).Value = "Số nguyên > 0";
            ws.Cell(hintRow, colNote).Value = "Ghi chú nội bộ";
            ws.Cell(hintRow, colGap).Value = "Khoảng cách (phút)";
            ws.Row(hintRow).Style.Font.FontColor = XLColor.DarkGray;

            ws.Range(dataStartRow, colFromDate, 1000, colFromDate).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Range(dataStartRow, colToDate, 1000, colToDate).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Range(dataStartRow, colStartTime, 1000, colStartTime).Style.NumberFormat.Format = "hh:mm";
            ws.Range(dataStartRow, colEndTime, 1000, colEndTime).Style.NumberFormat.Format = "hh:mm";

            lookup.Cell(1, 1).Value = "DayTypes";
            for (int i = 0; i < dayTypes.Count; i++)
                lookup.Cell(2 + i, 1).Value = dayTypes[i];

            var dayTypeRange = lookup.Range(2, 1, 2 + dayTypes.Count - 1, 1);
            workbook.NamedRanges.Add("DayTypes", dayTypeRange);

            var promotionNames = (promotions ?? new List<PromotionType>())
                .Select(p => p.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            lookup.Cell(1, 2).Value = "PromotionTypes";
            if (promotionNames.Count == 0)
            {
                lookup.Cell(2, 2).Value = "Normal";
                promotionNames.Add("Normal");
            }

            for (int i = 0; i < promotionNames.Count; i++)
                lookup.Cell(2 + i, 2).Value = promotionNames[i];

            var promoRange = lookup.Range(2, 2, 2 + promotionNames.Count - 1, 2);
            workbook.NamedRanges.Add("PromotionTypes", promoRange);

            // ====== DATA VALIDATION: use named range ======
            ws.Range($"B{dataStartRow}:B1000").SetDataValidation().List("=DayTypes", true);
            ws.Range($"G{dataStartRow}:G1000").SetDataValidation().List("=PromotionTypes", true);

            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new RemoteStreamContent(
                stream,
                $"Template_Import_Calendar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );
        }
    }
}
