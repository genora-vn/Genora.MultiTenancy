using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyLocations;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateSalonBeautyLocationDto Location { get; set; } = new();

    private readonly ISalonBeautyLocationAppService _locationAppService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public CreateModalModel(
        ISalonBeautyLocationAppService locationAppService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _locationAppService = locationAppService;
        _l = l;
    }

    public void OnGet()
    {
        Location = new CreateSalonBeautyLocationDto
        {
            IsActive = true,
            IsShowOnApp = false,
            IsUploadImage = false,
            OpenTime = new TimeSpan(8, 0, 0),
            CloseTime = new TimeSpan(21, 0, 0),
            SortOrder = 0
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateLocationInput(Location.Name, Location.Address, Location.Phone, Location.OpenTime, Location.CloseTime, Location.SortOrder, Location.IsActive, Location.IsShowOnApp, Location.ImageUrl, Location.Images);

        if (!ModelState.IsValid)
            return Page();

        await _locationAppService.CreateAsync(Location);
        return NoContent();
    }

    private void ValidateLocationInput(string? name, string? address, string? phone, TimeSpan openTime, TimeSpan closeTime, int sortOrder, bool isActive, bool isShowOnApp, string? imageUrl, IRemoteStreamContent? imageFile)
    {
        if (string.IsNullOrWhiteSpace(name))
            ModelState.AddModelError("Location.Name", _l["SalonBeautyLocations:NameRequired"]);

        if (string.IsNullOrWhiteSpace(address))
            ModelState.AddModelError("Location.Address", _l["SalonBeautyLocations:AddressRequired"]);

        if (!string.IsNullOrWhiteSpace(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone.Trim(), @"^0\d{9,10}$"))
            ModelState.AddModelError("Location.Phone", _l["SalonBeautyLocations:PhoneInvalid"]);

        if (openTime >= closeTime)
            ModelState.AddModelError("Location.OpenTime", _l["SalonBeautyLocations:OpenCloseInvalid"]);

        if (sortOrder < 0)
            ModelState.AddModelError("Location.SortOrder", _l["SalonBeautyLocations:SortOrderInvalid"]);

        if (isShowOnApp && !isActive)
            ModelState.AddModelError("Location.IsShowOnApp", _l["SalonBeautyLocations:ShowOnAppRequiresActive"]);
    }
}
