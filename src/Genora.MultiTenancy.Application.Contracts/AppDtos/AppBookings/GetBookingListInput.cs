using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class GetBookingListInput : PagedAndSortedResultRequestDto
{
    public string FilterText { get; set; }

    public Guid? CustomerId { get; set; }
    public Guid? GolfCourseId { get; set; }

    public BookingStatus? Status { get; set; }
    public BookingSource? Source { get; set; }

    public DateTime? PlayDateFrom { get; set; }
    public DateTime? PlayDateTo { get; set; }
}