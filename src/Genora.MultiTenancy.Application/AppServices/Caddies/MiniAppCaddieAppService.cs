using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace Genora.MultiTenancy.AppServices.Caddies;

public class MiniAppCaddieAppService : ApplicationService
{
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<AppCaddieLanguage, Guid> _caddieLanguageRepo;
    private readonly IRepository<AppCaddieVoiceRegion, Guid> _caddieVoiceRegionRepo;
    private readonly IRepository<AppLanguage, Guid> _languageRepo;
    private readonly IRepository<AppCaddieSchedule, Guid> _scheduleRepo;
    private readonly IRepository<AppCaddieBooking, Guid> _bookingRepo;
    private readonly IRepository<AppCaddieRating, Guid> _ratingRepo;
    private readonly IRepository<AppCaddieRatingDetail, Guid> _ratingDetailRepo;
    private readonly IRepository<AppCaddieSkill, Guid> _skillRepo;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;

    public MiniAppCaddieAppService(
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieLanguage, Guid> caddieLanguageRepo,
        IRepository<AppCaddieVoiceRegion, Guid> caddieVoiceRegionRepo,
        IRepository<AppLanguage, Guid> languageRepo,
        IRepository<AppCaddieSchedule, Guid> scheduleRepo,
        IRepository<AppCaddieBooking, Guid> bookingRepo,
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        IRepository<AppCaddieSkill, Guid> skillRepo,
        IGuidGenerator guidGenerator,
        ICurrentUser currentUser)
    {
        _caddieRepo = caddieRepo;
        _caddieLanguageRepo = caddieLanguageRepo;
        _caddieVoiceRegionRepo = caddieVoiceRegionRepo;
        _languageRepo = languageRepo;
        _scheduleRepo = scheduleRepo;
        _bookingRepo = bookingRepo;
        _ratingRepo = ratingRepo;
        _ratingDetailRepo = ratingDetailRepo;
        _skillRepo = skillRepo;
        _guidGenerator = guidGenerator;
        _currentUser = currentUser;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    /// <summary>
    /// GET danh sách caddie available theo ngày + giờ
    /// </summary>
    public async Task<List<MiniAppCaddieListDto>> GetAvailableCaddiesAsync(DateTime bookingDate, TimeSpan? startTime)
    {
        // Get active caddies shown on app
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => x.Status == (byte)CaddieStatus.Active && x.IsShowOnApp);
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        if (!caddies.Any())
            return new List<MiniAppCaddieListDto>();

        var caddieIds = caddies.Select(x => x.Id).ToList();

        // Check schedule availability
        var scheduleQuery = (await _scheduleRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.CaddieId)
                && x.WorkDate == bookingDate
                && x.SlotStatus == (byte)CaddieSlotStatus.Available);

        if (startTime.HasValue)
            scheduleQuery = scheduleQuery.Where(x => x.StartTime <= startTime.Value && x.EndTime > startTime.Value);

        var availableSchedules = await AsyncExecuter.ToListAsync(scheduleQuery);
        var availableCaddieIds = availableSchedules.Select(x => x.CaddieId).Distinct().ToHashSet();

