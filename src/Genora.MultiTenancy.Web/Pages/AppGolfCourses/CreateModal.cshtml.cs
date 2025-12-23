using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppGolfCourses;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppGolfCourseDto GolfCourse { get; set; }

    private readonly IAppGolfCourseService _appGolfCourseService;

    public CreateModalModel(IAppGolfCourseService appGolfCourseService)
    {
        _appGolfCourseService = appGolfCourseService;
    }

    public void OnGet()
    {
        var ulitities = new List<GolfCourseUtilityDto>();
        foreach (var utility in Enums.UlititiesEnum.List())
        {
            ulitities.Add(new GolfCourseUtilityDto
            {
                UtilityId = utility.Value,
                UtilityName = utility.Name,
                IsCheck = false
            });
        }

        var holes = new List<GolfCourseHoleDto>();
        foreach (var hole in Enums.GolfCourseNumberHoleEnum.List())
        {
            holes.Add(new GolfCourseHoleDto
            {
                Id = hole.Value,
                Name = hole.Name,
                IsCheck = false
            });
        }

        var sessions = new List<GolfCourseSessionOfDayDto>();
        foreach (var session in Enums.SessionOfDayEnum.List())
        {
            sessions.Add(new GolfCourseSessionOfDayDto
            {
                Id = session.Value,
                Name = session.Name,
                IsCheck = false
            });
        }

        GolfCourse = new CreateUpdateAppGolfCourseDto
        {
            IsActive = true,
            BookingStatus = 1,
            AvailableUtilities = ulitities,
            AvailableHoles = holes,
            AvailableSessionsOfDay = sessions
        };
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
        await _appGolfCourseService.CreateAsync(GolfCourse);
        return NoContent();
    }
}