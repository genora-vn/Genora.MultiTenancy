using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
using System;
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
        GolfCourse = ObjectMapper.Map<AppGolfCourseDto, CreateUpdateAppGolfCourseDto>(appGolfCourseDto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appGolfCourseService.UpdateAsync(Id, GolfCourse);
        return NoContent();
    }
}