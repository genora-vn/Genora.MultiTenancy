using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieRatings;

public class DetailModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public CaddieRatingDto Rating { get; set; } = null!;
    public bool CanEdit { get; set; }
    public decimal AvgSkillRating { get; set; }

    private readonly CaddieRatingAppService _ratingService;
    private readonly IAuthorizationService _authorizationService;

    public DetailModel(
        CaddieRatingAppService ratingService,
        IAuthorizationService authorizationService)
    {
        _ratingService = ratingService;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Rating = await _ratingService.GetAsync(Id);

        CanEdit = CurrentTenant.IsAvailable
            ? (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.AppCaddieRatings.Edit)).Succeeded
            : (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.HostAppCaddieRatings.Edit)).Succeeded;

        // Calculate avg from skill details
        if (Rating.Details != null && Rating.Details.Any())
        {
            AvgSkillRating = Math.Round((decimal)Rating.Details.Average(d => d.Score), 1);
        }
        else
        {
            AvgSkillRating = Rating.OverallRating;
        }

        return Page();
    }
}
