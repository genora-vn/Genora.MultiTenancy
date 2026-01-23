using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Genora.MultiTenancy.Helpers
{
    public class FormatDateTimeHelper
    {
        private static readonly string[] VietnameseDays = new string[]
    {
        "Chủ Nhật",   // Sunday
        "Thứ Hai",    // Monday
        "Thứ Ba",     // Tuesday
        "Thứ Tư",     // Wednesday
        "Thứ Năm",    // Thursday
        "Thứ Sáu",    // Friday
        "Thứ Bảy"     // Saturday
    };

        /// <summary>
        /// Trả về tên thứ đầy đủ bằng tiếng Việt
        /// </summary>
        public static string GetVietnameseDayOfWeek(DateTime date)
        {
            int index = (int)date.DayOfWeek;
            return VietnameseDays[index];
        }

        public static List<DateTime>? DeserializeDates(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                var arr = JsonSerializer.Deserialize<List<string>>(json);
                if (arr == null) return null;

                var result = new List<DateTime>();
                foreach (var s in arr)
                {
                    if (DateTime.TryParse(s, out var d))
                        result.Add(d.Date);
                }
                return result.Count == 0 ? null : result;
            }
            catch
            {
                return null;
            }
        }

        public static string? SerializeDates(List<DateTime>? dates)
        {
            if (dates == null || dates.Count == 0) return null;

            // chuẩn hoá date-only để đồng bộ
            var arr = dates
                .Select(x => x.Date.ToString("yyyy-MM-dd"))
                .Distinct()
                .ToList();

            return JsonSerializer.Serialize(arr);
        }
    }
}