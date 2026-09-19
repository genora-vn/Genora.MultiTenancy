using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Questions;
public class IndexModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Games";
    [BindProperty(SupportsGet = true)] public Guid ParentId { get; set; }
    public string ParentName { get; private set; } = "";
    private readonly IHlgGameAdminAppService _parents;
    public IndexModel(IHlgGameAdminAppService parents) { _parents = parents; }
    public async Task OnGetAsync() { ParentName = (await _parents.GetAsync(ParentId)).Name; }
}
