using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Helpers;

public class ExcelHelper
{
    public static DateTime ReadDate(IXLCell cell)
    {
        // ưu tiên lấy DateTime nếu cell là dạng date
        if (cell.TryGetValue<DateTime>(out var dt))
            return dt.Date;

        var s = cell.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s))
            throw new Exception("Ngày không được để trống");

        return DateTime.ParseExact(s, "dd/MM/yyyy", new CultureInfo("vi-VN"));
    }

    public static TimeSpan ReadTime(IXLCell cell)
    {
        if (cell.TryGetValue<TimeSpan>(out var ts))
            return ts;

        // trường hợp excel lưu giờ dạng DateTime
        if (cell.TryGetValue<DateTime>(out var dt))
            return dt.TimeOfDay;

        var s = cell.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s))
            throw new Exception("Giờ không được để trống");

        // HH:mm hoặc H:mm
        if (TimeSpan.TryParse(s, out ts)) return ts;

        throw new Exception("Giờ không hợp lệ, dùng định dạng HH:mm");
    }

    public static decimal ReadDecimal(IXLCell cell)
    {
        if (cell.TryGetValue<decimal>(out var d))
            return d;

        var s = cell.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) return 0m;

        // xử lý 1.234.567 hoặc 1,234,567
        s = s.Replace(".", "").Replace(",", "");
        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
            return d;

        throw new Exception("Giá không hợp lệ");
    }
}
