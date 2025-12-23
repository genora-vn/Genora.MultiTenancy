using System;
using System.Collections.Generic;
using System.Linq;

namespace Genora.MultiTenancy.Enums
{
    public class UlititiesEnum : Enumeration
    {
        public static UlititiesEnum Caddie = new UlititiesEnum(1, "RentCaddie", "fa fa-user");
        public static UlititiesEnum Clothes = new UlititiesEnum(2, "RentClothes", "fa fa-tshirt");
        public static UlititiesEnum GolfClubs = new UlititiesEnum(3, "RentGolfClubs", "fa fa-golf-club");
        public static UlititiesEnum Lunch = new UlititiesEnum(4, "Lunch", "fa fa-cutlery");
        public static UlititiesEnum StayPlay = new UlititiesEnum(5, "StayPlay", "fa fa-cutlery");

        public string Icon { get; set; }

        protected UlititiesEnum() { }

        public UlititiesEnum(int value, string name, string icon) : base(value, name)
        {
            Icon = icon;
        }
        public static IEnumerable<UlititiesEnum> List() => new[] { Caddie, Clothes, GolfClubs, StayPlay };

        public static UlititiesEnum FromName(string name)
        {
            var state = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return state;
        }

        public static UlititiesEnum From(int value)
        {
            var state = List().SingleOrDefault(s => s.Value == value);
            return state;
        }
    }
}
