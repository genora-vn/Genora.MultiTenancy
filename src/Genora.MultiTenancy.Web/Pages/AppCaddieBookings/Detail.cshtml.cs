using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieBookings;

public class DetailModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public CaddieBookingDto Booking { get; set; } = null!;
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

    private readonly CaddieBookingAppService _bookingService;
    private readonly IAuthorizationService _authorizationService;

    public DetailModel(
        CaddieBookingAppService bookingService,
        IAuthorizationService authorizationService)
    {
        _bookingService = bookingService;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Booking = await _bookingService.GetAsync(Id);

        CanEdit = CurrentTenant.IsAvailable
            ? (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.AppCaddieBookings.Edit)).Succeeded
            : (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.HostAppCaddieBookings.Edit)).Succeeded;

        CanDelete = CurrentTenant.IsAvailable
            ? (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.AppCaddieBookings.Delete)).Succeeded
            : (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.HostAppCaddieBookings.Delete)).Succeeded;

        return Page();
    }
}
