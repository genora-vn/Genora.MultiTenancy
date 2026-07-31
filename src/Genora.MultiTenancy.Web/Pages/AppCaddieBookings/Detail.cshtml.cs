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

    // Danh sách Caddie đã book trong booking (hỗ trợ nhiều Caddie)
    public List<BookingCaddieInfo> Caddies { get; set; } = new();
    // Đánh giá theo từng Caddie (1 booking nhiều Caddie → nhiều đánh giá)
    public List<CaddieRatingInfo> CaddieRatings { get; set; } = new();

    public string? CustomerAvatar { get; set; }
    public string? CustomerCode { get; set; }

    public class BookingCaddieInfo
    {
        public Guid CaddieId { get; set; }
        public string? CaddieName { get; set; }
        public string? CaddieCode { get; set; }
        public string? Avatar { get; set; }
        public string? Phone { get; set; }
        public string? PhoneMasked { get; set; }
        public decimal RatingAvg { get; set; }
        public string? Note { get; set; }
    }

    public class CaddieRatingInfo
    {
        public Guid CaddieId { get; set; }
        public string? CaddieName { get; set; }
        public int OverallRating { get; set; }
        public string? Comment { get; set; }
        public List<CaddieRatingDetailDto> Details { get; set; } = new();
    }

    private readonly CaddieBookingAppService _bookingService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<AppCaddieBookingDetail, Guid> _bookingDetailRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<AppCaddieRating, Guid> _ratingRepo;
    private readonly IRepository<AppCaddieRatingDetail, Guid> _ratingDetailRepo;
    private readonly IRepository<AppCaddieSkill, Guid> _skillRepo;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public DetailModel(
        CaddieBookingAppService bookingService,
        IAuthorizationService authorizationService,
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieBookingDetail, Guid> bookingDetailRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        IRepository<AppCaddieSkill, Guid> skillRepo,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _bookingService = bookingService;
        _authorizationService = authorizationService;
        _caddieRepo = caddieRepo;
        _bookingDetailRepo = bookingDetailRepo;
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

        // Load danh sách Caddie đã book trong booking (từ AppCaddieBookingDetails)
        try
        {
            var detailQuery = (await _bookingDetailRepo.GetQueryableAsync())
                .Where(d => d.CaddieBookingId == Id);
            var bookingDetails = await _asyncExecuter.ToListAsync(detailQuery);

            var caddieIds = bookingDetails.Select(d => d.CaddieId).Distinct().ToList();
            var caddieList = await _asyncExecuter.ToListAsync(
                (await _caddieRepo.GetQueryableAsync()).Where(c => caddieIds.Contains(c.Id)));

            Caddies = bookingDetails.Select(d =>
            {
                var caddie = caddieList.FirstOrDefault(c => c.Id == d.CaddieId);
                return new BookingCaddieInfo
                {
                    CaddieId = d.CaddieId,
                    CaddieName = caddie?.CaddieName,
                    CaddieCode = caddie?.CaddieCode,
                    Avatar = caddie?.Avatar,
                    Phone = caddie?.Phone,
                    PhoneMasked = MaskPhone(caddie?.Phone),
                    RatingAvg = caddie?.RatingAvg ?? 0,
                    Note = d.Note
                };
            }).ToList();
        }
        catch { /* details may not exist */ }

        // Load customer avatar
        try
        {
            var customer = await _customerRepo.GetAsync(Booking.CustomerId);
            CustomerAvatar = customer.AvatarUrl;
            CustomerCode = customer.CustomerCode;
        }
        catch { /* customer may not exist */ }

        // Load đánh giá theo từng Caddie (1 booking nhiều Caddie → nhiều đánh giá)
        try
        {
            var ratingQuery = (await _ratingRepo.GetQueryableAsync())
                .Where(x => x.BookingId == Id);
            var ratings = await _asyncExecuter.ToListAsync(ratingQuery);

            if (ratings.Any())
            {
                var ratingIds = ratings.Select(r => r.Id).ToList();
                var allDetails = await _asyncExecuter.ToListAsync(
                    (await _ratingDetailRepo.GetQueryableAsync()).Where(x => ratingIds.Contains(x.RatingId)));

                var skillIds = allDetails.Select(x => x.SkillId).Distinct().ToList();
                var skills = await _asyncExecuter.ToListAsync(
                    (await _skillRepo.GetQueryableAsync()).Where(x => skillIds.Contains(x.Id)));

                CaddieRatings = ratings.Select(r =>
                {
                    var caddie = Caddies.FirstOrDefault(c => c.CaddieId == r.CaddieId);
                    return new CaddieRatingInfo
                    {
                        CaddieId = r.CaddieId,
                        CaddieName = caddie?.CaddieName,
                        OverallRating = r.OverallRating,
                        Comment = r.Comment,
                        Details = allDetails.Where(d => d.RatingId == r.Id).Select(d => new CaddieRatingDetailDto
                        {
                            SkillId = d.SkillId,
                            SkillName = skills.FirstOrDefault(s => s.Id == d.SkillId)?.SkillName,
                            Score = d.Score
                        }).ToList()
                    };
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
