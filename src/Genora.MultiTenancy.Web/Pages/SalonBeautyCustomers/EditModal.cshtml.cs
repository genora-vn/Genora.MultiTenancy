using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.SalonBeauty;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyCustomers;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateSalonBeautyCustomerDto Customer { get; set; } = new();

    public List<SelectListItem> GenderItems { get; set; } = new();
    public List<SelectListItem> SourceItems { get; set; } = new();

    private readonly ISalonBeautyCustomerAppService _customerAppService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public EditModalModel(
        ISalonBeautyCustomerAppService customerAppService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _customerAppService = customerAppService;
        _l = l;
    }

    public async Task OnGetAsync()
    {
        var dto = await _customerAppService.GetAsync(Id);
        Customer = new UpdateSalonBeautyCustomerDto
        {
            Name = dto.Name,
            Phone = dto.Phone ?? string.Empty,
            Email = dto.Email,
            Gender = dto.Gender,
            Birthday = dto.Birthday,
            Avatar = dto.Avatar,
            ZaloUserId = dto.ZaloUserId,
            IsFollowOa = dto.IsFollowOa,
            Source = dto.Source,
            Status = dto.Status,
            Note = dto.Note
        };

        BuildSelectLists();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        NormalizeBirthdayFromForm();
        BuildSelectLists();

        if (!ModelState.IsValid)
            return Page();

        await _customerAppService.UpdateAsync(Id, Customer);
        return NoContent();
    }


    private void NormalizeBirthdayFromForm()
    {
        const string key = "Customer.Birthday";

        if (!Request.HasFormContentType)
        {
            return;
        }

        var raw = Request.Form[key].FirstOrDefault()?.Trim();

        if (string.IsNullOrWhiteSpace(raw))
        {
            Customer.Birthday = null;
            ModelState.Remove(key);
            return;
        }

        var formats = new[]
        {
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "yyyy-MM-dd"
        };

        if (DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthday) ||
            DateTime.TryParse(raw, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out birthday))
        {
            Customer.Birthday = birthday.Date;
            ModelState.Remove(key);
        }
    }

    private void BuildSelectLists()
    {
        GenderItems = Enum.GetValues(typeof(SalonBeautyGender))
            .Cast<SalonBeautyGender>()
            .Select(x => new SelectListItem(EnumText(x), ((byte)x).ToString(), Customer.Gender == x))
            .ToList();

        SourceItems = Enum.GetValues(typeof(SalonBeautyCustomerSource))
            .Cast<SalonBeautyCustomerSource>()
            .Select(x => new SelectListItem(EnumText(x), ((byte)x).ToString(), Customer.Source == x))
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
