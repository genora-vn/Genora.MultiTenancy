using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

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
    private readonly IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> _promotionTypeRepository;

    public EditModalModel(
        IAppGolfCourseService appGolfCourseService,
        IOptionExtendService extendService,
        IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> promotionTypeRepository)
    {
        _appGolfCourseService = appGolfCourseService;
        _extendService = extendService;
        _promotionTypeRepository = promotionTypeRepository;
    }

    public async Task OnGetAsync()
    {
        var appGolfCourseDto = await _appGolfCourseService.GetAsync(Id);

        UtilityDtos = await _extendService.GetUtilitiesAsync();

        var ulitities = new List<GolfCourseUtilityDto>();
        if (!string.IsNullOrEmpty(appGolfCourseDto.Utilities))
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

        var holes = new List<GolfCourseHoleDto>();
        if (!string.IsNullOrEmpty(appGolfCourseDto.NumberHoles))
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
        if (!string.IsNullOrEmpty(appGolfCourseDto.FrameTimes))
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

        var selectedPromotionIds = string.IsNullOrWhiteSpace(appGolfCourseDto.PromotionTypeIds)
            ? new HashSet<Guid>()
            : appGolfCourseDto.PromotionTypeIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                .Where(x => x != Guid.Empty)
                .ToHashSet();

        var promotionQuery = await _promotionTypeRepository.GetQueryableAsync();
        var promotions = await promotionQuery
            .Where(x => x.Status)
            .OrderBy(x => x.Name)
            .ToListAsync();

        var promotionDtos = promotions.Select(x => new GolfCoursePromotionTypeDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            IconUrl = x.IconUrl,
            ColorCode = x.ColorCode,
            IsCheck = selectedPromotionIds.Contains(x.Id)
        }).ToList();

        var golfCourse = ObjectMapper.Map<AppGolfCourseDto, CreateUpdateAppGolfCourseDto>(appGolfCourseDto);
        golfCourse.AvailableUtilities = ulitities;
        golfCourse.AvailableHoles = holes;
        golfCourse.AvailableSessionsOfDay = sessions;
        golfCourse.AvailablePromotionTypes = promotionDtos;

        GolfCourse = golfCourse;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        int currentId = GolfCourse.AvailableUtilities.OrderByDescending(x => x.UtilityId).FirstOrDefault()?.UtilityId ?? 0;

        GolfCourse.Utilities = null;
        GolfCourse.NumberHoles = null;
        GolfCourse.FrameTimes = null;

        foreach (var utility in GolfCourse.AvailableUtilities)
        {
            if (utility.UtilityId == 0 && !string.IsNullOrWhiteSpace(utility.UtilityName))
            {
                var createOption = new CreateUpdateOptionExtendDto
                {
                    OptionId = currentId + 1,
                    OptionName = utility.UtilityName.Trim(),
                    Type = OptionExtendTypeEnum.GolfCourseUlitity.Value
                };

                var create = await _extendService.CreateAsync(createOption);
                utility.UtilityId = create.OptionId;
                currentId = utility.UtilityId;
            }

            if (utility.IsCheck && utility.UtilityId > 0)
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

        GolfCourse.PromotionTypeIds = NormalizeGuidCsv(GolfCourse.PromotionTypeIds);

        await _appGolfCourseService.UpdateAsync(Id, GolfCourse);
        return NoContent();
    }

    private static string? NormalizeGuidCsv(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var values = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .Select(x => x.ToString())
            .ToList();

        return values.Count == 0 ? null : string.Join(",", values);
    }
}