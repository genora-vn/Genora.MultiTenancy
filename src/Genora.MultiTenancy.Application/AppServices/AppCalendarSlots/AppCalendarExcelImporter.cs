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

        public List<(int Row, AppCalendarSlotExcelRowDto Data)> Read(
            Stream stream,
            List<CustomerType> customerTypes)
        {
            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);

            var results = new List<(int, AppCalendarSlotExcelRowDto)>();

            var row = 5;
            var totalCustomerTypes = customerTypes.Count;

            while (!ws.Cell(row, 1).IsEmpty())
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

                    if (results.Any(x =>
                        x.Item2.GolfCourseCode == dto.GolfCourseCode &&
                        x.Item2.FromDate.Date == dto.FromDate.Date &&
                        x.Item2.StartTime == dto.StartTime &&
                        string.Equals(x.Item2.DayType, dto.DayType, StringComparison.OrdinalIgnoreCase)))
                    {
                        row++;
                        continue;
                    }

                    var index = 0;
                    var startCol = 11;

                    for (var col = startCol; col < startCol + (totalCustomerTypes * 4); col += 4)
                    {
                        var hasAny =
                            !ws.Cell(row, col).IsEmpty() ||
                            !ws.Cell(row, col + 1).IsEmpty() ||
                            !ws.Cell(row, col + 2).IsEmpty() ||
                            !ws.Cell(row, col + 3).IsEmpty();

                        if (hasAny)
                        {
                            var price = new CustomerTypeExcelRowDto
                            {
                                CustomerType = customerTypes[index].Name,

                                Price9 = ws.Cell(row, col).IsEmpty()
                                    ? (decimal?)null
                                    : ExcelHelper.ReadDecimal(ws.Cell(row, col)),

                                Price18 = ws.Cell(row, col + 1).IsEmpty()
                                    ? 0m
                                    : ExcelHelper.ReadDecimal(ws.Cell(row, col + 1)),

                                Price27 = ws.Cell(row, col + 2).IsEmpty()
                                    ? (decimal?)null
                                    : ExcelHelper.ReadDecimal(ws.Cell(row, col + 2)),

                                Price36 = ws.Cell(row, col + 3).IsEmpty()
                                    ? (decimal?)null
                                    : ExcelHelper.ReadDecimal(ws.Cell(row, col + 3)),
                            };

                            dto.CustomerTypePrice.Add(price);
                        }

                        index++;
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
    }

    internal static class TupleExt
    {
        public static AppCalendarSlotExcelRowDto Data(this (int Row, AppCalendarSlotExcelRowDto Data) x) => x.Data;
    }
}
