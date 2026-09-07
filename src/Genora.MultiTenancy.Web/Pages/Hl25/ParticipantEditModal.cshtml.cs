using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class ParticipantEditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateHl25ParticipantDto Participant { get; set; } = new();

    public string DisplaySpinInfo { get; set; } = "";

    private readonly IHl25ParticipantAppService _participantService;

    public ParticipantEditModalModel(IHl25ParticipantAppService participantService)
    {
        _participantService = participantService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _participantService.GetAsync(Id);
        Participant = new CreateUpdateHl25ParticipantDto
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            AgeGroup = dto.AgeGroup,
            Gender = dto.Gender,
            ReceiveAddress = dto.ReceiveAddress,
            IsFollowingOa = dto.IsFollowingOa,
            HasConsent = dto.HasConsent
        };
        DisplaySpinInfo = $"Lượt còn lại: {dto.RemainingSpinTurns} · Tổng lượt: {dto.TotalSpinTurns} · Quà đã trúng: {dto.TotalGiftsWon}";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _participantService.UpdateAsync(Id, Participant);
        return NoContent();
    }
}
