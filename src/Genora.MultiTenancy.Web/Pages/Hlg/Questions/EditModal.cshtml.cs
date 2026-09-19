using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Questions;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Games";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgQuestionInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    private readonly IHlgQuestionAdminAppService _service;
    public EditModalModel(IHlgQuestionAdminAppService service) { _service = service; }
    public async Task OnGetAsync()
    {
        var item = await _service.GetEditorAsync(Id);
        Input = new UpdateHlgQuestionInput
        {
            GameId = item.GameId,
            Index = item.Index,
            Content = item.Content,
            ImageUrl = item.ImageUrl,
            TimeLimitSec = item.TimeLimitSec,
            ScoreMultiplier = item.ScoreMultiplier,
            OptionA = item.OptionA,
            OptionB = item.OptionB,
            OptionC = item.OptionC,
            OptionD = item.OptionD,
            CorrectKey = item.CorrectKey,
            IsActive = item.IsActive,
        };
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.UpdateAsync(Id, Input);
        return NoContent();
    }
}
