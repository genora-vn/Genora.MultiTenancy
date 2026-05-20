using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyBookings;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateSalonBeautyBookingDto Booking { get; set; } = new();

    public List<SelectListItem> LocationItems { get; set; } = new();

    private readonly ISalonBeautyBookingAppService _bookingService;
    private readonly ISalonBeautyLocationAppService _locationAppService;

    public CreateModalModel(
        ISalonBeautyBookingAppService bookingService,
        ISalonBeautyLocationAppService locationAppService)
    {
        _bookingService = bookingService;
        _locationAppService = locationAppService;
    }

    public async Task OnGetAsync()
    {
        Booking = new CreateSalonBeautyBookingDto
        {
            BookingDate = DateTime.Today,
            StartTime = new TimeSpan(9, 0, 0),
            Items = new List<CreateSalonBeautyBookingItemDto>()
        };

        await BuildLocationItemsAsync();
        if (LocationItems.Count > 0 && Guid.TryParse(LocationItems[0].Value, out var firstId))
        {
            Booking.LocationId = firstId;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await BuildLocationItemsAsync();

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

    private async Task BuildLocationItemsAsync()
    {
        var locations = await _locationAppService.GetLookupAsync();
        LocationItems = locations
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), Booking.LocationId == x.Id))
            .ToList();
    }
}
