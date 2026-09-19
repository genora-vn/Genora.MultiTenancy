using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Rewards;
public class IndexModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Rewards";
    public void OnGet() { }
}
