using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
namespace Genora.MultiTenancy.Helpers;

public static class PriceByHoleHelper
{
    public static decimal GetPriceByNumberHoles(CalendarSlotPrice p, short? numberHoles)
    {
        return numberHoles switch
        {
            9 => p.Price9 ?? p.Price18,
            18 => p.Price18,
            27 => p.Price27 ?? p.Price18,
            36 => p.Price36 ?? p.Price18,
            _ => p.Price18
        };
    }

    public static decimal GetPriceByNumberHoles(AppCalendarSlotPriceDto p, short? numberHoles)
    {
        return numberHoles switch
        {
            9 => p.Price9 ?? p.Price18,
            18 => p.Price18,
            27 => p.Price27 ?? p.Price18,
            36 => p.Price36 ?? p.Price18,
            _ => p.Price18
        };
    }
}

