using System;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Content;
public class IndexModel : HlgAdminPageModel { protected override string PermissionGroup => "Content"; [BindProperty(SupportsGet=true)] public Guid? ParentId { get; set; } }
