using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Products;
public class IndexModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    [BindProperty(SupportsGet = true)] public Guid ParentId { get; set; }
    public string ParentName { get; private set; } = "";
    private readonly IHlgCategoryAdminAppService _parents;
    public IndexModel(IHlgCategoryAdminAppService parents) { _parents = parents; }
    public async Task OnGetAsync() { ParentName = (await _parents.GetAsync(ParentId)).Name; }
}
