using System;
using System.Collections.Generic;
using System.Linq;

namespace Genora.MultiTenancy.Enums
{
    public class SessionOfDayEnum : Enumeration
    {
        public static SessionOfDayEnum Morning = new SessionOfDayEnum(1, "Morning");
        public static SessionOfDayEnum Noon = new SessionOfDayEnum(2, "Noon");
        public static SessionOfDayEnum Afternoon = new SessionOfDayEnum(3, "Afternoon");
        public static SessionOfDayEnum Evening = new SessionOfDayEnum(4, "Evening");
        protected SessionOfDayEnum() { }

        public SessionOfDayEnum(int value, string name) : base(value, name)
        {
        }
        public static IEnumerable<SessionOfDayEnum> List() => new[] { Morning, Noon, Afternoon, Evening };

        public static SessionOfDayEnum FromName(string name)
        {
            var state = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return state;
        }

        public static SessionOfDayEnum From(int value)
        {
            var state = List().SingleOrDefault(s => s.Value == value);
            return state;
        }
    }
}
