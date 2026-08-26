using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class ParticipantGrantModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Range(1, 100, ErrorMessage = "Số lượt cấp phải từ 1 đến 100.")]
    public int Turns { get; set; } = 1;

    [BindProperty]
    [StringLength(512)]
    public string? Note { get; set; }

    public string ParticipantInfo { get; set; } = "";

    private readonly IHl25ParticipantAppService _participantService;

    public ParticipantGrantModalModel(IHl25ParticipantAppService participantService)
    {
        _participantService = participantService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _participantService.GetAsync(Id);
        ParticipantInfo = $"{dto.FullName ?? "(chưa có tên)"} · {dto.PhoneNumber ?? "-"} · Lượt còn lại: {dto.RemainingSpinTurns}";
        Turns = 1;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _participantService.GrantSpinTurnAsync(Id, Turns, Note);
        return NoContent();
    }
}
