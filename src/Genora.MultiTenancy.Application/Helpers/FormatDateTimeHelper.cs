using System;

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
    }
}
