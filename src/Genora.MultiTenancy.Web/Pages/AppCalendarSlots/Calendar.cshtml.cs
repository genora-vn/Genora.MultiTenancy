using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCalendarSlots;
public class CalendarModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid GolfCourseId { get; set; }

    public string GolfCourseName { get; set; }
    private readonly IAppGolfCourseService _golfCourseService;
    private readonly IPromotionTypeService _promotionTypeService;
    public Dictionary<Guid, (string Color, string Icon)> PromotionTypeMap { get; set; }

    public CalendarModel(IAppGolfCourseService golfCourseService, IPromotionTypeService promotionTypeService)
    {
        _golfCourseService = golfCourseService;
        _promotionTypeService = promotionTypeService;
    }

    public async Task OnGetAsync()
    {
        var promotionTypesResult = await _promotionTypeService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 1000,
            Sorting = ""
        });

        PromotionTypeMap = promotionTypesResult.Items.ToDictionary(
            x => x.Id,
            x => (x.ColorCode, x.IconUrl)
        );

        var course = await _golfCourseService.GetAsync(GolfCourseId);
        GolfCourseName = course.Name;
    }
}