using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppGolfCourses;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppGolfCourseDto GolfCourse { get; set; }

    [BindProperty]
    public List<GolfCourseUtilityDto> UtilityDtos { get; set; }

    private readonly IAppGolfCourseService _appGolfCourseService;
    private readonly IOptionExtendService _extendService;

    public EditModalModel(
        IAppGolfCourseService appGolfCourseService,
        IOptionExtendService extendService)
    {
        _appGolfCourseService = appGolfCourseService;
        _extendService = extendService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Id = id;

        var dto = await _appGolfCourseService.GetAsync(id);
        UtilityDtos = await _extendService.GetUtilitiesAsync();

        var selectedUtilities = (dto.Utilities ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var selectedHoles = (dto.NumberHoles ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var selectedSessions = (dto.FrameTimes ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        GolfCourse = new CreateUpdateAppGolfCourseDto
        {
            Code = dto.Code,
            Name = dto.Name,
            Province = dto.Province,
            Address = dto.Address,
            Phone = dto.Phone,
            Website = dto.Website,
            FanpageUrl = dto.FanpageUrl,
            ShortDescription = dto.ShortDescription,
            OpenTime = dto.OpenTime,
            CloseTime = dto.CloseTime,
            IsActive = dto.IsActive,
            BookingStatus = dto.BookingStatus,
            PaymentQrText = dto.PaymentQrText,
            PaymentQrBankCode = dto.PaymentQrBankCode,
            PaymentQrBankAccount = dto.PaymentQrBankAccount,
            PaymentQrBankDisplay = dto.PaymentQrBankDisplay,

            AvailableUtilities = UtilityDtos.Select(x => new GolfCourseUtilityDto
            {
                UtilityId = x.UtilityId,
                UtilityName = x.UtilityName,
                IsCheck = selectedUtilities.Contains(x.UtilityId.ToString())
            }).ToList(),

            AvailableHoles = Enums.GolfCourseNumberHoleEnum.List().Select(x => new GolfCourseHoleDto
            {
                Id = x.Value,
                Name = x.Name,
                IsCheck = selectedHoles.Contains(x.Value.ToString())
            }).ToList(),

            AvailableSessionsOfDay = Enums.SessionOfDayEnum.List().Select(x => new GolfCourseSessionOfDayDto
            {
                Id = x.Value,
                Name = x.Name,
                IsCheck = selectedSessions.Contains(x.Value.ToString())
            }).ToList()
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        GolfCourse.Utilities = string.Join(",",
            GolfCourse.AvailableUtilities.Where(x => x.IsCheck).Select(x => x.UtilityId));

        if (!string.IsNullOrWhiteSpace(GolfCourse.Utilities))
        {
            GolfCourse.Utilities += ",";
        }

        GolfCourse.NumberHoles = string.Join(",",
            GolfCourse.AvailableHoles.Where(x => x.IsCheck).Select(x => x.Id));

        if (!string.IsNullOrWhiteSpace(GolfCourse.NumberHoles))
        {
            GolfCourse.NumberHoles += ",";
        }

        GolfCourse.FrameTimes = string.Join(",",
            GolfCourse.AvailableSessionsOfDay.Where(x => x.IsCheck).Select(x => x.Id));

        if (!string.IsNullOrWhiteSpace(GolfCourse.FrameTimes))
        {
            GolfCourse.FrameTimes += ",";
        }

        await _appGolfCourseService.UpdateAsync(Id, GolfCourse);
        return NoContent();
    }
}