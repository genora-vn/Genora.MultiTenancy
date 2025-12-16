using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppCalendarSlots;
public class CalendarModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid GolfCourseId { get; set; }

    public string GolfCourseName { get; set; }

    private readonly IAppGolfCourseService _golfCourseService;

    public CalendarModel(IAppGolfCourseService golfCourseService)
    {
        _golfCourseService = golfCourseService;
    }

    public async Task OnGetAsync()
    {
        var course = await _golfCourseService.GetAsync(GolfCourseId);
        GolfCourseName = course.Name;
    }
}