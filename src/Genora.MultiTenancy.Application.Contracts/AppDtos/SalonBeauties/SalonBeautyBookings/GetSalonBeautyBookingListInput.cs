using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class GetSalonBeautyBookingListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StylistId { get; set; }
    public byte? Status { get; set; }
    public byte? PaymentStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}