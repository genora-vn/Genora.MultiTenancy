using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyBookings;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public UpdateSalonBeautyBookingDto Booking { get; set; } = new();

    public Guid BookingId { get; set; }
    public string? BookingCode { get; set; }
    public List<SelectListItem> StatusItems { get; set; } = new();

    private readonly ISalonBeautyBookingAppService _bookingService;

    public EditModalModel(ISalonBeautyBookingAppService bookingService)
    {
        _bookingService = bookingService;
    }

    public async Task OnGetAsync(Guid id)
    {
        BookingId = id;
        var detail = await _bookingService.GetAsync(id);
        BookingCode = detail.BookingCode;

        Booking = new UpdateSalonBeautyBookingDto
        {
            CustomerId = detail.CustomerId,
            StylistId = detail.StylistId,
            BookingDate = detail.BookingDate,
            StartTime = detail.StartTime,
            EndTime = detail.EndTime,
            Status = detail.Status,
            CustomerNote = detail.CustomerNote,
            InternalNote = detail.InternalNote,
            Items = new List<CreateSalonBeautyBookingItemDto>()
        };

        foreach (var item in detail.Items)
        {
            Booking.Items.Add(new CreateSalonBeautyBookingItemDto
            {
                ServiceId = item.ServiceId,
                StylistId = item.StylistId,
                Price = item.Price,
                Duration = item.Duration
            });
        }

        BuildStatusItems();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        BuildStatusItems();

        if (Booking.CustomerId == Guid.Empty)
            ModelState.AddModelError("Booking.CustomerId", "Vui lòng chọn khách hàng.");
        if (Booking.Items == null || Booking.Items.Count == 0)
            ModelState.AddModelError("Booking.Items", "Vui lòng chọn ít nhất một dịch vụ.");

        if (!ModelState.IsValid) return Page();

        await _bookingService.UpdateAsync(id, Booking);
        return NoContent();
    }

    private void BuildStatusItems()
    {
        StatusItems = new List<SelectListItem>
        {
            new("Chờ xác nhận", ((byte)SalonBeautyBookingStatus.New).ToString(), Booking.Status == SalonBeautyBookingStatus.New),
            new("Đã xác nhận", ((byte)SalonBeautyBookingStatus.Confirmed).ToString(), Booking.Status == SalonBeautyBookingStatus.Confirmed),
            new("Hoàn thành", ((byte)SalonBeautyBookingStatus.Completed).ToString(), Booking.Status == SalonBeautyBookingStatus.Completed),
            new("Đã hủy", ((byte)SalonBeautyBookingStatus.Cancelled).ToString(), Booking.Status == SalonBeautyBookingStatus.Cancelled),
        };
    }
}
