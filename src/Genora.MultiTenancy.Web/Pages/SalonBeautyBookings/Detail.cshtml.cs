using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyBookings;

public class DetailModel : MultiTenancyPageModel
{
    public SalonBeautyBookingDetailDto Booking { get; set; } = null!;
    public bool CanEdit { get; set; }
    public bool CanCancel { get; set; }
    public bool CanCheckin { get; set; }
    public bool CanUpdatePayment { get; set; }

    private readonly ISalonBeautyBookingAppService _bookingService;
    private readonly IAuthorizationService _authorizationService;

    public DetailModel(
        ISalonBeautyBookingAppService bookingService,
        IAuthorizationService authorizationService)
    {
        _bookingService = bookingService;
        _authorizationService = authorizationService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Booking = await _bookingService.GetAsync(id);
        CanEdit = (await _authorizationService.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyBookings.Edit));
        CanCancel = (await _authorizationService.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyBookings.Cancel));
        CanCheckin = (await _authorizationService.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyBookings.Checkin));
        CanUpdatePayment = (await _authorizationService.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyBookings.UpdatePayment));
    }

    public int GetCurrentStep()
    {
        return Booking.Status switch
        {
            Genora.MultiTenancy.Enums.SalonBeautyBookingStatus.New => 1,
            Genora.MultiTenancy.Enums.SalonBeautyBookingStatus.Confirmed => 2,
            Genora.MultiTenancy.Enums.SalonBeautyBookingStatus.Completed => 4,
            Genora.MultiTenancy.Enums.SalonBeautyBookingStatus.Cancelled => -1,
            _ => 1
        };
    }

    public bool IsCancelled() => Booking.Status == Genora.MultiTenancy.Enums.SalonBeautyBookingStatus.Cancelled;

    public string GetCustomerInitials()
    {
        if (string.IsNullOrWhiteSpace(Booking.CustomerName)) return "K";
        var parts = Booking.CustomerName.Trim().Split(' ');
        return parts[^1].Substring(0, 1).ToUpper();
    }
}
