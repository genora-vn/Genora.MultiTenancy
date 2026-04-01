using ClosedXML.Excel;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots;

public class AppCalendarExcelTemplateGenerator : ITransientDependency
{
    private const int MaxDataRow = 1000;
    private const string ExcelMoneyFormat = "#,##0"; // nếu chỉ muốn số nguyên: "#,##0"

    private sealed class PriceColumnDef
    {
        public int Holes { get; set; }
        public string Header { get; set; } = default!;
        public bool IsRequired { get; set; }
    }

    public IRemoteStreamContent GenerateTemplate(
        List<GolfCourse> golfCourses,
        List<CustomerType> customerTypes,
        List<PromotionType> promotions,
        List<SpecialDate> specialDates,
        Guid? golfCourseId = null)
    {
        using var workbook = new XLWorkbook();

        var ws = workbook.Worksheets.Add("Danh sách cấu hình tee time");
        var lookup = workbook.Worksheets.Add("Lookup");
        lookup.Visibility = XLWorksheetVisibility.VeryHidden;

        const int headerTopRow = 1;
        const int headerMiddleRow = 2;
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

        var selectedGolfCourse = golfCourseId.HasValue
            ? golfCourses.FirstOrDefault(x => x.Id == golfCourseId.Value)
            : null;

        // Nếu tải template riêng theo 1 sân => render đúng cột giá theo NumberHoles
        // Nếu tải template chung nhiều sân => render full bộ cột để không làm hỏng cấu trúc import nhiều sân
        var priceColumns = BuildPriceColumns(selectedGolfCourse?.NumberHoles, renderAllWhenNoCourseSelected: selectedGolfCourse == null);

        ws.Cell(headerTopRow, colGolfCode).Value = "Mã sân golf (*)";
        ws.Cell(headerTopRow, colDayType).Value = "Loại ngày (*)";
        ws.Cell(headerTopRow, colFromDate).Value = "Ngày bắt đầu (*)";
        ws.Cell(headerTopRow, colToDate).Value = "Ngày kết thúc (*)";
        ws.Cell(headerTopRow, colStartTime).Value = "Giờ bắt đầu (*)";
        ws.Cell(headerTopRow, colEndTime).Value = "Giờ kết thúc";
        ws.Cell(headerTopRow, colPromotion).Value = "Loại ưu đãi (*)";
        ws.Cell(headerTopRow, colMaxSlots).Value = "Số slot tối đa";
        ws.Cell(headerTopRow, colNote).Value = "Ghi chú";
        ws.Cell(headerTopRow, colGap).Value = "Gap (phút)";

        for (int c = colGolfCode; c <= colGap; c++)
        {
            ws.Range(headerTopRow, c, headerBottomRow, c).Merge();
        }

        ws.Column(colGolfCode).Width = 18;
        ws.Column(colDayType).Width = 20;
        ws.Column(colFromDate).Width = 16;
        ws.Column(colToDate).Width = 16;
        ws.Column(colStartTime).Width = 14;
        ws.Column(colEndTime).Width = 14;
        ws.Column(colPromotion).Width = 18;
        ws.Column(colMaxSlots).Width = 14;
        ws.Column(colNote).Width = 24;
        ws.Column(colGap).Width = 14;

        var totalCustomerTypes = customerTypes?.Count ?? 0;
        var totalPriceCols = totalCustomerTypes * priceColumns.Count;
        var priceEndCol = totalPriceCols > 0
            ? priceStartCol + totalPriceCols - 1
            : colGap;

        if (totalCustomerTypes > 0 && priceColumns.Count > 0)
        {
            ws.Range(headerTopRow, priceStartCol, headerTopRow, priceEndCol).Merge();
            ws.Cell(headerTopRow, priceStartCol).Value = "Bảng giá";

            for (int i = 0; i < totalCustomerTypes; i++)
            {
                var customerType = customerTypes[i];
                int groupStart = priceStartCol + (i * priceColumns.Count);
                int groupEnd = groupStart + priceColumns.Count - 1;

                ws.Range(headerMiddleRow, groupStart, headerMiddleRow, groupEnd).Merge();
                ws.Cell(headerMiddleRow, groupStart).Value = customerType.Name;

                for (int j = 0; j < priceColumns.Count; j++)
                {
                    int col = groupStart + j;
                    var priceCol = priceColumns[j];

                    ws.Cell(headerBottomRow, col).Value = priceCol.Header;
                    ws.Column(col).Width = 16;

                    // Format hiển thị kiểu tiền tệ, nhưng bản chất vẫn là numeric
                    ws.Range(dataStartRow, col, MaxDataRow, col).Style.NumberFormat.Format = ExcelMoneyFormat;
                    ws.Range(dataStartRow, col, MaxDataRow, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
            }
        }

        var lastHeaderCol = Math.Max(priceEndCol, colGap);
        var headerRange = ws.Range(headerTopRow, 1, headerBottomRow, lastHeaderCol);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.WrapText = true;

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

        // Hàng hướng dẫn
        ws.Cell(hintRow, colGolfCode).Value = selectedGolfCourse?.Code ?? "VD: MONT";
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
        ws.Row(hintRow).Style.Alignment.WrapText = true;

        // Hướng dẫn cho cột giá
        if (totalCustomerTypes > 0 && priceColumns.Count > 0)
        {
            for (int i = 0; i < totalCustomerTypes; i++)
            {
                int groupStart = priceStartCol + (i * priceColumns.Count);

                for (int j = 0; j < priceColumns.Count; j++)
                {
                    var col = groupStart + j;
                    var priceCol = priceColumns[j];

                    ws.Cell(hintRow, col).Value = priceCol.IsRequired
                        ? "Nhập số, có thể gõ 1500000 hoặc 1,500,000"
                        : "Không bắt buộc - nhập số nếu áp dụng";
                }
            }
        }

        // Format ngày giờ
        ws.Range(dataStartRow, colFromDate, MaxDataRow, colFromDate).Style.DateFormat.Format = "dd/MM/yyyy";
        ws.Range(dataStartRow, colToDate, MaxDataRow, colToDate).Style.DateFormat.Format = "dd/MM/yyyy";
        ws.Range(dataStartRow, colStartTime, MaxDataRow, colStartTime).Style.NumberFormat.Format = "hh:mm";
        ws.Range(dataStartRow, colEndTime, MaxDataRow, colEndTime).Style.NumberFormat.Format = "hh:mm";

        // Lookup - DayTypes
        lookup.Cell(1, 1).Value = "DayTypes";
        for (int i = 0; i < dayTypes.Count; i++)
        {
            lookup.Cell(2 + i, 1).Value = dayTypes[i];
        }
        var dayTypeRange = lookup.Range(2, 1, 1 + dayTypes.Count, 1);
        dayTypeRange.AddToNamed("DayTypes");

        // Lookup - PromotionTypes
        var promotionNames = (promotions ?? new List<PromotionType>())
            .Select(p => p.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        lookup.Cell(1, 2).Value = "PromotionTypes";
        if (promotionNames.Count == 0)
        {
            promotionNames.Add("Normal");
        }

        for (int i = 0; i < promotionNames.Count; i++)
        {
            lookup.Cell(2 + i, 2).Value = promotionNames[i];
        }
        var promoRange = lookup.Range(2, 2, 1 + promotionNames.Count, 2);
        promoRange.AddToNamed("PromotionTypes");

        // Lookup - GolfCourses
        var activeGolfCourses = (golfCourses ?? new List<GolfCourse>())
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToList();

        lookup.Cell(1, 3).Value = "GolfCourseCodes";
        lookup.Cell(1, 4).Value = "GolfCourseNames";
        lookup.Cell(1, 5).Value = "GolfCourseNumberHoles";

        for (int i = 0; i < activeGolfCourses.Count; i++)
        {
            var row = 2 + i;
            lookup.Cell(row, 3).Value = activeGolfCourses[i].Code;
            lookup.Cell(row, 4).Value = activeGolfCourses[i].Name;
            lookup.Cell(row, 5).Value = activeGolfCourses[i].NumberHoles;
        }

        if (activeGolfCourses.Count > 0)
        {
            var golfCodeRange = lookup.Range(2, 3, 1 + activeGolfCourses.Count, 3);
            golfCodeRange.AddToNamed("GolfCourseCodes");
        }

        // Data validation dropdown
        ws.Range($"A{dataStartRow}:A{MaxDataRow}").SetDataValidation().List("=GolfCourseCodes", true);
        ws.Range($"B{dataStartRow}:B{MaxDataRow}").SetDataValidation().List("=DayTypes", true);
        ws.Range($"G{dataStartRow}:G{MaxDataRow}").SetDataValidation().List("=PromotionTypes", true);

        // Numeric validation cho MaxSlots, Gap
        var maxSlotsValidation = ws.Range($"H{dataStartRow}:H{MaxDataRow}").SetDataValidation();
        maxSlotsValidation.AllowedValues = XLAllowedValues.WholeNumber;
        maxSlotsValidation.Operator = XLOperator.EqualOrGreaterThan;
        maxSlotsValidation.MinValue = "0";

        var gapValidation = ws.Range($"J{dataStartRow}:J{MaxDataRow}").SetDataValidation();
        gapValidation.AllowedValues = XLAllowedValues.WholeNumber;
        gapValidation.Operator = XLOperator.EqualOrGreaterThan;
        gapValidation.MinValue = "0";

        // Numeric validation cho cột giá
        if (totalCustomerTypes > 0 && priceColumns.Count > 0)
        {
            for (int i = 0; i < totalCustomerTypes; i++)
            {
                int groupStart = priceStartCol + (i * priceColumns.Count);

                for (int j = 0; j < priceColumns.Count; j++)
                {
                    int col = groupStart + j;

                    var priceValidation = ws.Range(dataStartRow, col, MaxDataRow, col).SetDataValidation();
                    priceValidation.AllowedValues = XLAllowedValues.Decimal;
                    priceValidation.Operator = XLOperator.EqualOrGreaterThan;
                    priceValidation.MinValue = "0";
                }
            }
        }
        // Freeze
        ws.SheetView.FreezeRows(dataStartRow - 1);

        // Borders vùng nhập
        ws.Range(dataStartRow, 1, MaxDataRow, lastHeaderCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(dataStartRow, 1, MaxDataRow, lastHeaderCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Auto filter
        ws.Range(headerBottomRow, 1, MaxDataRow, lastHeaderCol).SetAutoFilter();

        ws.Columns(1, lastHeaderCol).AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = selectedGolfCourse == null
            ? $"Template_Import_Calendar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            : $"Template_Import_Calendar_{selectedGolfCourse.Code}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return new RemoteStreamContent(
            stream,
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }

    private static List<PriceColumnDef> BuildPriceColumns(string? numberHoles, bool renderAllWhenNoCourseSelected)
    {
        if (renderAllWhenNoCourseSelected)
        {
            return new List<PriceColumnDef>
            {
                new() { Holes = 9,  Header = "Giá 9 hố",  IsRequired = false },
                new() { Holes = 18, Header = "Giá 18 hố (*)", IsRequired = true  },
                new() { Holes = 27, Header = "Giá 27 hố", IsRequired = false },
                new() { Holes = 36, Header = "Giá 36 hố", IsRequired = false }
            };
        }

        var supported = ResolveSupportedHoles(numberHoles);

        var result = new List<PriceColumnDef>();

        if (supported.Contains(9))
        {
            result.Add(new PriceColumnDef { Holes = 9, Header = "Giá 9 hố", IsRequired = false });
        }

        if (supported.Contains(18))
        {
            result.Add(new PriceColumnDef { Holes = 18, Header = "Giá 18 hố (*)", IsRequired = true });
        }

        if (supported.Contains(27))
        {
            result.Add(new PriceColumnDef { Holes = 27, Header = "Giá 27 hố", IsRequired = false });
        }

        if (supported.Contains(36))
        {
            result.Add(new PriceColumnDef { Holes = 36, Header = "Giá 36 hố", IsRequired = false });
        }

        // fallback
        if (result.Count == 0)
        {
            result.Add(new PriceColumnDef { Holes = 18, Header = "Giá 18 hố (*)", IsRequired = true });
        }

        return result;
    }

    private static HashSet<int> ResolveSupportedHoles(string? numberHoles)
    {
        // Hỗ trợ các dạng:
        // "18", "27", "36", "18,27", "18-27", "9;18;27", ...
        if (string.IsNullOrWhiteSpace(numberHoles))
        {
            return new HashSet<int> { 18 };
        }

        var numbers = Regex.Matches(numberHoles, @"\d+")
            .Select(x => int.TryParse(x.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0)
            .Where(x => x is 9 or 18 or 27 or 36)
            .Distinct()
            .ToList();

        if (numbers.Count == 0)
        {
            return new HashSet<int> { 18 };
        }

        // Nếu cấu hình chỉ có 18 => cho nhập 9 và 18
        // Nếu 27 => cho nhập 9,18,27
        // Nếu 36 => cho nhập 9,18,27,36
        var max = numbers.Max();

        return max switch
        {
            36 => new HashSet<int> { 9, 18, 27, 36 },
            27 => new HashSet<int> { 9, 18, 27 },
            18 => new HashSet<int> { 9, 18 },
            9 => new HashSet<int> { 9 },
            _ => new HashSet<int> { 18 }
        };
    }
}