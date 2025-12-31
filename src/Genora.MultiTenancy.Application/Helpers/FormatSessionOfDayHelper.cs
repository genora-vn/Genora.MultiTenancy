using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.Helpers
{
    public class FormatSessionOfDayHelper
    {
        public static SessionOfDayEnum DateTimeToSessionOfDay(TimeSpan date)
        {
            int hour = date.Hours;
            if (hour >= 5 && hour < 11)
                return SessionOfDayEnum.Morning;
            else if (hour >= 11 && hour < 13)
                return SessionOfDayEnum.Noon;
            else if (hour >= 13 && hour < 18)
                return SessionOfDayEnum.Afternoon;
            else
                return SessionOfDayEnum.Evening;
        }
    }
}
