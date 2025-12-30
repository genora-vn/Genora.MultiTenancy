using System;
using System.Collections.Generic;
using System.Linq;

namespace Genora.MultiTenancy.Enums
{
    public class OptionExtendTypeEnum : Enumeration
    {
        public static OptionExtendTypeEnum GolfCourseUlitity = new OptionExtendTypeEnum(1, "GolfCourseUlitity");
        public static OptionExtendTypeEnum CustomerSourse = new OptionExtendTypeEnum(2, "CustomerSourse");
        
        protected OptionExtendTypeEnum() { }

        public OptionExtendTypeEnum(int value, string name) : base(value, name)
        {
        }
        public static IEnumerable<OptionExtendTypeEnum> List() => new[] { GolfCourseUlitity, CustomerSourse };

        public static OptionExtendTypeEnum FromName(string name)
        {
            var state = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return state;
        }

        public static OptionExtendTypeEnum From(int value)
        {
            var state = List().SingleOrDefault(s => s.Value == value);
            return state;
        }
    }
}
