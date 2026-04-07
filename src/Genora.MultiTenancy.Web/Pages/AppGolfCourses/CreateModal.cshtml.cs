using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.Web.Pages.AppGolfCourses;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppGolfCourseDto GolfCourse { get; set; }

    [BindProperty]
    public List<GolfCourseUtilityDto> UtilityDtos { get; set; }

    private readonly IAppGolfCourseService _appGolfCourseService;
    private readonly IOptionExtendService _extendService;
    private readonly IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> _promotionTypeRepository;

    public CreateModalModel(
        IAppGolfCourseService appGolfCourseService,
        IOptionExtendService extendService,
        IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> promotionTypeRepository)
    {
        _appGolfCourseService = appGolfCourseService;
        _extendService = extendService;
        _promotionTypeRepository = promotionTypeRepository;
    }

    public async Task OnGet()
    {
        var ulitities = new List<GolfCourseUtilityDto>();
        UtilityDtos = await _extendService.GetUtilitiesAsync();

        foreach (var utility in UtilityDtos)
        {
            ulitities.Add(new GolfCourseUtilityDto
            {
                UtilityId = utility.UtilityId,
                UtilityName = utility.UtilityName,
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
            IsCheck = false
        }).ToList();

        GolfCourse = new CreateUpdateAppGolfCourseDto
        {
            IsActive = true,
            BookingStatus = 1,
            AvailableUtilities = ulitities,
            AvailableHoles = holes,
            AvailableSessionsOfDay = sessions,
            AvailablePromotionTypes = promotionDtos
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Giữ default create nếu form không post các field này
        GolfCourse.IsActive = true;
        if (GolfCourse.BookingStatus == 0)
        {
            GolfCourse.BookingStatus = 1;
        }

        GolfCourse.Utilities = null;
        GolfCourse.NumberHoles = null;
        GolfCourse.FrameTimes = null;

        int currentId = GolfCourse.AvailableUtilities?
            .OrderByDescending(x => x.UtilityId)
            .FirstOrDefault()?.UtilityId ?? 0;

        if (GolfCourse.AvailableUtilities != null)
        {
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
        }

        if (GolfCourse.AvailableHoles != null)
        {
            foreach (var hole in GolfCourse.AvailableHoles)
            {
                if (hole.IsCheck)
                {
                    GolfCourse.NumberHoles ??= string.Empty;
                    GolfCourse.NumberHoles += hole.Id + ",";
                }
            }
        }

        if (GolfCourse.AvailableSessionsOfDay != null)
        {
            foreach (var session in GolfCourse.AvailableSessionsOfDay)
            {
                if (session.IsCheck)
                {
                    GolfCourse.FrameTimes ??= string.Empty;
                    GolfCourse.FrameTimes += session.Id + ",";
                }
            }
        }

        GolfCourse.PromotionTypeIds = NormalizeGuidCsv(GolfCourse.PromotionTypeIds);

        await _appGolfCourseService.CreateAsync(GolfCourse);
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