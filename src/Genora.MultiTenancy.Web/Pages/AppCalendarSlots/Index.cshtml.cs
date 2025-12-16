using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCalendarSlots;

public class IndexModel : MultiTenancyPageModel
{
    public List<SelectListItem> GolfCourseItems { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SelectedGolfCourseId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? SelectedDate { get; set; }

    private readonly IAppGolfCourseService _golfCourseService;

    public IndexModel(IAppGolfCourseService golfCourseService)
    {
        _golfCourseService = golfCourseService;
    }

    public async Task OnGetAsync()
    {
        await LoadGolfCoursesAsync();
    }

    private async Task LoadGolfCoursesAsync()
    {
        var result = await _golfCourseService.GetListAsync(
            new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000,
                Sorting = "Name"
            });

        GolfCourseItems = result.Items
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();
    }
}