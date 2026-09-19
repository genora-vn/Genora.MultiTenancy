using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Rewards;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Rewards";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgRewardDto Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    private readonly IHlgRewardAdminAppService _service;
    public EditModalModel(IHlgRewardAdminAppService service) { _service = service; }
    public async Task OnGetAsync()
    {
        var item = await _service.GetAsync(Id);
        Input = new UpdateHlgRewardDto
        {
            Name = item.Name,
            ImageUrl = item.ImageUrl,
            PointCost = item.PointCost,
            Type = item.Type,
            StockQuantity = item.StockQuantity,
            VoucherCode = item.VoucherCode,
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
