using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppBookings;
using System;
using System.Collections.Generic;
using System.IO;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppBookings;
public class AppBookingExcelImporter : ITransientDependency
{
    public List<(int Row, AppBookingExcelRowDto Data)> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var results = new List<(int, AppBookingExcelRowDto)>();

        // Bỏ qua row 1,2 bắt đầu từ row 3
        var row = 3;

        while (!ws.Cell(row, 1).IsEmpty())
        {
            try
            {
                var dto = new AppBookingExcelRowDto
                {
                    PlayDate = ws.Cell(row, 2).GetDateTime(),
                    NumberOfGolfers = ws.Cell(row, 3).GetValue<int>(),
                    TotalAmount = ws.Cell(row, 4).GetValue<decimal>(),
                    PaymentMethod = ws.Cell(row, 5).GetString(),
                    Status = ws.Cell(row, 6).GetString(),
                    Source = ws.Cell(row, 7).GetString()
                };

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