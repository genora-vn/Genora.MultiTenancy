using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppGolfCourses;
public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppGolfCourseDto GolfCourse { get; set; }

    private readonly IAppGolfCourseService _appGolfCourseService;

    public EditModalModel(IAppGolfCourseService appGolfCourseService)
    {
        _appGolfCourseService = appGolfCourseService;
    }

    public async Task OnGetAsync()
    {
        var appGolfCourseDto = await _appGolfCourseService.GetAsync(Id);

        var ulitities = new List<GolfCourseUtilityDto>();
        if (string.IsNullOrEmpty(appGolfCourseDto.Utilities) == false)
        {
            var utilities = appGolfCourseDto.Utilities.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var utility in Enums.UlititiesEnum.List())
            {
                ulitities.Add(new GolfCourseUtilityDto
                {
                    UtilityId = utility.Value,
                    UtilityName = utility.Name,
                    IsCheck = Array.Exists(utilities, element => element == utility.Value.ToString())
                });
            }
        }
        else
        {
            foreach (var utility in Enums.UlititiesEnum.List())
            {
                ulitities.Add(new GolfCourseUtilityDto
                {
                    UtilityId = utility.Value,
                    UtilityName = utility.Name,
                    IsCheck = false
                });
            }
        }

        var holes = new List<GolfCourseHoleDto>();
        if (string.IsNullOrEmpty(appGolfCourseDto.NumberHoles) == false)
        {
            var utilities = appGolfCourseDto.NumberHoles.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var hole in Enums.GolfCourseNumberHoleEnum.List())
            {
                holes.Add(new GolfCourseHoleDto
                {
                    Id = hole.Value,
                    Name = hole.Name,
                    IsCheck = Array.Exists(utilities, element => element == hole.Value.ToString())
                });
            }
        }
        else
        {
            foreach (var hole in Enums.GolfCourseNumberHoleEnum.List())
            {
                holes.Add(new GolfCourseHoleDto
                {
                    Id = hole.Value,
                    Name = hole.Name,
                    IsCheck = false
                });
            }
        }

        var sessions = new List<GolfCourseSessionOfDayDto>();
        if (string.IsNullOrEmpty(appGolfCourseDto.FrameTimes) == false)
        {
            var frameTimes = appGolfCourseDto.FrameTimes.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var session in Enums.SessionOfDayEnum.List())
            {
                sessions.Add(new GolfCourseSessionOfDayDto
                {
                    Id = session.Value,
                    Name = session.Name,
                    IsCheck = Array.Exists(frameTimes, element => element == session.Value.ToString())
                });
            }
        }
        else
        {
            foreach (var session in Enums.SessionOfDayEnum.List())
            {
                sessions.Add(new GolfCourseSessionOfDayDto
                {
                    Id = session.Value,
                    Name = session.Name,
                    IsCheck = false
                });
            }
        }
        
        var golfCourse = ObjectMapper.Map<AppGolfCourseDto, CreateUpdateAppGolfCourseDto>(appGolfCourseDto);
        golfCourse.AvailableUtilities = ulitities;
        golfCourse.AvailableHoles = holes;
        golfCourse.AvailableSessionsOfDay = sessions;
        GolfCourse = golfCourse;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        foreach (var utility in GolfCourse.AvailableUtilities)
        {
            if (utility.IsCheck)
            {
                GolfCourse.Utilities ??= string.Empty;
                GolfCourse.Utilities += utility.UtilityId + ",";
            }
        }
        foreach (var hole in GolfCourse.AvailableHoles)
        {
            if (hole.IsCheck)
            {
                GolfCourse.NumberHoles ??= string.Empty;
                GolfCourse.NumberHoles += hole.Id + ",";
            }
        }
        foreach (var session in GolfCourse.AvailableSessionsOfDay)
        {
            if (session.IsCheck)
            {
                GolfCourse.FrameTimes ??= string.Empty;
                GolfCourse.FrameTimes += session.Id + ",";
            }
        }
        await _appGolfCourseService.UpdateAsync(Id, GolfCourse);
        return NoContent();
    }
}