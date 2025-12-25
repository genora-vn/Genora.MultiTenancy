using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppBookings;

public class EditModalModel : PageModel
{
    [HiddenInput]
    [BindProperty]
    public Guid Id { get; set; }

    // DTO dùng để submit update
    [BindProperty]
    public CreateUpdateAppBookingDto Booking { get; set; }

    // DTO hiển thị thông tin đầy đủ (bao gồm BookingCode, CustomerName, GolfCourseName...)
    public AppBookingDto BookingView { get; set; }

    public List<SelectListItem> StatusItems { get; set; } = new();
    public List<SelectListItem> PaymentMethodItems { get; set; } = new();
    public List<SelectListItem> SourceItems { get; set; } = new();

    private readonly IAppBookingService _bookingService;
    private readonly IStringLocalizer<MultiTenancyResource> _L;
    public EditModalModel(IAppBookingService bookingService, IStringLocalizer<MultiTenancyResource> L)
    {
        _bookingService = bookingService;
        _L = L;
    }

    public async Task OnGetAsync(Guid id)
    {
        var dto = await _bookingService.GetAsync(id);

        Id = id;
        BookingView = dto;

        Booking = new CreateUpdateAppBookingDto
        {
            CustomerId = dto.CustomerId,
            PlayDate = dto.PlayDate,
            GolfCourseId = dto.GolfCourseId,
            CalendarSlotId = dto.CalendarSlotId,
            NumberOfGolfers = dto.NumberOfGolfers,
            PricePerGolfer = dto.PricePerGolfer,
            TotalAmount = dto.TotalAmount,
            PaymentMethod = dto.PaymentMethod,
            Status = dto.Status,
            Source = dto.Source,
            Utilities = dto.Utilities,
            IsExportInvoice = dto.IsExportInvoice,
            NumberHoles = dto.NumberHoles,
            // ⭐ Map Players sang DTO để cho phép sửa
            Players = dto.Players?.ConvertAll(p => new CreateUpdateBookingPlayerDto
            {
                CustomerId = p.CustomerId,
                PlayerName = p.PlayerName,
                VgaCode = p.VgaCode,
                PricePerPlayer = dto.TotalAmount / dto.NumberOfGolfers,
                Notes = p.Notes
            }) ?? new List<CreateUpdateBookingPlayerDto>()
        };

        BuildSelectItems();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            BuildSelectItems();

            // load lại view info
            BookingView = await _bookingService.GetAsync(Id);
            return Page();
        }
        if (Booking.Players.Count != Booking.NumberOfGolfers)
        {
            ModelState.AddModelError("Booking.NumberOfGolfers", _L["NotMatchPlayerError"]);
            BuildSelectItems();
            // load lại view info
            BookingView = await _bookingService.GetAsync(Id);
            return Page();
        }
        await _bookingService.UpdateAsync(Id, Booking);
        return new NoContentResult();
    }

    private void BuildSelectItems()
    {
        StatusItems = new List<SelectListItem>
            {
                new("Processing", ((int)BookingStatus.Processing).ToString()),
                new("Confirmed", ((int)BookingStatus.Confirmed).ToString()),
                new("Paid", ((int)BookingStatus.Paid).ToString()),
                new("Completed", ((int)BookingStatus.Completed).ToString()),
                new("Cancelled (refund)", ((int)BookingStatus.CancelledRefund).ToString()),
                new("Cancelled (no refund)", ((int)BookingStatus.CancelledNoRefund).ToString())
            };

        PaymentMethodItems = new List<SelectListItem>
            {
                new("COD", ((int)PaymentMethod.COD).ToString()),
                new("Online", ((int)PaymentMethod.Online).ToString()),
                new("Bank transfer", ((int)PaymentMethod.BankTransfer).ToString())
            };

        SourceItems = new List<SelectListItem>
            {
                new("Mini App", ((int)BookingSource.MiniApp).ToString()),
                new("Hotline", ((int)BookingSource.Hotline).ToString()),
                new("Agent", ((int)BookingSource.Agent).ToString())
            };
    }
}