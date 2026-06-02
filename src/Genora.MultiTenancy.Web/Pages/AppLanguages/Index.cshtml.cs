using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppLanguages;

public class IndexModel : MultiTenancyPageModel
{
    public async Task OnGetAsync()
    {
        await Task.CompletedTask;
    }
}
