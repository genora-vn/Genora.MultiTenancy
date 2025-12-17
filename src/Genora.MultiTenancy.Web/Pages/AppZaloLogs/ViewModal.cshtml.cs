using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppZaloLogs;
public class ViewModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public AppZaloLogDto Log { get; set; } = default!;
    public string MaskedRequest { get; set; } = "";
    public string MaskedResponse { get; set; } = "";

    private readonly IAppZaloLogAppService _service;

    public ViewModalModel(IAppZaloLogAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        Log = await _service.GetAsync(Id);
        MaskedRequest = MaskTokens(Log.RequestBody);
        MaskedResponse = MaskTokens(Log.ResponseBody);
    }

    private static string MaskTokens(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var s = input;

        s = Regex.Replace(s, "(\"access_token\"\\s*:\\s*\")([^\"]+)(\")", "$1***MASKED***$3", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "(\"refresh_token\"\\s*:\\s*\")([^\"]+)(\")", "$1***MASKED***$3", RegexOptions.IgnoreCase);

        s = Regex.Replace(s, "(access_token=)([^&\\s]+)", "$1***MASKED***", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "(refresh_token=)([^&\\s]+)", "$1***MASKED***", RegexOptions.IgnoreCase);

        return s;
    }
}