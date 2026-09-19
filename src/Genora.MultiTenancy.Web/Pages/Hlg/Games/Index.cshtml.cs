using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Games;
public class IndexModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Games";
    public void OnGet() { }
}
