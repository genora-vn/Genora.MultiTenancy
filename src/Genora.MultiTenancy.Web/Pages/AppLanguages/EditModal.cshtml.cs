using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppLanguages;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateLanguageDto Language { get; set; } = new();

    private readonly CaddieLanguageAppService _service;

    public EditModalModel(CaddieLanguageAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var list = await _service.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });
        var item = list.Items.FirstOrDefault(x => x.Id == Id);
        if (item != null)
        {
            Language = new CreateUpdateLanguageDto
            {
                LanguageCode = item.LanguageCode,
                LanguageName = item.LanguageName,
                NativeName = item.NativeName,
                SortOrder = item.SortOrder,
                Status = item.Status
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Id, Language);
        return NoContent();
    }
}
