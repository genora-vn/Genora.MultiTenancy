using ClosedXML.Excel;
using Genora.MultiTenancy.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppCustomers;

public class AppCustomerExcelImporter : ITransientDependency
{
    public List<(int Row, string FullName, string? VgaCode, DateTime? DateOfBirth, string PhoneNumber, string? Email)> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var results = new List<(int, string, string?, DateTime?, string, string?)>();

        var row = 3;

        while (!ws.Cell(row, 1).IsEmpty())
        {
            try
            {
                var fullName = (ws.Cell(row, 1).GetString() ?? "").Trim();
                var vgaCode = (ws.Cell(row, 2).GetString() ?? "").Trim();

                var phone = ReadPhoneCell(ws.Cell(row, 4));
                var email = (ws.Cell(row, 5).GetString() ?? "").Trim();

                DateTime? dob = null;
                if (!ws.Cell(row, 3).IsEmpty())
                {
                    var d = ExcelHelper.ReadDate(ws.Cell(row, 3));
                    dob = d == DateTime.MinValue ? null : d.Date;
                }

                results.Add((
                    row,
                    fullName,
                    string.IsNullOrWhiteSpace(vgaCode) ? null : vgaCode,
                    dob,
                    phone,
                    string.IsNullOrWhiteSpace(email) ? null : email
                ));
            }
            catch (Exception ex)
            {
                throw new BusinessException("Customer:ImportUnknownRowError")
                    .WithData("RowNumber", row)
                    .WithData("ExceptionMessage", ex.Message);
            }

            row++;
        }

        return results;
    }

    private static string ReadPhoneCell(IXLCell cell)
    {
        var s = (cell.GetString() ?? "").Trim();

        if (string.IsNullOrWhiteSpace(s) && cell.DataType == XLDataType.Number)
        {
            var dbl = cell.GetDouble();
            s = Convert.ToInt64(Math.Round(dbl)).ToString(CultureInfo.InvariantCulture);
        }

        s = Regex.Replace(s, @"[\s\.\-\(\)]", "");

        if (!s.StartsWith("0") && !s.StartsWith("84") && !s.StartsWith("+84"))
        {
            if (Regex.IsMatch(s, @"^\d{9,10}$"))
                s = "0" + s;
        }

        return s;
    }
}
