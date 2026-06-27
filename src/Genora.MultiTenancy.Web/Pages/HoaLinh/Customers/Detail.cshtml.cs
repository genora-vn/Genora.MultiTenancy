using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Genora.MultiTenancy.Web.Pages.HoaLinh.Customers;

public class DetailModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Phone { get; set; }

    public void OnGet() { }
}
