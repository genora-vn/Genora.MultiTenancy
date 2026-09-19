using System;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Brands;
public class IndexModel : HlgAdminPageModel { protected override string PermissionGroup => "Knowledge"; [BindProperty(SupportsGet=true)] public Guid? ParentId { get; set; } }
