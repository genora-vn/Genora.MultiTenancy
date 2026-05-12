using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyServices;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateSalonBeautyServiceDto Service { get; set; } = new();

    public List<SelectListItem> CategoryItems { get; set; } = new();
    public List<SelectListItem> RoleItems { get; set; } = new();
    public List<SelectListItem> LevelItems { get; set; } = new();

    private readonly ISalonBeautyServiceAppService _serviceAppService;
    private readonly ISalonBeautyServiceCategoryAppService _categoryAppService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public CreateModalModel(
        ISalonBeautyServiceAppService serviceAppService,
        ISalonBeautyServiceCategoryAppService categoryAppService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _serviceAppService = serviceAppService;
        _categoryAppService = categoryAppService;
        _l = l;
    }

    public async Task OnGetAsync()
    {
        Service = new CreateSalonBeautyServiceDto
        {
            Status = 1,
            IsShowOnApp = true,
            SortOrder = 1,
            Duration = 60,
            Price = 0,
            ApplicableRole = (byte)Enum.GetValues(typeof(SalonBeautyStylistRole)).Cast<SalonBeautyStylistRole>().First(),
            ApplicableLevel = (byte)Enum.GetValues(typeof(SalonBeautyStylistLevel)).Cast<SalonBeautyStylistLevel>().First()
        };

        await BuildSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await BuildSelectListsAsync();
        ValidateServiceInput(Service.Name, Service.CategoryId, Service.Price, Service.Duration, Service.ApplicableRole, Service.ApplicableLevel, Service.Status, Service.IsShowOnApp, Service.SortOrder);

        if (!ModelState.IsValid)
            return Page();

        await _serviceAppService.CreateAsync(Service);
        return NoContent();
    }

    private void ValidateServiceInput(string? name, Guid categoryId, decimal price, int duration, byte? role, byte? level, byte status, bool isShowOnApp, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            ModelState.AddModelError("Service.Name", _l["SalonBeautyServices:NameRequired"]);

        if (categoryId == Guid.Empty)
            ModelState.AddModelError("Service.CategoryId", _l["SalonBeautyServices:CategoryRequired"]);

        if (price < 0)
            ModelState.AddModelError("Service.Price", _l["SalonBeautyServices:PriceInvalid"]);

        if (duration <= 0)
            ModelState.AddModelError("Service.Duration", _l["SalonBeautyServices:DurationInvalid"]);

        if (!role.HasValue)
            ModelState.AddModelError("Service.ApplicableRole", _l["SalonBeautyServices:RoleRequired"]);

        if (!level.HasValue)
            ModelState.AddModelError("Service.ApplicableLevel", _l["SalonBeautyServices:LevelRequired"]);

        if (status != 0 && status != 1)
            ModelState.AddModelError("Service.Status", _l["SalonBeautyServices:StatusInvalid"]);

        if (isShowOnApp && status != 1)
            ModelState.AddModelError("Service.IsShowOnApp", _l["SalonBeautyServices:ShowOnAppRequiresActive"]);

        if (sortOrder < 0)
            ModelState.AddModelError("Service.SortOrder", _l["SalonBeautyServices:SortOrderInvalid"]);
    }

    private async Task BuildSelectListsAsync()
    {
        var categories = await _categoryAppService.GetListAsync(new GetSalonBeautyListInput
        {
            MaxResultCount = 1000,
            Sorting = "SortOrder asc, Name asc"
        });

        CategoryItems = categories.Items
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), Service.CategoryId == x.Id))
            .ToList();

        RoleItems = Enum.GetValues(typeof(SalonBeautyStylistRole))
            .Cast<SalonBeautyStylistRole>()
            .Select(x => new SelectListItem(EnumText(x), ((byte)x).ToString(), Service.ApplicableRole == (byte)x))
            .ToList();

        LevelItems = Enum.GetValues(typeof(SalonBeautyStylistLevel))
            .Cast<SalonBeautyStylistLevel>()
            .Select(x => new SelectListItem(EnumText(x), ((byte)x).ToString(), Service.ApplicableLevel == (byte)x))
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
