using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots
{
    public class AppCalendarExcelExporter
    {
        private const int FixedCols = 10;          // A..J
        private const int StartPriceCol = 11;      // K
        private const int PriceColsPerCustomerType = 4; // 9/18/27/36

        public IRemoteStreamContent Export(
            List<AppCalendarSlotExcelRowDto> rows,
            List<CustomerType> customerTypes,
            List<string>? dayTypes = null)
        {
            rows ??= new List<AppCalendarSlotExcelRowDto>();
            customerTypes ??= new List<CustomerType>();
            dayTypes ??= new List<string>();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("CalendarSlots");

            var totalCustomerTypes = customerTypes.Count;
            var totalPriceCols = totalCustomerTypes * PriceColsPerCustomerType;
            var lastCol = FixedCols + totalPriceCols;
            if (lastCol < StartPriceCol) lastCol = StartPriceCol;

            ws.Cell(1, 1).Value = "Mã sân golf"; ws.Range(1, 1, 3, 1).Merge();
            ws.Cell(1, 2).Value = "Loại ngày (*)"; ws.Range(1, 2, 3, 2).Merge();
            ws.Cell(1, 3).Value = "Ngày bắt đầu(*)"; ws.Range(1, 3, 3, 3).Merge();
            ws.Cell(1, 4).Value = "Ngày kết thúc(*)"; ws.Range(1, 4, 3, 4).Merge();
            ws.Cell(1, 5).Value = "Giờ bắt đầu (*)"; ws.Range(1, 5, 3, 5).Merge();
            ws.Cell(1, 6).Value = "Giờ kết thúc"; ws.Range(1, 6, 3, 6).Merge();
            ws.Cell(1, 7).Value = "Loại ưu đãi (*)"; ws.Range(1, 7, 3, 7).Merge();
            ws.Cell(1, 8).Value = "Số slot tối đa"; ws.Range(1, 8, 3, 8).Merge();
            ws.Cell(1, 9).Value = "Ghi chú"; ws.Range(1, 9, 3, 9).Merge();
            ws.Cell(1, 10).Value = "Gap (Tần suất)"; ws.Range(1, 10, 3, 10).Merge();

            if (totalPriceCols > 0)
            {
                ws.Cell(1, StartPriceCol).Value = "Bảng giá";
                ws.Range(1, StartPriceCol, 1, lastCol).Merge();

                for (int idx = 0; idx < totalCustomerTypes; idx++)
                {
                    var baseCol = StartPriceCol + (idx * PriceColsPerCustomerType);
                    var ctName = customerTypes[idx].Name;

                    ws.Cell(2, baseCol).Value = ctName;
                    ws.Range(2, baseCol, 2, baseCol + 3).Merge();

                    ws.Cell(3, baseCol + 0).Value = "Giá 9 hố";
                    ws.Cell(3, baseCol + 1).Value = "Giá 18 hố";
                    ws.Cell(3, baseCol + 2).Value = "Giá 27 hố";
                    ws.Cell(3, baseCol + 3).Value = "Giá 36 hố";
                }
            }
            else
            {
                ws.Cell(1, StartPriceCol).Value = "Bảng giá";
            }

            // ===== Style header =====
            var headerRange = ws.Range(1, 1, 3, lastCol);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Cell(4, 1).Value = "VD: MONT";
            ws.Cell(4, 2).Value = dayTypes.Count > 0 ? string.Join("/", dayTypes) : "Trong tuần/Cuối tuần/Ngày lễ";
            ws.Cell(4, 3).Value = "dd/MM/yyyy";
            ws.Cell(4, 4).Value = "dd/MM/yyyy";
            ws.Cell(4, 5).Value = "HH:mm (vd 06:30)";
            ws.Cell(4, 6).Value = "HH:mm (vd 07:00)";
            ws.Cell(4, 7).Value = "Normal/Promotion";
            ws.Cell(4, 8).Value = "Số nguyên < 100";
            ws.Cell(4, 9).Value = "Ghi chú nội bộ";
            ws.Cell(4, 10).Value = "Khoảng cách 2 tee time (phút)";
            ws.Row(4).Style.Font.FontColor = XLColor.DarkGray;

            var rowIndex = 5;
            foreach (var r in rows)
            {
                ws.Cell(rowIndex, 1).Value = r.GolfCourseCode ?? r.GolfCourseName;
                ws.Cell(rowIndex, 2).Value = r.DayType;
                ws.Cell(rowIndex, 3).Value = r.FromDate;
                ws.Cell(rowIndex, 4).Value = r.ToDate;
                ws.Cell(rowIndex, 5).Value = r.StartTime;
                ws.Cell(rowIndex, 6).Value = r.EndTime;
                ws.Cell(rowIndex, 7).Value = r.PromotionType;
                ws.Cell(rowIndex, 8).Value = r.MaxSlots;
                ws.Cell(rowIndex, 9).Value = r.InternalNote;
                ws.Cell(rowIndex, 10).Value = r.Gap;

                ws.Cell(rowIndex, 3).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(rowIndex, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(rowIndex, 5).Style.NumberFormat.Format = "hh:mm";
                ws.Cell(rowIndex, 6).Style.NumberFormat.Format = "hh:mm";

                var priceDict = (r.CustomerTypePrice ?? new List<CustomerTypeExcelRowDto>())
                    .Where(x => !string.IsNullOrWhiteSpace(x.CustomerType))
                    .GroupBy(x => x.CustomerType.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);

                for (int idx = 0; idx < totalCustomerTypes; idx++)
                {
                    var baseCol = StartPriceCol + (idx * PriceColsPerCustomerType);
                    var ctName = (customerTypes[idx].Name ?? "").Trim();

                    if (!string.IsNullOrWhiteSpace(ctName) && priceDict.TryGetValue(ctName, out var p))
                    {
                        if (p.Price9.HasValue) ws.Cell(rowIndex, baseCol + 0).Value = p.Price9.Value;
                        ws.Cell(rowIndex, baseCol + 1).Value = p.Price18;
                        if (p.Price27.HasValue) ws.Cell(rowIndex, baseCol + 2).Value = p.Price27.Value;
                        if (p.Price36.HasValue) ws.Cell(rowIndex, baseCol + 3).Value = p.Price36.Value;
                    }
                }

                rowIndex++;
            }

            if (totalPriceCols > 0)
            {
                ws.Range(5, StartPriceCol, Math.Max(rowIndex - 1, 5), lastCol).Style.NumberFormat.Format = "#,##0";
            }

            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new RemoteStreamContent(
                stream,
                $"CalendarSlots_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );
        }
    }
}
