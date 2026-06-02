using System.Linq;
using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieSkills;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateCaddieSkillDto Skill { get; set; } = new();

    private readonly CaddieSkillAppService _service;

    public EditModalModel(CaddieSkillAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var list = await _service.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });
        var item = list.Items.FirstOrDefault(x => x.Id == Id);
        if (item != null)
        {
            Skill = new CreateUpdateCaddieSkillDto
            {
                SkillCode = item.SkillCode,
                SkillName = item.SkillName,
                Description = item.Description,
                SortOrder = item.SortOrder,
                Status = item.Status
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Id, Skill);
        return NoContent();
    }
}
