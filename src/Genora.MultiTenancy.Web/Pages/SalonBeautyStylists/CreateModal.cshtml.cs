using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyStylists;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateSalonBeautyStylistDto Stylist { get; set; } = new();

    public List<SelectListItem> GenderItems { get; set; } = new();
    public List<SelectListItem> RoleItems { get; set; } = new();
    public List<SelectListItem> LevelItems { get; set; } = new();
    public List<SelectListItem> LocationItems { get; set; } = new();

    private readonly ISalonBeautyStylistAppService _stylistAppService;
    private readonly ISalonBeautyLocationAppService _locationAppService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public CreateModalModel(
        ISalonBeautyStylistAppService stylistAppService,
        ISalonBeautyLocationAppService locationAppService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _stylistAppService = stylistAppService;
        _locationAppService = locationAppService;
        _l = l;
    }

    public async Task OnGetAsync()
    {
        Stylist = new CreateSalonBeautyStylistDto
        {
            Status = 1,
            IsShowOnApp = false,
            IsUploadImage = false,
            ExperienceYear = 0,
            SortOrder = 0
        };

        await BuildLocationItemsAsync();
        if (LocationItems.Count > 0 && !Stylist.LocationId.HasValue
            && Guid.TryParse(LocationItems[0].Value, out var firstId))
        {
            Stylist.LocationId = firstId;
        }
        BuildSelectLists();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await BuildLocationItemsAsync();
        BuildSelectLists();

        ValidateStylistInput(Stylist.DisplayName, Stylist.Phone, Stylist.Role, Stylist.Level, Stylist.ExperienceYear, Stylist.IsShowOnApp, Stylist.Avatar, Stylist.Images);

        if (!ModelState.IsValid)
            return Page();

        await _stylistAppService.CreateAsync(Stylist);
        return NoContent();
    }

    private async Task BuildLocationItemsAsync()
    {
        var locations = await _locationAppService.GetLookupAsync();
        LocationItems = locations
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), Stylist.LocationId == x.Id))
            .ToList();
    }

    private void ValidateStylistInput(string? displayName, string? phone, byte? role, byte? level, int experienceYear, bool isShowOnApp, string? avatar, IRemoteStreamContent? imageFile)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            ModelState.AddModelError("Stylist.DisplayName", _l["SalonBeautyStylists:DisplayNameRequired"]);

        if (!string.IsNullOrWhiteSpace(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone.Trim(), @"^0\d{9,10}$"))
            ModelState.AddModelError("Stylist.Phone", _l["SalonBeautyStylists:PhoneInvalid"]);

        if (!role.HasValue)
            ModelState.AddModelError("Stylist.Role", _l["SalonBeautyStylists:RoleRequired"]);

        if (!level.HasValue)
            ModelState.AddModelError("Stylist.Level", _l["SalonBeautyStylists:LevelRequired"]);

        if (experienceYear < 0 || experienceYear > 50)
            ModelState.AddModelError("Stylist.ExperienceYear", _l["SalonBeautyStylists:ExperienceInvalid"]);

        if (isShowOnApp && string.IsNullOrWhiteSpace(avatar) && (imageFile == null || (imageFile.ContentLength ?? 0) <= 0))
            ModelState.AddModelError("Stylist.Avatar", _l["SalonBeautyStylists:ShowOnAppRequiresAvatar"]);
    }

    private void BuildSelectLists()
    {
        GenderItems = Enum.GetValues(typeof(SalonBeautyGender))
            .Cast<SalonBeautyGender>()
            .Select(x => new SelectListItem(EnumText(x), ((byte)x).ToString(), Stylist.Gender == (byte)x))
            .ToList();

        RoleItems = Enum.GetValues(typeof(SalonBeautyStylistRole))
            .Cast<SalonBeautyStylistRole>()
            .Select(x => new SelectListItem(EnumText(x), ((byte)x).ToString(), Stylist.Role == (byte)x))
            .ToList();

        LevelItems = Enum.GetValues(typeof(SalonBeautyStylistLevel))
            .Cast<SalonBeautyStylistLevel>()
            .Select(x => new SelectListItem(EnumText(x), ((byte)x).ToString(), Stylist.Level == (byte)x))
            .ToList();
    }

    private string EnumText<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var key = $"Enum:{typeof(TEnum).Name}.{value}";
        var text = _l[key].Value;
        return string.IsNullOrWhiteSpace(text) || text.Equals(key, StringComparison.OrdinalIgnoreCase)
            ? value.ToString()
            : text;
    }
}
