using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class UpdateBookingPaymentDto
{
    public SalonBeautyPaymentStatus PaymentStatus { get; set; }
    public SalonBeautyPaymentMethod? PaymentMethod { get; set; }
}