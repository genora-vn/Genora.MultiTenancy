using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Users;
public class DetailModalModel : HlgAdminPageModel {
    protected override string PermissionGroup => "Users";
    public HlgUserDetailDto Item { get; set; }=new();
    private readonly IHlgUserAdminAppService _service;
    public DetailModalModel(IHlgUserAdminAppService service) { _service=service; }
    public async Task OnGetAsync(Guid id) { Item=await _service.GetAsync(id); }
}
