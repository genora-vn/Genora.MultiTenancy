using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots
{
    public class AppCalendarExcelImporter : ITransientDependency
    {
        private readonly IStringLocalizer<MultiTenancyResource> _l;

        public AppCalendarExcelImporter(IStringLocalizer<MultiTenancyResource> l)
        {
            _l = l;
        }

        private sealed class PriceColumnMeta
        {
            public int ColumnIndex { get; set; }
            public string CustomerTypeName { get; set; } = default!;
            public int Holes { get; set; }
        }

        public List<(int Row, AppCalendarSlotExcelRowDto Data)> Read(
            Stream stream,
            List<CustomerType> customerTypes)
        {
            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);

            var results = new List<(int, AppCalendarSlotExcelRowDto)>();
            var row = 5;

            var priceColumns = ReadPriceColumns(ws);

            while (!IsDataRowEmpty(ws, row))
            {
                try
                {
                    var dto = new AppCalendarSlotExcelRowDto
                    {
                        GolfCourseCode = ws.Cell(row, 1).GetString()?.Trim(),
                        DayType = ws.Cell(row, 2).GetString()?.Trim(),
                        FromDate = ExcelHelper.ReadDate(ws.Cell(row, 3)),
                        ToDate = ExcelHelper.ReadDate(ws.Cell(row, 4)),
                        StartTime = ExcelHelper.ReadTime(ws.Cell(row, 5)),
                        EndTime = ExcelHelper.ReadTime(ws.Cell(row, 6)),
                        PromotionType = ws.Cell(row, 7).GetString()?.Trim() ?? "",
                        MaxSlots = ws.Cell(row, 8).GetValue<int>(),
                        InternalNote = ws.Cell(row, 9).GetString(),
                        Gap = ws.Cell(row, 10).GetValue<int>(),
                        CustomerTypePrice = new List<CustomerTypeExcelRowDto>()
                    };

                    var grouped = priceColumns
                        .GroupBy(x => x.CustomerTypeName, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var group in grouped)
                    {
                        var priceDto = new CustomerTypeExcelRowDto
                        {
                            CustomerType = group.Key,
                            Price9 = null,
                            Price18 = 0m,
                            Price27 = null,
                            Price36 = null
                        };

                        var hasAny = false;

                        foreach (var colMeta in group)
                        {
                            var cell = ws.Cell(row, colMeta.ColumnIndex);
                            if (cell.IsEmpty()) continue;

                            var value = ExcelHelper.ReadDecimal(cell);
                            hasAny = true;

                            switch (colMeta.Holes)
                            {
                                case 9:
                                    priceDto.Price9 = value;
                                    break;
                                case 18:
                                    priceDto.Price18 = value;
                                    break;
                                case 27:
                                    priceDto.Price27 = value;
                                    break;
                                case 36:
                                    priceDto.Price36 = value;
                                    break;
                            }
                        }

                        if (hasAny)
                        {
                            dto.CustomerTypePrice.Add(priceDto);
                        }
                    }

                    results.Add((row, dto));
                }
                catch (Exception ex)
                {
                    throw ErrorHelper.ImportError(
                        _l,
                        CalendarSlotErrorCodes.UnknownRowError,
                        row,
                        ex.Message,
                        ex.StackTrace
                    );
                }

                row++;
            }

            return results;
        }

        private static List<PriceColumnMeta> ReadPriceColumns(IXLWorksheet ws)
        {
            const int customerTypeHeaderRow = 2;
            const int priceHeaderRow = 3;
            const int startCol = 11;

            var metas = new List<PriceColumnMeta>();
            var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? startCol;

            string? currentCustomerTypeName = null;

            for (int col = startCol; col <= lastCol; col++)
            {
                var customerTypeName = ws.Cell(customerTypeHeaderRow, col).GetString()?.Trim();
                var priceHeader = ws.Cell(priceHeaderRow, col).GetString()?.Trim();

                // Row 2 đang merge theo nhóm customer type.
                // Các cột sau trong cùng group có thể rỗng, nên dùng lại tên gần nhất bên trái.
                if (!string.IsNullOrWhiteSpace(customerTypeName))
                {
                    currentCustomerTypeName = customerTypeName;
                }
                else
                {
                    customerTypeName = currentCustomerTypeName;
                }

                if (string.IsNullOrWhiteSpace(customerTypeName) || string.IsNullOrWhiteSpace(priceHeader))
                    continue;

                var holes = ResolveHoles(priceHeader);
                if (holes == null) continue;

                metas.Add(new PriceColumnMeta
                {
                    ColumnIndex = col,
                    CustomerTypeName = customerTypeName,
                    Holes = holes.Value
                });
            }

            return metas;
        }

        private static int? ResolveHoles(string header)
        {
            var s = (header ?? "").Trim();

            if (s.Contains("36")) return 36;
            if (s.Contains("27")) return 27;
            if (s.Contains("18")) return 18;
            if (s.Contains("9")) return 9;

            return null;
        }

        private static bool IsDataRowEmpty(IXLWorksheet ws, int row)
        {
            // Chỉ coi là hết dữ liệu khi toàn bộ phần cột cố định A..J đều trống
            for (int col = 1; col <= 10; col++)
            {
                if (!ws.Cell(row, col).IsEmpty())
                    return false;
            }

            return true;
        }
    }

    internal static class TupleExt
    {
        public static AppCalendarSlotExcelRowDto Data(this (int Row, AppCalendarSlotExcelRowDto Data) x) => x.Data;
    }
}