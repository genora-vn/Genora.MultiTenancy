using System;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Winners;
public class IndexModel : HlgAdminPageModel { protected override string PermissionGroup => "Ranking"; [BindProperty(SupportsGet=true)] public Guid? ParentId { get; set; } }
