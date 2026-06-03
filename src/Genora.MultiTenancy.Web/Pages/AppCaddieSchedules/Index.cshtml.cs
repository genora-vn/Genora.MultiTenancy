using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieSchedules;

public class IndexModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? CaddieId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? WeekStart { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? View { get; set; }

    public List<CaddieScheduleDto> Schedules { get; set; } = new();
    public List<SelectListItem> CaddieItems { get; set; } = new();
    public DateTime CurrentWeekStart { get; set; }
    public string CurrentView { get; set; } = "week";

    private readonly CaddieScheduleAppService _scheduleService;
    private readonly CaddieAppService _caddieService;

    public IndexModel(CaddieScheduleAppService scheduleService, CaddieAppService caddieService)
    {
        _scheduleService = scheduleService;
        _caddieService = caddieService;
    }

    public async Task OnGetAsync()
    {
        // Determine view mode
        CurrentView = (View ?? "week").ToLower();
        if (CurrentView != "day" && CurrentView != "week" && CurrentView != "month")
            CurrentView = "week";

        // Determine week start (Monday)
        var today = DateTime.Today;
        if (WeekStart.HasValue)
            CurrentWeekStart = WeekStart.Value;
        else
        {
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            CurrentWeekStart = today.AddDays(-diff);
        }

        // Load schedules based on view mode
        if (CurrentView == "month")
        {
            // Load entire month
            var monthStart = new DateTime(CurrentWeekStart.Year, CurrentWeekStart.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            Schedules = await _scheduleService.GetListAsync(new GetCaddieScheduleListInput
            {
                FromDate = monthStart,
                ToDate = monthEnd.AddDays(-1),
                MaxResultCount = 1000
            }).ContinueWith(t => t.Result.Items.ToList());
        }
        else if (CurrentView == "day")
        {
            // Load single day
            Schedules = await _scheduleService.GetListAsync(new GetCaddieScheduleListInput
            {
                FromDate = CurrentWeekStart.Date,
                ToDate = CurrentWeekStart.Date,
                MaxResultCount = 500
            }).ContinueWith(t => t.Result.Items.ToList());
        }
        else
        {
            // Load week
            Schedules = await _scheduleService.GetWeekScheduleAsync(CurrentWeekStart);
        }

        // Load caddie lookup
        var caddies = await _caddieService.GetListAsync(new GetCaddieListInput
        {
            MaxResultCount = 500,
            Status = 1
        });
        CaddieItems = caddies.Items
            .Select(x => new SelectListItem($"{x.CaddieName} ({x.CaddieCode})", x.Id.ToString()))
            .ToList();
    }
}
