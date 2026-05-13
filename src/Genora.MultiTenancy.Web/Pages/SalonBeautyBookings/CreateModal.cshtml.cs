using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyBookings;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateSalonBeautyBookingDto Booking { get; set; } = new();

    private readonly ISalonBeautyBookingAppService _bookingService;

    public CreateModalModel(ISalonBeautyBookingAppService bookingService)
    {
        _bookingService = bookingService;
    }

    public void OnGet()
    {
        Booking = new CreateSalonBeautyBookingDto
        {
            BookingDate = DateTime.Today,
            StartTime = new TimeSpan(9, 0, 0),
            Items = new List<CreateSalonBeautyBookingItemDto>()
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Booking.CustomerId == Guid.Empty)
        {
            ModelState.AddModelError("Booking.CustomerId", "Vui lòng chọn khách hàng.");
        }
        if (Booking.Items == null || Booking.Items.Count == 0)
        {
            ModelState.AddModelError("Booking.Items", "Vui lòng chọn ít nhất một dịch vụ.");
        }

        if (!ModelState.IsValid) return Page();

        await _bookingService.CreateAsync(Booking);
        return NoContent();
    }
}
