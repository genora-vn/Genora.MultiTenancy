using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class CancelBookingDto
{
    public SalonBeautyCancelReason CancelReason { get; set; }
    public string? CancelNote { get; set; }
}