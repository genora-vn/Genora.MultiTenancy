using ClosedXML.Excel;
using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppBookings;

public class AppBookingExcelImporter : ITransientDependency
{
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    private const string ExpectedPlayDateFormat = "dd/MM/yyyy HH:mm:ss";

    public AppBookingExcelImporter(IStringLocalizer<MultiTenancyResource> l)
    {
        _l = l;
    }

    public List<(int Row, AppBookingExcelRowDto Data)> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var results = new List<(int, AppBookingExcelRowDto)>();

        var row = 3;

        while (true)
        {
            var playDateCell = ws.Cell(row, 2);
            var playDateRaw = (playDateCell.GetString() ?? "").Trim();

            if (string.IsNullOrWhiteSpace(playDateRaw))
                break;

            try
            {
                if (!TryParsePlayDate(playDateRaw, out var playDate))
                {
                    throw ErrorHelper.ImportError(
                            _l,
                            BookingImportErrorCodes.PlayDateInvalidFormat,
                            row,
                            field: "PlayDate",
                            value: playDateRaw,
                            detailCode: BookingImportErrorCodes.PlayDateInvalidFormat + "_Data",
                            detailArgs: new { ExpectedFormat = ExpectedPlayDateFormat, Value = playDateRaw }
                        )
                        .WithData("ExpectedFormat", ExpectedPlayDateFormat);
                }

                var golfersCell = ws.Cell(row, 3);
                var golfersRaw = (golfersCell.GetString() ?? "").Trim();
                int golfers = 0;

                if (!int.TryParse(golfersRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out golfers) || golfers <= 0)
                {
                    if (string.IsNullOrWhiteSpace(golfersRaw))
                    {
                        try
                        {
                            var v = golfersCell.GetValue<double>();
                            golfers = Convert.ToInt32(v);
                        }
                        catch { /* ignore */ }
                    }

                    if (golfers <= 0)
                    {
                        throw ErrorHelper.ImportError(
                            _l,
                            BookingImportErrorCodes.NumberOfGolfersInvalid,
                            row,
                            field: "NumberOfGolfers",
                            value: golfersRaw,
                            detailCode: BookingImportErrorCodes.NumberOfGolfersInvalid + "_Data",
                            detailArgs: new { Value = golfersRaw }
                        );
                    }
                }

                var amountCell = ws.Cell(row, 4);
                var amountRaw = (amountCell.GetString() ?? "").Trim();
                decimal totalAmount = 0m;

                if (!TryParseDecimalFlexible(amountRaw, out totalAmount) || totalAmount <= 0)
                {
                    if (string.IsNullOrWhiteSpace(amountRaw))
                    {
                        try { totalAmount = amountCell.GetValue<decimal>(); }
                        catch { /* ignore */ }
                    }

                    if (totalAmount <= 0)
                    {
                        throw ErrorHelper.ImportError(
                            _l,
                            BookingImportErrorCodes.TotalAmountInvalid,
                            row,
                            field: "TotalAmount",
                            value: amountRaw,
                            detailCode: BookingImportErrorCodes.TotalAmountInvalid + "_Data",
                            detailArgs: new { Value = amountRaw }
                        );
                    }
                }

                var paymentMethod = (ws.Cell(row, 5).GetString() ?? "").Trim();
                if (string.IsNullOrWhiteSpace(paymentMethod))
                {
                    throw ErrorHelper.ImportError(
                        _l,
                        BookingImportErrorCodes.PaymentMethodRequired,
                        row,
                        field: "PaymentMethod",
                        detailCode: BookingImportErrorCodes.PaymentMethodRequired + "_Data"
                    );
                }

                var status = (ws.Cell(row, 6).GetString() ?? "").Trim();
                if (string.IsNullOrWhiteSpace(status))
                {
                    throw ErrorHelper.ImportError(
                        _l,
                        BookingImportErrorCodes.StatusRequired,
                        row,
                        field: "Status",
                        detailCode: BookingImportErrorCodes.StatusRequired + "_Data"
                    );
                }

                var source = (ws.Cell(row, 7).GetString() ?? "").Trim();
                if (string.IsNullOrWhiteSpace(source))
                {
                    throw ErrorHelper.ImportError(
                        _l,
                        BookingImportErrorCodes.SourceRequired,
                        row,
                        field: "Source",
                        detailCode: BookingImportErrorCodes.SourceRequired + "_Data"
                    );
                }

                var dto = new AppBookingExcelRowDto
                {
                    PlayDate = playDate,
                    NumberOfGolfers = golfers,
                    TotalAmount = totalAmount,
                    PaymentMethod = paymentMethod,
                    Status = status,
                    Source = source
                };

                results.Add((row, dto));
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.UnknownRowError, row)
                    .WithData("ExceptionMessage", ex.Message);
            }

            row++;
        }

        return results;
    }

    private static bool TryParsePlayDate(string raw, out DateTime value)
    {
        var formats = new[]
        {
            "dd/MM/yyyy HH:mm:ss",
            "d/M/yyyy HH:mm:ss",
            "dd/MM/yyyy H:mm:ss",
            "d/M/yyyy H:mm:ss",
            "dd/MM/yyyy"
        };

        return DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out value);
    }

    private static bool TryParseDecimalFlexible(string raw, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;

        var vi = CultureInfo.GetCultureInfo("vi-VN");
        if (decimal.TryParse(raw, NumberStyles.Any, vi, out value))
            return true;

        var cleaned = raw.Replace(",", "").Replace(" ", "");
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
