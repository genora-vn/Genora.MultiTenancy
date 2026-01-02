using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots
{
    public class AppCalendarExcelImporter : ITransientDependency
    {
        public List<(int Row, AppCalendarSlotExcelRowDto Data)> Read(Stream stream, List<CustomerType> customerTypes)
        {
            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);

            var results = new List<(int, AppCalendarSlotExcelRowDto)>();

            // Bỏ qua row 1,2 bắt đầu từ row 3
            var row = 4;
            var totalCustomerTypes = customerTypes.Count;
            while (!ws.Cell(row, 2).IsEmpty())
            {
                try
                {
                    var dto = new AppCalendarSlotExcelRowDto
                    {
                        GolfCourseCode = ws.Cell(row, 1).GetString(),
                        PlayDate = DateTime.ParseExact(ws.Cell(row, 2).GetString(), "dd/MM/yyyy", new CultureInfo("vi-VN")),
                        StartTime = ws.Cell(row, 3).GetValue<TimeSpan>(),
                        EndTime = ws.Cell(row, 4).GetValue<TimeSpan>(),
                        PromotionType = ws.Cell(row, 5).GetString(),
                        MaxSlots = ws.Cell(row, 6).GetValue<int>(),
                        InternalNote = ws.Cell(row, 7).GetString(),
                    };
                    if (results.Any(x => x.Item2.GolfCourseCode == dto.GolfCourseCode && x.Item2.PlayDate == dto.PlayDate && x.Item2.StartTime == dto.StartTime)) continue;
                    var index = 0;
                    var priceColsIndex = 8;
                    for (var priceCols = priceColsIndex; priceCols < (totalCustomerTypes + 8); priceCols++)
                    {
                        if (!ws.Cell(row, priceCols).IsEmpty())
                        {
                            var price = new CustomerTypeExcelRowDto
                            {
                                CustomerType = customerTypes[index].Name,
                                Price = ws.Cell(row, priceCols).GetValue<decimal>(),
                            };
                            dto.CustomerTypePrice.Add(price);
                        }
                        index++;
                    }
                    
                    results.Add((row, dto));
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException(
                        "Import Excel lỗi",
                        $"Dòng {row}: {ex.Message}"
                    );
                }

                row++;
            }

            return results;
        }
    }
}
