using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieRatings;

public class DetailModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public CaddieRatingDto Rating { get; set; } = null!;
    public bool CanEdit { get; set; }
    public decimal AvgSkillRating { get; set; }

    // Extra fields
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerAvatar { get; set; }

    private readonly CaddieRatingAppService _ratingService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRepository<Customer, Guid> _customerRepo;

    public DetailModel(
        CaddieRatingAppService ratingService,
        IAuthorizationService authorizationService,
        IRepository<Customer, Guid> customerRepo)
    {
        _ratingService = ratingService;
        _authorizationService = authorizationService;
        _customerRepo = customerRepo;
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

        // Load customer info
        try
        {
            var customer = await _customerRepo.GetAsync(Rating.CustomerId);
            CustomerPhone = customer.PhoneNumber;
            CustomerPhoneMasked = MaskPhone(customer.PhoneNumber);
            CustomerCode = customer.CustomerCode;
            CustomerAvatar = customer.AvatarUrl;
        }
        catch { /* customer may not exist */ }

        return Page();
    }

    private static string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
            return phone;
        return phone[..4] + " **** " + phone[^2..];
    }
}
