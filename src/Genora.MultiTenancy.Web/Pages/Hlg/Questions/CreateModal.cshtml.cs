using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Questions;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Games";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgQuestionInput Input { get; set; } = new();
    private readonly IHlgQuestionAdminAppService _service;
    public CreateModalModel(IHlgQuestionAdminAppService service) { _service = service; }
    public void OnGet(Guid parentId) { Input.GameId = parentId; }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.CreateAsync(Input);
        return NoContent();
    }
}