        // Load languages
        var langQuery = (await _caddieLanguageRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.CaddieId));
        var allLangQuery = await _languageRepo.GetQueryableAsync();
        var joinedLangQuery = langQuery.Join(allLangQuery,
            cl => cl.LanguageId, l => l.Id,
            (cl, l) => new { cl.CaddieId, l.LanguageName });
        var caddieLanguages = await AsyncExecuter.ToListAsync(joinedLangQuery);

        // Load voice regions
        var vrQuery = (await _caddieVoiceRegionRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.CaddieId));
        var caddieVoiceRegions = await AsyncExecuter.ToListAsync(vrQuery);

        return caddies.Select(x =>
        {
            var experienceYear = x.JoinDate.HasValue
                ? (int)((DateTime.Now - x.JoinDate.Value).TotalDays / 365.25) : 0;

            return new MiniAppCaddieListDto
            {
                Id = x.Id,
                CaddieCode = x.CaddieCode,
                CaddieName = x.CaddieName,
                Avatar = x.Avatar,
                Gender = x.Gender,
                GenderText = x.Gender switch
                {
                    (byte)CaddieGender.Male => "Nam",
                    (byte)CaddieGender.Female => "Nữ",
                    _ => null
                },
                ExperienceYear = experienceYear,
                HeightCm = x.HeightCm,
                RatingAvg = x.RatingAvg,
                TotalBooking = x.TotalBooking,
                Languages = caddieLanguages.Where(cl => cl.CaddieId == x.Id).Select(cl => cl.LanguageName).ToList(),
                VoiceRegions = caddieVoiceRegions.Where(vr => vr.CaddieId == x.Id).Select(vr => GetVoiceRegionText(vr.VoiceRegion)).ToList(),
                IsAvailable = availableCaddieIds.Contains(x.Id)
            };
        })
        .OrderByDescending(x => x.IsAvailable)
        .ThenByDescending(x => x.RatingAvg)
        .ToList();
    }

    /// <summary>
    /// GET chi tiết caddie + recent reviews
    /// </summary>
    public async Task<MiniAppCaddieDetailDto> GetCaddieDetailAsync(Guid caddieId)
    {
        var caddie = await _caddieRepo.GetAsync(caddieId);

        if (caddie.Status != (byte)CaddieStatus.Active || !caddie.IsShowOnApp)
            throw new UserFriendlyException("Caddie không khả dụng.");

        var experienceYear = caddie.JoinDate.HasValue
            ? (int)((DateTime.Now - caddie.JoinDate.Value).TotalDays / 365.25) : 0;

        // Load languages
        var langQuery = (await _caddieLanguageRepo.GetQueryableAsync()).Where(x => x.CaddieId == caddieId);
        var allLangQuery = await _languageRepo.GetQueryableAsync();
        var joinedQuery = langQuery.Join(allLangQuery, cl => cl.LanguageId, l => l.Id, (cl, l) => l.LanguageName);
        var languages = await AsyncExecuter.ToListAsync(joinedQuery);

        // Load voice regions
        var vrQuery = (await _caddieVoiceRegionRepo.GetQueryableAsync()).Where(x => x.CaddieId == caddieId);
        var voiceRegions = await AsyncExecuter.ToListAsync(vrQuery);

        // Load recent approved reviews (top 5)
        var reviewQuery = (await _ratingRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == caddieId && x.ApprovalStatus == (byte)CaddieRatingApprovalStatus.Approved)
            .OrderByDescending(x => x.CreationTime)
            .Take(5);
        var reviews = await AsyncExecuter.ToListAsync(reviewQuery);

        // Load review details
        var reviewIds = reviews.Select(x => x.Id).ToList();
        var detailQuery = (await _ratingDetailRepo.GetQueryableAsync())
            .Where(x => reviewIds.Contains(x.RatingId));
        var details = await AsyncExecuter.ToListAsync(detailQuery);

        // Load skill names
        var skillIds = details.Select(x => x.SkillId).Distinct().ToList();
        var skillQuery = (await _skillRepo.GetQueryableAsync())
            .Where(x => skillIds.Contains(x.Id));
        var skills = await AsyncExecuter.ToListAsync(skillQuery);

        // Load booking info for customer names
        var bookingIds = reviews.Select(x => x.BookingId).ToList();
        var bookingQuery = (await _bookingRepo.GetQueryableAsync())
            .Where(x => bookingIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CustomerName });
        var bookingCustomers = await AsyncExecuter.ToListAsync(bookingQuery);

        return new MiniAppCaddieDetailDto
        {
            Id = caddie.Id,
            CaddieCode = caddie.CaddieCode,
            CaddieName = caddie.CaddieName,
            Avatar = caddie.Avatar,
            Gender = caddie.Gender,
            GenderText = caddie.Gender switch
            {
                (byte)CaddieGender.Male => "Nam",
                (byte)CaddieGender.Female => "Nữ",
                _ => null
            },
            ExperienceYear = experienceYear,
            HeightCm = caddie.HeightCm,
            RatingAvg = caddie.RatingAvg,
            TotalBooking = caddie.TotalBooking,
            Languages = languages,
            VoiceRegions = voiceRegions.Select(x => GetVoiceRegionText(x.VoiceRegion)).ToList(),
            RecentReviews = reviews.Select(r =>
            {
                var customer = bookingCustomers.FirstOrDefault(b => b.Id == r.BookingId);
                return new MiniAppCaddieReviewDto
                {
                    OverallRating = r.OverallRating,
                    Comment = r.Comment,
                    CustomerName = customer?.CustomerName,
                    CreationTime = r.CreationTime,
                    Details = details.Where(d => d.RatingId == r.Id).Select(d =>
                    {
                        var skill = skills.FirstOrDefault(s => s.Id == d.SkillId);
                        return new CaddieRatingDetailDto { SkillId = d.SkillId, SkillName = skill?.SkillName, Score = d.Score };
                    }).ToList()
                };
            }).ToList()
        };
    }

    /// <summary>
    /// POST đặt caddie
    /// </summary>
    public async Task<MiniAppCaddieBookingHistoryDto> CreateBookingAsync(MiniAppCreateCaddieBookingDto input, Guid customerId, string customerName, string phone)
    {
        // Validate caddie
        var caddie = await _caddieRepo.GetAsync(input.CaddieId);
        if (caddie.Status != (byte)CaddieStatus.Active)
            throw new UserFriendlyException("Caddie không khả dụng.");

        // Validate booking date
        if (input.BookingDate.Date < DateTime.Today)
            throw new UserFriendlyException("Ngày chơi không được nhỏ hơn ngày hiện tại.");

        // Find available schedule slot
        var scheduleQuery = (await _scheduleRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == input.CaddieId
                && x.WorkDate == input.BookingDate.Date
                && x.SlotStatus == (byte)CaddieSlotStatus.Available
                && x.StartTime <= input.StartTime
                && x.EndTime > input.StartTime);
        var schedule = await AsyncExecuter.FirstOrDefaultAsync(scheduleQuery);

        if (schedule == null)
            throw new UserFriendlyException("Caddie không có lịch trống vào thời gian này.");

        // Generate booking code
        var bookingCode = $"CB-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        // Create booking
        var booking = new AppCaddieBooking
        {
            BookingCode = bookingCode,
            CustomerId = customerId,
            CustomerName = customerName,
            Phone = phone,
            GolfCourseId = caddie.GolfCourseId ?? Guid.Empty,
            CaddieId = input.CaddieId,
            ScheduleId = schedule.Id,
            BookingDate = input.BookingDate.Date,
            StartTime = input.StartTime,
            NumberOfHoles = input.NumberOfHoles,
            Note = input.Note,
            Status = (byte)CaddieBookingStatus.New,
            PaymentStatus = (byte)CaddiePaymentStatus.Unpaid,
            CheckinStatus = (byte)CaddieCheckinStatus.NotCheckedIn
        };

        await _bookingRepo.InsertAsync(booking, autoSave: true);

        // Lock schedule slot
        schedule.SlotStatus = (byte)CaddieSlotStatus.Booked;
        schedule.BookingId = booking.Id;
        await _scheduleRepo.UpdateAsync(schedule, autoSave: true);

        return new MiniAppCaddieBookingHistoryDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            CaddieName = caddie.CaddieName,
            CaddieAvatar = caddie.Avatar,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            NumberOfHoles = booking.NumberOfHoles,
            Status = booking.Status,
            StatusText = "Mới",
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = "Chưa thanh toán",
            HasRating = false
        };
    }

    /// <summary>
    /// GET lịch sử booking của customer
    /// </summary>
    public async Task<List<MiniAppCaddieBookingHistoryDto>> GetBookingHistoryAsync(Guid customerId)
    {
        var bookingQuery = (await _bookingRepo.GetQueryableAsync())
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.BookingDate);
        var bookings = await AsyncExecuter.ToListAsync(bookingQuery);

        if (!bookings.Any())
            return new List<MiniAppCaddieBookingHistoryDto>();

        // Load caddie info
        var caddieIds = bookings.Select(x => x.CaddieId).Distinct().ToList();
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CaddieName, x.Avatar });
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        // Check which bookings have ratings
        var bookingIds = bookings.Select(x => x.Id).ToList();
        var ratingQuery = (await _ratingRepo.GetQueryableAsync())
            .Where(x => bookingIds.Contains(x.BookingId))
            .Select(x => x.BookingId);
        var ratedBookingIds = (await AsyncExecuter.ToListAsync(ratingQuery)).ToHashSet();

        return bookings.Select(x =>
        {
            var caddie = caddies.FirstOrDefault(c => c.Id == x.CaddieId);
            return new MiniAppCaddieBookingHistoryDto
            {
                Id = x.Id,
                BookingCode = x.BookingCode,
                CaddieName = caddie?.CaddieName ?? "—",
                CaddieAvatar = caddie?.Avatar,
                BookingDate = x.BookingDate,
                StartTime = x.StartTime,
                NumberOfHoles = x.NumberOfHoles,
                Status = x.Status,
                StatusText = x.Status switch
                {
                    (byte)CaddieBookingStatus.New => "Mới",
                    (byte)CaddieBookingStatus.Confirmed => "Đã xác nhận",
                    (byte)CaddieBookingStatus.Completed => "Hoàn thành",
                    (byte)CaddieBookingStatus.Cancelled => "Đã hủy",
                    _ => "Khác"
                },
                PaymentStatus = x.PaymentStatus,
                PaymentStatusText = x.PaymentStatus switch
                {
                    (byte)CaddiePaymentStatus.Unpaid => "Chưa thanh toán",
                    (byte)CaddiePaymentStatus.Paid => "Đã thanh toán",
                    _ => "Khác"
                },
                HasRating = ratedBookingIds.Contains(x.Id)
            };
        }).ToList();
    }

    /// <summary>
    /// POST đánh giá caddie
    /// </summary>
    public async Task CreateRatingAsync(MiniAppCreateCaddieRatingDto input, Guid customerId)
    {
        // Validate booking
        var booking = await _bookingRepo.GetAsync(input.BookingId);

        if (booking.CustomerId != customerId)
            throw new UserFriendlyException("Bạn không có quyền đánh giá booking này.");

        if (booking.Status != (byte)CaddieBookingStatus.Completed)
            throw new UserFriendlyException("Chỉ booking đã hoàn thành mới được đánh giá.");

        // Check if already rated
        var existingQuery = (await _ratingRepo.GetQueryableAsync())
            .Where(x => x.BookingId == input.BookingId);
        var existingCount = await AsyncExecuter.CountAsync(existingQuery);
        if (existingCount > 0)
            throw new UserFriendlyException("Booking này đã được đánh giá.");

        // Validate rating
        if (input.OverallRating < 1 || input.OverallRating > 5)
            throw new UserFriendlyException("Đánh giá phải từ 1 đến 5 sao.");

        // Create rating
        var rating = new AppCaddieRating
        {
            BookingId = input.BookingId,
            CustomerId = customerId,
            CaddieId = booking.CaddieId,
            OverallRating = input.OverallRating,
            Comment = input.Comment,
            ApprovalStatus = (byte)CaddieRatingApprovalStatus.Pending
        };

        await _ratingRepo.InsertAsync(rating, autoSave: true);

        // Create rating details (skill ratings)
        if (input.SkillRatings?.Any() == true)
        {
            var details = input.SkillRatings
                .Where(s => s.Score >= 1 && s.Score <= 5)
                .Select(s => new AppCaddieRatingDetail(_guidGenerator.Create(), rating.Id, s.SkillId, s.Score))
                .ToList();

            if (details.Any())
                await _ratingDetailRepo.InsertManyAsync(details, autoSave: true);
        }
    }

    /// <summary>
    /// GET danh sách kỹ năng active (cho form đánh giá)
    /// </summary>
    public async Task<List<CaddieSkillDto>> GetActiveSkillsAsync()
    {
        var query = (await _skillRepo.GetQueryableAsync())
            .Where(x => x.Status == 1)
            .OrderBy(x => x.SortOrder);
        var items = await AsyncExecuter.ToListAsync(query);

        return items.Select(x => new CaddieSkillDto
        {
            Id = x.Id,
            SkillCode = x.SkillCode,
            SkillName = x.SkillName,
            Description = x.Description,
            SortOrder = x.SortOrder,
            Status = x.Status
        }).ToList();
    }

    private static string GetVoiceRegionText(byte region) => region switch
    {
        (byte)CaddieVoiceRegion.North => "Miền Bắc",
        (byte)CaddieVoiceRegion.Central => "Miền Trung",
        (byte)CaddieVoiceRegion.South => "Miền Nam",
        _ => "Khác"
    };
}
