using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class GetMiniAppBookingListInput : PagedAndSortedResultRequestDto
{
    // bắt buộc: lọc theo customer đang login trên mini app
    public Guid CustomerId { get; set; }

    // optional filters
    public DateTime? PlayDateFrom { get; set; }
    public DateTime? PlayDateTo { get; set; }

    public BookingStatus? Status { get; set; }
}