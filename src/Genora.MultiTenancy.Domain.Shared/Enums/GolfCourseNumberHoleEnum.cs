using System;
using System.Collections.Generic;
using System.Linq;

namespace Genora.MultiTenancy.Enums
{
    public class GolfCourseNumberHoleEnum : Enumeration
    {
        public static GolfCourseNumberHoleEnum EightTeen = new GolfCourseNumberHoleEnum(18,"18 hố");
        public static GolfCourseNumberHoleEnum TwentySeven = new GolfCourseNumberHoleEnum(27, "27 hố");
        public static GolfCourseNumberHoleEnum ThirtySix = new GolfCourseNumberHoleEnum(36, "36 hố");
        public static GolfCourseNumberHoleEnum FiftyFour = new GolfCourseNumberHoleEnum(54, "54 hố");
        protected GolfCourseNumberHoleEnum() { }

        public GolfCourseNumberHoleEnum(int value, string name) : base(value, name)
        {
        }
        public static IEnumerable<GolfCourseNumberHoleEnum> List() => new[] { EightTeen, TwentySeven, ThirtySix, FiftyFour };

        public static GolfCourseNumberHoleEnum FromName(string name)
        {
            var state = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return state;
        }

        public static GolfCourseNumberHoleEnum From(int value)
        {
            var state = List().SingleOrDefault(s => s.Value == value);
            return state;
        }
    }
}
