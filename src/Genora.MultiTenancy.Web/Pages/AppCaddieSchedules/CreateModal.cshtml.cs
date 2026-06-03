using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieSchedules;

public class CreateModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public CreateUpdateCaddieScheduleDto Schedule { get; set; } = new();

    /// <summary>
    /// End date for auto-generate range (optional, only for Create mode)
    /// </summary>
    [BindProperty]
    public DateTime? WorkDateTo { get; set; }

    /// <summary>
    /// Multiple shift codes selected (for Create mode)
    /// </summary>
    [BindProperty]
    public List<byte> SelectedShiftCodes { get; set; } = new();

    public List<SelectListItem> CaddieItems { get; set; } = new();
    public List<SelectListItem> ShiftItems { get; set; } = new();
    public List<SelectListItem> StatusItems { get; set; } = new();
    public bool IsEdit { get; set; }

    private readonly CaddieScheduleAppService _scheduleService;
    private readonly CaddieAppService _caddieService;

    public CreateModalModel(CaddieScheduleAppService scheduleService, CaddieAppService caddieService)
    {
        _scheduleService = scheduleService;
        _caddieService = caddieService;
    }

    public async Task OnGetAsync()
    {
        await BuildSelectListsAsync();

        if (Id.HasValue)
        {
            IsEdit = true;
            var list = await _scheduleService.GetListAsync(new GetCaddieScheduleListInput { MaxResultCount = 1000 });
            var item = list.Items.FirstOrDefault(x => x.Id == Id.Value);
            if (item != null)
            {
                Schedule = new CreateUpdateCaddieScheduleDto
                {
                    CaddieId = item.CaddieId,
                    WorkDate = item.WorkDate,
                    ShiftCode = item.ShiftCode,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    SlotStatus = item.SlotStatus,
                    IsNightShift = item.IsNightShift,
                    Note = item.Note
                };
            }
        }
        else
        {
            Schedule = new CreateUpdateCaddieScheduleDto
            {
                WorkDate = DateTime.Today,
                ShiftCode = (byte)CaddieShiftCode.Morning,
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                SlotStatus = (byte)CaddieSlotStatus.Available
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Id.HasValue)
        {
            // Edit: single record update
            await _scheduleService.UpdateAsync(Id.Value, Schedule);
        }
        else
        {
            // Create: auto-generate for date range and multiple shifts
            var fromDate = Schedule.WorkDate;
            var toDate = WorkDateTo ?? fromDate; // If no end date, only 1 day

            // Ensure toDate >= fromDate
            if (toDate < fromDate) toDate = fromDate;

            // Limit range to max 31 days to prevent abuse
            if ((toDate - fromDate).TotalDays > 31) toDate = fromDate.AddDays(31);

            // Collect shifts to generate
            var shifts = SelectedShiftCodes?.Any() == true
                ? SelectedShiftCodes.Distinct().ToList()
                : new List<byte> { Schedule.ShiftCode };

            // Generate schedules for each day and each shift
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                foreach (var shiftCode in shifts)
                {
                    var dto = new CreateUpdateCaddieScheduleDto
                    {
                        CaddieId = Schedule.CaddieId,
                        WorkDate = date,
                        ShiftCode = shiftCode,
                        StartTime = Schedule.StartTime,
                        EndTime = Schedule.EndTime,
                        SlotStatus = Schedule.SlotStatus,
                        IsNightShift = shiftCode == (byte)CaddieShiftCode.Night || Schedule.IsNightShift,
                        Note = Schedule.Note
                    };

                    try
                    {
                        await _scheduleService.CreateAsync(dto);
                    }
                    catch
                    {
                        // Skip if max shifts exceeded for a day (business rule)
                    }
                }
            }
        }

        return NoContent();
    }

    private async Task BuildSelectListsAsync()
    {
        var caddies = await _caddieService.GetListAsync(new GetCaddieListInput { MaxResultCount = 500, Status = 1 });
        CaddieItems = caddies.Items
            .Select(x => new SelectListItem($"{x.CaddieName} ({x.CaddieCode})", x.Id.ToString()))
            .ToList();

        ShiftItems = new List<SelectListItem>
        {
            new("Sáng (Morning)", ((byte)CaddieShiftCode.Morning).ToString()),
            new("Chiều (Afternoon)", ((byte)CaddieShiftCode.Afternoon).ToString()),
            new("Tối (Night)", ((byte)CaddieShiftCode.Night).ToString())
        };

        StatusItems = new List<SelectListItem>
        {
            new("Trống lịch", ((byte)CaddieSlotStatus.Available).ToString()),
            new("Đang phục vụ", ((byte)CaddieSlotStatus.Booked).ToString()),
            new("Nghỉ", ((byte)CaddieSlotStatus.Off).ToString())
        };
    }
}
