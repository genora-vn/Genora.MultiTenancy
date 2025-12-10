using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
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
        GolfCourse = new CreateUpdateAppGolfCourseDto
        {
            IsActive = true,
            BookingStatus = 1
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appGolfCourseService.CreateAsync(GolfCourse);
        return NoContent();
    }
}