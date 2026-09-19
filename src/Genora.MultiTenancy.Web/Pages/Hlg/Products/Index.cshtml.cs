using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Products;
public class IndexModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    [BindProperty(SupportsGet = true)] public Guid? ParentId { get; set; }
    [BindProperty(SupportsGet=true)] public Guid? BrandId { get; set; }
    public string ParentName { get; private set; } = "";
    private readonly IHlgCategoryAdminAppService _parents;
    public IndexModel(IHlgCategoryAdminAppService parents) { _parents = parents; }
    public async Task OnGetAsync() { if (ParentId.HasValue) ParentName = (await _parents.GetAsync(ParentId.Value)).Name; }
}
