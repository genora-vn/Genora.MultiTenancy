using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieRatings;

public class IndexModel : MultiTenancyPageModel
{
    public async Task OnGetAsync()
    {
        await Task.CompletedTask;
    }
}
