using Genora.MultiTenancy.AppDtos.AppEmails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Genora.MultiTenancy.Web.Pages.AppEmails;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    public bool IsReadOnly { get; set; }

    // Dùng để hiển thị ở View (Status/TryCount/SentTime/LastError/ModelJson...)
    public AppEmailDto? Email { get; set; }

    // Dùng để submit Update (To/Cc/Bcc/Subject/Body/BookingCode...)
    [BindProperty]
    public CreateUpdateEmailDto EmailInput { get; set; } = new();

    private readonly IAppEmailService _appEmailService;

    public EditModalModel(IAppEmailService appEmailService)
    {
        _appEmailService = appEmailService;
    }

    public async Task OnGetAsync(Guid id, bool? readonlyParam = null)
    {
        Id = id;
        IsReadOnly = readonlyParam == true;

        Email = await _appEmailService.GetAsync(id);

        // map sang input để edit
        EmailInput = new CreateUpdateEmailDto
        {
            ToEmails = Email.ToEmails,
            CcEmails = Email.CcEmails,
            BccEmails = Email.BccEmails,
            Subject = Email.Subject,
            Body = Email.Body,
            BookingId = Email.BookingId,
            BookingCode = Email.BookingCode
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (IsReadOnly)
        {
            // View-only: không cho update
            return NoContent();
        }

        await _appEmailService.UpdateAsync(Id, EmailInput);
        return NoContent();
    }
}