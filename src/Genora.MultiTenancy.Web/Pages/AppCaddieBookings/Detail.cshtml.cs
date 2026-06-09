using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieBookings;

public class DetailModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public CaddieBookingDto Booking { get; set; } = null!;
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

    // Extra data for detail page
    public string? CaddieAvatar { get; set; }
    public string? CaddiePhone { get; set; }
    public string? CaddiePhoneMasked { get; set; }
    public decimal CaddieRatingAvg { get; set; }
    public string? CustomerAvatar { get; set; }
    public string? CustomerCode { get; set; }
    public List<CaddieRatingDetailDto> RatingDetails { get; set; } = new();
    public string? RatingComment { get; set; }
    public Guid? RatingId { get; set; }

    private readonly CaddieBookingAppService _bookingService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<AppCaddieRating, Guid> _ratingRepo;
    private readonly IRepository<AppCaddieRatingDetail, Guid> _ratingDetailRepo;
    private readonly IRepository<AppCaddieSkill, Guid> _skillRepo;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public DetailModel(
        CaddieBookingAppService bookingService,
        IAuthorizationService authorizationService,
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        IRepository<AppCaddieSkill, Guid> skillRepo,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _bookingService = bookingService;
        _authorizationService = authorizationService;
        _caddieRepo = caddieRepo;
        _customerRepo = customerRepo;
        _ratingRepo = ratingRepo;
        _ratingDetailRepo = ratingDetailRepo;
        _skillRepo = skillRepo;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Booking = await _bookingService.GetAsync(Id);

        CanEdit = CurrentTenant.IsAvailable
            ? (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.AppCaddieBookings.Edit)).Succeeded
            : (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.HostAppCaddieBookings.Edit)).Succeeded;

        CanDelete = CurrentTenant.IsAvailable
            ? (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.AppCaddieBookings.Delete)).Succeeded
            : (await _authorizationService.AuthorizeAsync(User, MultiTenancyPermissions.HostAppCaddieBookings.Delete)).Succeeded;

        // Load caddie extra info
        try
        {
            var caddie = await _caddieRepo.GetAsync(Booking.CaddieId);
            CaddieAvatar = caddie.Avatar;
            CaddiePhone = caddie.Phone;
            CaddiePhoneMasked = MaskPhone(caddie.Phone);
            CaddieRatingAvg = caddie.RatingAvg;
        }
        catch { /* caddie may not exist */ }

        // Load customer avatar
        try
        {
            var customer = await _customerRepo.GetAsync(Booking.CustomerId);
            CustomerAvatar = customer.AvatarUrl;
            CustomerCode = customer.CustomerCode;
        }
        catch { /* customer may not exist */ }

        // Load rating for this booking
        try
        {
            var ratingQuery = (await _ratingRepo.GetQueryableAsync())
                .Where(x => x.BookingId == Id);
            var rating = await _asyncExecuter.FirstOrDefaultAsync(ratingQuery);
            if (rating != null)
            {
                RatingId = rating.Id;
                RatingComment = rating.Comment;

                var detailQuery = (await _ratingDetailRepo.GetQueryableAsync())
                    .Where(x => x.RatingId == rating.Id);
                var details = await _asyncExecuter.ToListAsync(detailQuery);

                var skillIds = details.Select(x => x.SkillId).ToList();
                var skillQuery = (await _skillRepo.GetQueryableAsync())
                    .Where(x => skillIds.Contains(x.Id));
                var skills = await _asyncExecuter.ToListAsync(skillQuery);

                RatingDetails = details.Select(d => new CaddieRatingDetailDto
                {
                    SkillId = d.SkillId,
                    SkillName = skills.FirstOrDefault(s => s.Id == d.SkillId)?.SkillName,
                    Score = d.Score
                }).ToList();
            }
        }
        catch { /* rating may not exist */ }

        return Page();
    }

    private static string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
            return phone;
        return phone[..3] + " *** " + phone[^4..];
    }
}
