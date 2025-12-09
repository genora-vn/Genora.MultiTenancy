using Genora.MultiTenancy.AuditLogs;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Genora.MultiTenancy.Web.Pages.Admin.AuditLogs;

[Authorize(AuditLogPermissions.View)]
public class IndexModel : AbpPageModel
{
    private readonly AuditLogAppService _service;
    public PagedResultDto<AuditLogListDto> Result { get; set; }
    [BindProperty(SupportsGet = true)] public AuditLogGetListInputDto Filter { get; set; } = new();

    public IndexModel(AuditLogAppService service) { _service = service; }

    public async Task OnGetAsync()
    {
        if (Filter.MaxResultCount == 0) Filter.MaxResultCount = 20;
        Result = await _service.GetListAsync(Filter);
    }
}