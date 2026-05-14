using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;

public class UpdateBookingStatusDto
{
    public SalonBeautyBookingStatus Status { get; set; }

    public string? Note { get; set; }

    public string? InternalNote { get; set; }

    public string? Reason { get; set; }
}
