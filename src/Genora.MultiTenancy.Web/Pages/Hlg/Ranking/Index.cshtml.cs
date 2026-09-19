using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Ranking;
public class IndexModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    public void OnGet() { }
}
