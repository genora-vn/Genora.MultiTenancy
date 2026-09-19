using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Fulfillment;
public class EditModalModel : HlgAdminPageModel {
    protected override string PermissionGroup => "Rewards"; protected override string ActionSuffix => ".Edit";
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    [BindProperty] public HlgFulfillmentInput Input { get; set; }=new();
    public HlgFulfillmentDto Item { get; set; }=new();
    private readonly IHlgFulfillmentAdminAppService _service;
    public EditModalModel(IHlgFulfillmentAdminAppService service) { _service=service; }
    public async Task OnGetAsync() { Item=await _service.GetAsync(Id); Input.Status=Item.Status; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) { Item=await _service.GetAsync(Id); return Page(); } await _service.UpdateAsync(Id,Input); return NoContent(); }
}
