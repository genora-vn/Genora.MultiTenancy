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
    public class CreateModalModel : MultiTenancyPageModel
    {
        [BindProperty]
        public CreateUpdateSpecialDateDto SpecialDate { get; set; } = new();

        public List<SelectListItem> GolfCourseItems { get; set; } = new();

        private readonly IAppSpecialDateService _service;
        private readonly IAppGolfCourseService _golfCourseService;

        public CreateModalModel(IAppSpecialDateService service, IAppGolfCourseService golfCourseService)
        {
            _service = service;
            _golfCourseService = golfCourseService;
        }

        public async Task OnGetAsync()
        {
            await LoadGolfCoursesAsync();

            // default: chọn sân đang active (nếu có), không có thì chọn dòng đầu
            if (!SpecialDate.GolfCourseId.HasValue || SpecialDate.GolfCourseId == Guid.Empty)
            {
                var defaultId = GetDefaultGolfCourseId();
                if (defaultId.HasValue) SpecialDate.GolfCourseId = defaultId.Value;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _service.CreateAsync(SpecialDate);
            return NoContent();
        }

        private async Task LoadGolfCoursesAsync()
        {
            var result = await _golfCourseService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000,
                Sorting = "Name"
            });

            // Ưu tiên active lên đầu (nếu DTO có IsActive)
            // Nếu AppGolfCourseDto của không có IsActive thì bỏ OrderByDescending bên dưới.
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
            // Ưu tiên item đầu (đã sort active lên trước)
            if (GolfCourseItems.Count == 0) return null;
            if (Guid.TryParse(GolfCourseItems[0].Value, out var id)) return id;
            return null;
        }
    }
}
