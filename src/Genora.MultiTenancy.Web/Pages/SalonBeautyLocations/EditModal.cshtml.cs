using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyLocations;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public EditLocationViewModel Location { get; set; } = new();

    private readonly ISalonBeautyLocationAppService _locationAppService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public EditModalModel(
        ISalonBeautyLocationAppService locationAppService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _locationAppService = locationAppService;
        _l = l;
    }

    public async Task OnGetAsync(Guid id)
    {
        var dto = await _locationAppService.GetAsync(id);
        Location = new EditLocationViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            OpenTime = dto.OpenTime,
            CloseTime = dto.CloseTime,
            ImageUrl = dto.ImageUrl,
            IsUploadImage = false,
            IsActive = dto.IsActive,
            IsShowOnApp = dto.IsShowOnApp,
            Note = dto.Note,
            SortOrder = dto.SortOrder
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateLocationInput(Location.Name, Location.Address, Location.Phone, Location.OpenTime, Location.CloseTime, Location.SortOrder, Location.IsActive, Location.IsShowOnApp, Location.ImageUrl, Location.Images);

        if (!ModelState.IsValid)
            return Page();

        var updateDto = new UpdateSalonBeautyLocationDto
        {
            Name = Location.Name,
            Address = Location.Address,
            Phone = Location.Phone,
            OpenTime = Location.OpenTime,
            CloseTime = Location.CloseTime,
            ImageUrl = Location.ImageUrl,
            Images = Location.Images,
            IsUploadImage = Location.IsUploadImage,
            IsActive = Location.IsActive,
            IsShowOnApp = Location.IsShowOnApp,
            Note = Location.Note,
            SortOrder = Location.SortOrder
        };

        await _locationAppService.UpdateAsync(Location.Id, updateDto);
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

public class EditLocationViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public string? ImageUrl { get; set; }
    public IRemoteStreamContent? Images { get; set; }
    public bool IsUploadImage { get; set; }
    public bool IsActive { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
