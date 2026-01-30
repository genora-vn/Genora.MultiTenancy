using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppSpecialDates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppSpecialDates
{
    public class EditModalModel : MultiTenancyPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateUpdateSpecialDateDto SpecialDate { get; set; } = new();

        public List<SelectListItem> GolfCourseItems { get; set; } = new();

        private readonly IAppSpecialDateService _service;
        private readonly IAppGolfCourseService _golfCourseService;

        public EditModalModel(IAppSpecialDateService service, IAppGolfCourseService golfCourseService)
        {
            _service = service;
            _golfCourseService = golfCourseService;
        }

        public async Task OnGetAsync()
        {
            await LoadGolfCoursesAsync();

            var dto = await _service.GetAsync(Id);

            SpecialDate = new CreateUpdateSpecialDateDto
            {
                Name = dto.Name,
                Description = dto.Description,
                Dates = dto.Dates,
                Weekdays = dto.Weekdays, // NEW
                GolfCourseId = dto.GolfCourseId,
                IsActive = dto.IsActive
            };

            if (!SpecialDate.GolfCourseId.HasValue || SpecialDate.GolfCourseId == Guid.Empty)
            {
                var defaultId = GetDefaultGolfCourseId();
                if (defaultId.HasValue) SpecialDate.GolfCourseId = defaultId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _service.UpdateAsync(Id, SpecialDate);
            return NoContent();
        }

        private async Task LoadGolfCoursesAsync()
        {
            var result = await _golfCourseService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000,
                Sorting = "Name"
            });

            var ordered = result.Items
                .OrderByDescending(x => (x as dynamic)?.IsActive == true)
                .ThenBy(x => x.Name)
                .ToList();

            GolfCourseItems = ordered
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToList();
        }

        private Guid? GetDefaultGolfCourseId()
        {
            if (GolfCourseItems.Count == 0) return null;
            if (Guid.TryParse(GolfCourseItems[0].Value, out var id)) return id;
            return null;
        }
    }
}
