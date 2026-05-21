using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyTimeSlots;

public class EditModalModel : MultiTenancyPageModel
{
    public Guid StylistId { get; set; }
    public string StylistName { get; set; } = string.Empty;

    public List<SelectListItem> LocationItems { get; set; } = new();
    public List<SalonBeautyLocationLookupDto> Locations { get; set; } = new();

    public Guid? CurrentLocationId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int WeekdayMask { get; set; } = 127;
    public byte Status { get; set; } = 1;
    public bool IsShowOnApp { get; set; } = true;
    public string? Note { get; set; }
    public List<TimeRangeDto> Ranges { get; set; } = new();

    private readonly ISalonBeautyLocationAppService _locationAppService;
    private readonly ISalonBeautyTimeSlotAppService _slotAppService;

    public EditModalModel(
        ISalonBeautyLocationAppService locationAppService,
        ISalonBeautyTimeSlotAppService slotAppService)
    {
        _locationAppService = locationAppService;
        _slotAppService = slotAppService;
    }

    public async Task OnGetAsync(Guid stylistId)
    {
        StylistId = stylistId;

        Locations = (await _locationAppService.GetLookupAsync()).ToList();

        var data = await _slotAppService.GetByStylistAsync(stylistId);
        StylistName = data.StylistName ?? string.Empty;
        CurrentLocationId = data.LocationId ?? Locations.FirstOrDefault()?.Id;
        FromDate = data.FromDate;
        ToDate = data.ToDate;
        WeekdayMask = data.WeekdayMask == 0 ? 127 : data.WeekdayMask;
        Status = data.Status;
        IsShowOnApp = data.IsShowOnApp;
        Note = data.Note;
        Ranges = data.Ranges ?? new List<TimeRangeDto>();

        LocationItems = Locations
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), CurrentLocationId.HasValue && CurrentLocationId.Value == x.Id))
            .ToList();
    }
}
