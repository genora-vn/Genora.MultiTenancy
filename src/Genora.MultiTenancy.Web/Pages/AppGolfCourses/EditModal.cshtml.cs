using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppGolfCourses;
public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppGolfCourseDto GolfCourse { get; set; }
    public List<GolfCourseUtilityDto> UtilityDtos { get; set; }
    private readonly IAppGolfCourseService _appGolfCourseService;
    private readonly IOptionExtendService _extendService;
    public EditModalModel(IAppGolfCourseService appGolfCourseService, IOptionExtendService extendService)
    {
        _appGolfCourseService = appGolfCourseService;
        _extendService = extendService;
    }

    public async Task OnGetAsync()
    {
        var appGolfCourseDto = await _appGolfCourseService.GetAsync(Id);
        UtilityDtos = await _extendService.GetUtilitiesAsync();
        var ulitities = new List<GolfCourseUtilityDto>();
        if (string.IsNullOrEmpty(appGolfCourseDto.Utilities) == false)
        {
            var utilities = appGolfCourseDto.Utilities.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var utility in UtilityDtos)
            {
                ulitities.Add(new GolfCourseUtilityDto
                {
                    UtilityId = utility.UtilityId,
                    UtilityName = utility.UtilityName,
                    IsCheck = Array.Exists(utilities, element => element == utility.UtilityId.ToString())
                });
            }
        }
        else
        {
            foreach (var utility in UtilityDtos)
            {
                ulitities.Add(new GolfCourseUtilityDto
                {
                    UtilityId = utility.UtilityId,
                    UtilityName = utility.UtilityName,
                    IsCheck = false
                });
            }
        }
        //if(UtilityDtos.Count == 0 && ulitities.Count == 0)
        //{
        //    UtilityDtos = UlititiesEnum.List().Select(x => new GolfCourseUtilityDto { UtilityId = x.Value, UtilityName = x.Name, IsCheck = false}).ToList();
        //    ulitities = UtilityDtos;
        //}

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
        int currentId = GolfCourse.AvailableUtilities.OrderByDescending(x => x.UtilityId).FirstOrDefault()?.UtilityId ?? 0;
        foreach (var utility in GolfCourse.AvailableUtilities)
        {
            if (utility.UtilityId == 0)
            {
                var createOption = new CreateUpdateOptionExtendDto { OptionId = currentId + 1, OptionName = utility.UtilityName , Type = OptionExtendTypeEnum.GolfCourseUlitity.Value};
                var create = await _extendService.CreateAsync(createOption);
                utility.UtilityId = create.OptionId;
                currentId = utility.UtilityId;
            }
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