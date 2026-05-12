using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCalendarSlots;

public class DeleteModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? GolfCourseId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? GolfCourseName { get; set; }

    private readonly IAppGolfCourseService _golfCourseService;

    public DeleteModalModel(IAppGolfCourseService golfCourseService)
    {
        _golfCourseService = golfCourseService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Nếu không có GolfCourseId từ query, thử lấy từ GolfCourseName
        if (!GolfCourseId.HasValue || GolfCourseId.Value == Guid.Empty)
        {
            // Try to find by name
            if (!string.IsNullOrEmpty(GolfCourseName))
            {
                var courses = await _golfCourseService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });
                var found = courses.Items.FirstOrDefault(x => x.Name == GolfCourseName);
                if (found != null)
                {
                    GolfCourseId = found.Id;
                }
            }
        }
        else
        {
            // Get GolfCourseName if we have GolfCourseId
            if (string.IsNullOrEmpty(GolfCourseName))
            {
                try
                {
                    var course = await _golfCourseService.GetAsync(GolfCourseId.Value);
                    GolfCourseName = course?.Name;
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        return Page();
    }
}
