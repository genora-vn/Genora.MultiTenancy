using Genora.MultiTenancy.AuditLogs;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Genora.MultiTenancy.Web.Pages.Admin.AuditLogs;

[Authorize(AuditLogPermissions.View)]
public class DetailModel : AbpPageModel
{
    private readonly AuditLogAppService _service;
    public AuditLogDetailDto Item { get; set; }

    public DetailModel(AuditLogAppService service) { _service = service; }

    public async Task OnGetAsync(Guid id)
    {
        Item = await _service.GetAsync(id);
    }
}