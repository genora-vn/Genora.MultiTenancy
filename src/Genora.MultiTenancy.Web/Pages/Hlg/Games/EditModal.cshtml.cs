using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Games;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Games";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgGameInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    private readonly IHlgGameAdminAppService _service;
    public EditModalModel(IHlgGameAdminAppService service) { _service = service; }
    public async Task OnGetAsync()
    {
        var item = await _service.GetAsync(Id);
        Input = new UpdateHlgGameInput
        {
            Name = item.Name,
            Type = item.Type,
            ImageUrl = item.ImageUrl,
            Description = item.Description,
            Rules = item.Rules,
            RewardDescription = item.RewardDescription,
            Status = item.Status,
            StartAt = item.StartAt,
            EndAt = item.EndAt,
            BaseScorePerQuestion = item.BaseScorePerQuestion,
            DisplayOrder = item.DisplayOrder,
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
