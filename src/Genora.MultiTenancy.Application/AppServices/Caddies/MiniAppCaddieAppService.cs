using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Caddies;

public class MiniAppCaddieAppService : ApplicationService
{
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<AppCaddieLanguage, Guid> _caddieLanguageRepo;
    private readonly IRepository<AppCaddieVoiceRegion, Guid> _caddieVoiceRegionRepo;
    private readonly IRepository<AppLanguage, Guid> _languageRepo;
    private readonly IRepository<AppCaddieSchedule, Guid> _scheduleRepo;
    private readonly IRepository<AppCaddieBooking, Guid> _bookingRepo;
    private readonly IRepository<AppCaddieBookingDetail, Guid> _bookingDetailRepo;
    private readonly IRepository<AppCaddieRating, Guid> _ratingRepo;
    private readonly IRepository<AppCaddieRatingDetail, Guid> _ratingDetailRepo;
    private readonly IRepository<AppCaddieSkill, Guid> _skillRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepo;
    private readonly IRepository<Booking, Guid> _golfBookingRepo;
    private readonly IRepository<BookingPlayer, Guid> _bookingPlayerRepo;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly IConfiguration _configuration;

    public MiniAppCaddieAppService(
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieLanguage, Guid> caddieLanguageRepo,
        IRepository<AppCaddieVoiceRegion, Guid> caddieVoiceRegionRepo,
        IRepository<AppLanguage, Guid> languageRepo,
        IRepository<AppCaddieSchedule, Guid> scheduleRepo,
        IRepository<AppCaddieBooking, Guid> bookingRepo,
        IRepository<AppCaddieBookingDetail, Guid> bookingDetailRepo,
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        IRepository<AppCaddieSkill, Guid> skillRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<GolfCourse, Guid> golfCourseRepo,
        IRepository<Booking, Guid> golfBookingRepo,
        IRepository<BookingPlayer, Guid> bookingPlayerRepo,
        IGuidGenerator guidGenerator,
        ICurrentUser currentUser,
        IConfiguration configuration)
    {
        _caddieRepo = caddieRepo;
        _caddieLanguageRepo = caddieLanguageRepo;
        _caddieVoiceRegionRepo = caddieVoiceRegionRepo;
        _languageRepo = languageRepo;
        _scheduleRepo = scheduleRepo;
        _bookingRepo = bookingRepo;
        _bookingDetailRepo = bookingDetailRepo;
        _ratingRepo = ratingRepo;
        _ratingDetailRepo = ratingDetailRepo;
        _skillRepo = skillRepo;
        _customerRepo = customerRepo;
        _golfCourseRepo = golfCourseRepo;
        _golfBookingRepo = golfBookingRepo;
        _bookingPlayerRepo = bookingPlayerRepo;
        _guidGenerator = guidGenerator;
        _currentUser = currentUser;
        _configuration = configuration;
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
                Avatar = ResolveAvatarUrl(x.Avatar),
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
            throw new AbpValidationException("Caddie không khả dụng.");

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
            Avatar = ResolveAvatarUrl(caddie.Avatar),
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
                var reviewDetails = details.Where(d => d.RatingId == r.Id).ToList();
                // Compute avg from skill details instead of using stored OverallRating
                var computedRating = reviewDetails.Count > 0
                    ? Math.Round((decimal)reviewDetails.Average(d => d.Score), 2)
                    : (decimal)r.OverallRating;
                return new MiniAppCaddieReviewDto
                {
                    OverallRating = computedRating,
                    Comment = r.Comment,
                    CustomerName = customer?.CustomerName,
                    CreationTime = r.CreationTime,
                    Details = reviewDetails.Select(d =>
                    {
                        var skill = skills.FirstOrDefault(s => s.Id == d.SkillId);
                        return new CaddieRatingDetailDto { SkillId = d.SkillId, SkillName = skill?.SkillName, Score = d.Score };
                    }).ToList()
                };
            }).ToList()
        };
    }

    /// <summary>
    /// POST đặt caddie (hỗ trợ book 1 hoặc nhiều caddy)
    /// </summary>
    /// <summary>
    /// UPSERT booking Caddie: nếu input.CaddieBookingId có giá trị → cập nhật (reconcile danh sách caddie),
    /// ngược lại → tạo booking mới. Hỗ trợ khách thêm/bớt caddie theo danh sách người chơi.
    /// TotalCaddieFee được server tự tính lại = số caddie × GolfCourse.CaddieFee.
    /// </summary>
    public async Task<MiniAppCreatedCaddieBookingDto> CreateBookingAsync(MiniAppCreateCaddieBookingDto input)
    {
        // Look up customer from DB
        if (input.CustomerId == Guid.Empty)
            throw new AbpValidationException("CustomerId không hợp lệ.");

        var customer = await _customerRepo.FindAsync(input.CustomerId);
        if (customer == null)
            throw new AbpValidationException("Không tìm thấy thông tin khách hàng.");

        var customerName = customer.FullName;
        var phone = customer.PhoneNumber;

        // Validate caddies input
        if (input.Caddies == null || !input.Caddies.Any())
            throw new AbpValidationException("Vui lòng chọn ít nhất 1 Caddie.");

        var caddieItems = input.Caddies.GroupBy(c => c.CaddieId).Select(g => g.First()).ToList();

        // Validate booking date
        if (input.BookingDate.Date < DateTime.Today)
            throw new AbpValidationException("Ngày chơi không được nhỏ hơn ngày hiện tại.");

        var isUpdate = input.CaddieBookingId.HasValue && input.CaddieBookingId.Value != Guid.Empty;

        // ── Load hoặc tạo mới booking header ──────────────────────────────
        AppCaddieBooking booking;
        List<AppCaddieBookingDetail> existingDetails = new();

        if (isUpdate)
        {
            booking = await _bookingRepo.FindAsync(input.CaddieBookingId!.Value)
                ?? throw new AbpValidationException("Không tìm thấy booking Caddie cần cập nhật.");

            if (booking.Status == (byte)CaddieBookingStatus.Completed || booking.Status == (byte)CaddieBookingStatus.Cancelled)
                throw new AbpValidationException("Không thể cập nhật booking Caddie đã hoàn thành hoặc đã hủy.");

            existingDetails = await AsyncExecuter.ToListAsync(
                (await _bookingDetailRepo.GetQueryableAsync())
                    .Where(d => d.CaddieBookingId == booking.Id));
        }
        else
        {
            var bookingCode = $"CB-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
            booking = new AppCaddieBooking
            {
                BookingCode = bookingCode,
                CustomerId = input.CustomerId,
                CustomerName = customerName,
                Phone = phone,
                GolfCourseId = Guid.Empty, // set sau khi biết caddie đầu tiên
                BookingDate = input.BookingDate.Date,
                StartTime = input.StartTime,
                NumberOfHoles = input.NumberOfHoles,
                Note = input.Note,
                TotalCaddieFee = 0m,
                PaymentMethod = input.PaymentMethod,
                Status = (byte)CaddieBookingStatus.New,
                PaymentStatus = (byte)CaddiePaymentStatus.Unpaid,
                CheckinStatus = (byte)CaddieCheckinStatus.NotCheckedIn
            };
            await _bookingRepo.InsertAsync(booking, autoSave: true);
        }

        // ── Validate + resolve schedule cho các caddie MỚI (chưa có detail) ──
        var existingCaddieIds = existingDetails.Select(d => d.CaddieId).ToHashSet();
        var targetCaddieIds = caddieItems.Select(c => c.CaddieId).ToHashSet();

        var caddieMap = new Dictionary<Guid, AppCaddie>();
        var newCaddieSchedules = new List<(Guid CaddieId, AppCaddieSchedule Schedule, string? Note)>();
        AppCaddie? firstCaddie = null;

        foreach (var item in caddieItems)
        {
            var caddie = await _caddieRepo.GetAsync(item.CaddieId);
            caddieMap[item.CaddieId] = caddie;
            if (firstCaddie == null) firstCaddie = caddie;

            // Caddie đã có trong booking rồi → giữ nguyên, chỉ cập nhật note nếu đổi
            if (existingCaddieIds.Contains(item.CaddieId))
            {
                var existed = existingDetails.First(d => d.CaddieId == item.CaddieId);
                if (existed.Note != item.Note)
                {
                    existed.Note = item.Note;
                    await _bookingDetailRepo.UpdateAsync(existed, autoSave: true);
                }
                continue;
            }

            if (caddie.Status != (byte)CaddieStatus.Active)
                throw new AbpValidationException($"Caddie {caddie.CaddieName} không khả dụng.");

            // Schedule Conflict Detection: check for existing bookings that overlap this time slot
            var conflictDetails = await AsyncExecuter.ToListAsync(
                (await _bookingDetailRepo.GetQueryableAsync())
                    .Where(d => d.CaddieId == item.CaddieId && d.CaddieBookingId != booking.Id));
            var conflictBookingIds = conflictDetails.Select(d => d.CaddieBookingId).Distinct().ToList();
            if (conflictBookingIds.Any())
            {
                var conflictQuery = (await _bookingRepo.GetQueryableAsync())
                    .Where(b => conflictBookingIds.Contains(b.Id)
                        && b.BookingDate == input.BookingDate.Date
                        && b.Status != (byte)CaddieBookingStatus.Cancelled
                        && b.StartTime == input.StartTime);
                var conflictBooking = await AsyncExecuter.FirstOrDefaultAsync(conflictQuery);
                if (conflictBooking != null)
                    throw new AbpValidationException($"Caddie {caddie.CaddieName} đã có booking #{conflictBooking.BookingCode} trùng thời gian.");
            }

            var scheduleQuery = (await _scheduleRepo.GetQueryableAsync())
                .Where(x => x.CaddieId == item.CaddieId
                    && x.WorkDate == input.BookingDate.Date
                    && x.SlotStatus == (byte)CaddieSlotStatus.Available
                    && x.StartTime <= input.StartTime
                    && x.EndTime > input.StartTime);
            var schedule = await AsyncExecuter.FirstOrDefaultAsync(scheduleQuery);

            if (schedule == null)
                throw new AbpValidationException($"Caddie {caddie.CaddieName} không có lịch trống vào thời gian này.");

            newCaddieSchedules.Add((item.CaddieId, schedule, item.Note));
        }

        // ── Xóa các caddie KHÔNG còn trong danh sách (khi update) + nhả schedule ──
        foreach (var removed in existingDetails.Where(d => !targetCaddieIds.Contains(d.CaddieId)).ToList())
        {
            await ReleaseScheduleSlotAsync(removed.ScheduleId);
            await _bookingDetailRepo.DeleteAsync(removed, autoSave: true);
            existingDetails.Remove(removed);
        }

        // ── Thêm detail cho các caddie mới + khóa schedule ──────────────────
        foreach (var (caddieId, schedule, note) in newCaddieSchedules)
        {
            var detail = new AppCaddieBookingDetail(_guidGenerator.Create(), booking.Id, caddieId, schedule.Id)
            {
                Note = note
            };
            await _bookingDetailRepo.InsertAsync(detail, autoSave: true);
            existingDetails.Add(detail);

            schedule.SlotStatus = (byte)CaddieSlotStatus.Booked;
            schedule.BookingId = booking.Id;
            await _scheduleRepo.UpdateAsync(schedule, autoSave: true);
        }

        // ── Cập nhật header: GolfCourse, giờ chơi, phí Caddie (server tự tính) ──
        booking.GolfCourseId = firstCaddie!.GolfCourseId ?? booking.GolfCourseId;
        booking.BookingDate = input.BookingDate.Date;
        booking.StartTime = input.StartTime;
        booking.NumberOfHoles = input.NumberOfHoles;
        if (!string.IsNullOrWhiteSpace(input.Note)) booking.Note = input.Note;
        booking.PaymentMethod = input.PaymentMethod;

        var totalCaddieFee = await ComputeCaddieFeeAsync(booking.GolfCourseId, existingDetails.Count);
        booking.TotalCaddieFee = totalCaddieFee;
        await _bookingRepo.UpdateAsync(booking, autoSave: true);

        // ── Đồng bộ phí Caddie sang booking golf liên kết (nếu có) ──────────
        await SyncGolfBookingCaddieFeeAsync(booking.Id, totalCaddieFee);

        // ── Build response: toàn bộ caddie hiện có trong booking ────────────
        var caddieItemsResult = new List<MiniAppCreatedCaddieItemDto>();
        foreach (var detail in existingDetails)
        {
            if (!caddieMap.TryGetValue(detail.CaddieId, out var caddie))
            {
                caddie = await _caddieRepo.GetAsync(detail.CaddieId);
                caddieMap[detail.CaddieId] = caddie;
            }
            caddieItemsResult.Add(new MiniAppCreatedCaddieItemDto
            {
                CaddieBookingDetailId = detail.Id,
                CaddieId = detail.CaddieId,
                CaddieName = caddie.CaddieName,
                CaddieCode = caddie.CaddieCode,
                CaddieAvatar = ResolveAvatarUrl(caddie.Avatar),
                RatingAvg = caddie.RatingAvg,
                ScheduleId = detail.ScheduleId,
                Note = detail.Note
            });
        }

        return new MiniAppCreatedCaddieBookingDto
        {
            CaddieBookingId = booking.Id,
            BookingCode = booking.BookingCode,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            NumberOfHoles = booking.NumberOfHoles,
            Status = booking.Status,
            StatusText = "Mới",
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = "Chưa thanh toán",
            TotalCaddieFee = booking.TotalCaddieFee,
            PaymentMethod = booking.PaymentMethod,
            Caddies = caddieItemsResult
        };
    }

    /// <summary>
    /// Hủy gán 1 Caddie khỏi booking: xóa AppCaddieBookingDetail + nhả schedule + gỡ liên kết trong AppBookingPlayers.
    /// Nếu hết Caddie → booking Caddie chuyển trạng thái Cancelled, TotalCaddieFee=0.
    /// Luôn cập nhật lại TotalCaddieFee (AppCaddieBookings) + đồng bộ booking golf (AppBookings.TotalAmount).
    /// </summary>
    public async Task<MiniAppCreatedCaddieBookingDto> UnassignCaddieAsync(Guid caddieBookingId, Guid caddieId)
    {
        var booking = await _bookingRepo.FindAsync(caddieBookingId)
            ?? throw new AbpValidationException("Không tìm thấy booking Caddie.");

        if (booking.Status == (byte)CaddieBookingStatus.Completed)
            throw new AbpValidationException("Không thể hủy Caddie của booking đã hoàn thành.");

        var details = await AsyncExecuter.ToListAsync(
            (await _bookingDetailRepo.GetQueryableAsync())
                .Where(d => d.CaddieBookingId == caddieBookingId));

        var target = details.FirstOrDefault(d => d.CaddieId == caddieId)
            ?? throw new AbpValidationException("Caddie không có trong booking này.");

        // Xóa detail + nhả schedule
        await ReleaseScheduleSlotAsync(target.ScheduleId);
        await _bookingDetailRepo.DeleteAsync(target, autoSave: true);
        details.Remove(target);

        // Gỡ liên kết Caddie này khỏi các người chơi golf (AppBookingPlayers)
        var linkedPlayers = await _bookingPlayerRepo.GetListAsync(
            p => p.CaddieBookingId == caddieBookingId && p.CaddieId == caddieId);
        foreach (var p in linkedPlayers)
        {
            p.CaddieId = null;
            p.CaddieName = null;
            p.CaddieBookingId = null;
            await _bookingPlayerRepo.UpdateAsync(p, autoSave: true);
        }

        if (details.Count == 0)
        {
            // Hết Caddie → hủy booking Caddie
            booking.Status = (byte)CaddieBookingStatus.Cancelled;
            booking.CancelReason = "Đã hủy toàn bộ Caddie";
            booking.TotalCaddieFee = 0m;
            await _bookingRepo.UpdateAsync(booking, autoSave: true);
            await SyncGolfBookingCaddieFeeAsync(caddieBookingId, 0m);
        }
        else
        {
            // Còn Caddie → tính lại phí
            var newFee = await ComputeCaddieFeeAsync(booking.GolfCourseId, details.Count);
            booking.TotalCaddieFee = newFee;
            await _bookingRepo.UpdateAsync(booking, autoSave: true);
            await SyncGolfBookingCaddieFeeAsync(caddieBookingId, newFee);
        }

        // Build response
        var caddieItemsResult = new List<MiniAppCreatedCaddieItemDto>();
        foreach (var detail in details)
        {
            var caddie = await _caddieRepo.GetAsync(detail.CaddieId);
            caddieItemsResult.Add(new MiniAppCreatedCaddieItemDto
            {
                CaddieBookingDetailId = detail.Id,
                CaddieId = detail.CaddieId,
                CaddieName = caddie.CaddieName,
                CaddieCode = caddie.CaddieCode,
                CaddieAvatar = ResolveAvatarUrl(caddie.Avatar),
                RatingAvg = caddie.RatingAvg,
                ScheduleId = detail.ScheduleId,
                Note = detail.Note
            });
        }

        return new MiniAppCreatedCaddieBookingDto
        {
            CaddieBookingId = booking.Id,
            BookingCode = booking.BookingCode,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            NumberOfHoles = booking.NumberOfHoles,
            Status = booking.Status,
            StatusText = booking.Status == (byte)CaddieBookingStatus.Cancelled ? "Đã hủy" : "Mới",
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = booking.PaymentStatus == (byte)CaddiePaymentStatus.Paid ? "Đã thanh toán" : "Chưa thanh toán",
            TotalCaddieFee = booking.TotalCaddieFee,
            PaymentMethod = booking.PaymentMethod,
            Caddies = caddieItemsResult
        };
    }

    /// <summary>Nhả 1 slot lịch Caddie về trạng thái Available (bỏ khóa booking). No-op nếu scheduleId null/rỗng.</summary>
    private async Task ReleaseScheduleSlotAsync(Guid? scheduleId)
    {
        if (scheduleId == null || scheduleId.Value == Guid.Empty) return;
        try
        {
            var schedule = await _scheduleRepo.GetAsync(scheduleId.Value);
            schedule.SlotStatus = (byte)CaddieSlotStatus.Available;
            schedule.BookingId = null;
            await _scheduleRepo.UpdateAsync(schedule, autoSave: true);
        }
        catch { /* Schedule may not exist */ }
    }

    /// <summary>Tính tổng phí Caddie = số caddie × đơn giá GolfCourse.CaddieFee.</summary>
    private async Task<decimal> ComputeCaddieFeeAsync(Guid golfCourseId, int caddieCount)
    {
        if (caddieCount <= 0 || golfCourseId == Guid.Empty) return 0m;
        var golfCourse = await _golfCourseRepo.FindAsync(golfCourseId);
        var unitFee = golfCourse?.CaddieFee ?? 0m;
        return unitFee * caddieCount;
    }

    /// <summary>Đồng bộ TotalCaddieFee sang tất cả booking golf (AppBookings) có liên kết CaddieBookingId; cập nhật lại TotalAmount.</summary>
    private async Task SyncGolfBookingCaddieFeeAsync(Guid caddieBookingId, decimal newCaddieFee)
    {
        // Tìm các booking golf liên kết qua AppBookingPlayers.CaddieBookingId
        var linkedPlayers = await _bookingPlayerRepo.GetListAsync(p => p.CaddieBookingId == caddieBookingId);
        var golfBookingIds = linkedPlayers.Select(p => p.BookingId).Distinct().ToList();

        foreach (var golfBookingId in golfBookingIds)
        {
            var golfBooking = await _golfBookingRepo.FindAsync(golfBookingId);
            if (golfBooking == null) continue;

            // TotalAmount = tổng giá người chơi + phí Caddie mới (thay phí cũ)
            var playersTotal = (await _bookingPlayerRepo.GetListAsync(p => p.BookingId == golfBookingId))
                .Sum(p => p.PricePerPlayer ?? 0m);
            golfBooking.TotalCaddieFee = newCaddieFee > 0 ? newCaddieFee : (decimal?)null;
            golfBooking.TotalAmount = playersTotal + newCaddieFee;
            await _golfBookingRepo.UpdateAsync(golfBooking, autoSave: true);
        }
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

        // Load caddie info from booking details (giữ Note để trả về từng caddie)
        var bookingIds = bookings.Select(x => x.Id).ToList();
        var allDetails = await AsyncExecuter.ToListAsync(
            (await _bookingDetailRepo.GetQueryableAsync())
                .Where(d => bookingIds.Contains(d.CaddieBookingId))
                .Select(d => new { d.CaddieBookingId, d.CaddieId, d.Note }));

        var caddieIds = allDetails.Select(d => d.CaddieId).Distinct().ToList();
        var caddies = await AsyncExecuter.ToListAsync(
            (await _caddieRepo.GetQueryableAsync())
                .Where(x => caddieIds.Contains(x.Id)));

        // Check which bookings have ratings
        var ratingQuery = (await _ratingRepo.GetQueryableAsync())
            .Where(x => bookingIds.Contains(x.BookingId))
            .Select(x => x.BookingId);
        var ratedBookingIds = (await AsyncExecuter.ToListAsync(ratingQuery)).ToHashSet();

        return bookings.Select(x =>
        {
            var bookingDetails = allDetails.Where(d => d.CaddieBookingId == x.Id).ToList();

            // Danh sách tất cả Caddie trong booking (match shape của API detail)
            var caddieList = bookingDetails.Select(d =>
            {
                var caddie = caddies.FirstOrDefault(c => c.Id == d.CaddieId);
                return new MiniAppBookingCaddieDetailDto
                {
                    CaddieId = d.CaddieId,
                    CaddieName = caddie?.CaddieName ?? "—",
                    CaddieCode = caddie?.CaddieCode,
                    CaddieAvatar = ResolveAvatarUrl(caddie?.Avatar),
                    RatingAvg = caddie?.RatingAvg ?? 0,
                    Phone = caddie?.Phone,
                    Gender = caddie?.Gender,
                    GenderText = caddie?.Gender switch
                    {
                        (byte)CaddieGender.Male => "Nam",
                        (byte)CaddieGender.Female => "Nữ",
                        _ => null
                    },
                    Note = d.Note
                };
            }).ToList();

            // Caddie đầu tiên — giữ các field phẳng cho tương thích ngược (client cũ)
            var first = caddieList.FirstOrDefault();
            return new MiniAppCaddieBookingHistoryDto
            {
                Id = x.Id,
                BookingCode = x.BookingCode,
                CaddieName = first?.CaddieName ?? "—",
                CaddieCode = first?.CaddieCode,
                CaddieAvatar = first?.CaddieAvatar,
                CaddieRatingAvg = first?.RatingAvg ?? 0,
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
                TotalCaddieFee = x.TotalCaddieFee,
                PaymentMethod = x.PaymentMethod,
                HasRating = ratedBookingIds.Contains(x.Id),
                Caddies = caddieList
            };
        }).ToList();
    }

    /// <summary>
    /// GET chi tiết lịch đặt caddie
    /// </summary>
    public async Task<MiniAppCaddieBookingDetailDto> GetBookingDetailAsync(Guid bookingId)
    {
        var booking = await _bookingRepo.GetAsync(bookingId);

        // Load golf course info
        string? golfCourseName = null;
        string? golfCourseAddress = null;
        var golfCourse = await _golfCourseRepo.FindAsync(booking.GolfCourseId);
        if (golfCourse != null)
        {
            golfCourseName = golfCourse.Name;
            golfCourseAddress = golfCourse.Address;
        }

        // Load booking details (caddies)
        var detailQuery = (await _bookingDetailRepo.GetQueryableAsync())
            .Where(d => d.CaddieBookingId == bookingId);
        var details = await AsyncExecuter.ToListAsync(detailQuery);

        var caddieIds = details.Select(d => d.CaddieId).Distinct().ToList();
        var caddies = await AsyncExecuter.ToListAsync(
            (await _caddieRepo.GetQueryableAsync())
                .Where(x => caddieIds.Contains(x.Id)));

        return new MiniAppCaddieBookingDetailDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            NumberOfHoles = booking.NumberOfHoles,
            Status = booking.Status,
            StatusText = booking.Status switch
            {
                (byte)CaddieBookingStatus.New => "Mới",
                (byte)CaddieBookingStatus.Confirmed => "Đã xác nhận",
                (byte)CaddieBookingStatus.Completed => "Hoàn thành",
                (byte)CaddieBookingStatus.Cancelled => "Đã hủy",
                _ => "Khác"
            },
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = booking.PaymentStatus switch
            {
                (byte)CaddiePaymentStatus.Unpaid => "Chưa thanh toán",
                (byte)CaddiePaymentStatus.Paid => "Đã thanh toán",
                _ => "Khác"
            },
            TotalCaddieFee = booking.TotalCaddieFee,
            PaymentMethod = booking.PaymentMethod,
            PaymentMethodText = booking.PaymentMethod switch
            {
                0 => "Thanh toán tại quầy",
                1 => "Thanh toán online",
                2 => "Chuyển khoản ngân hàng",
                _ => "Khác"
            },
            CheckinStatus = booking.CheckinStatus,
            CheckinStatusText = booking.CheckinStatus switch
            {
                (byte)CaddieCheckinStatus.NotCheckedIn => "Chưa check-in",
                (byte)CaddieCheckinStatus.CheckedIn => "Đã check-in",
                _ => "Khác"
            },
            CheckinTime = booking.CheckinTime,
            Note = booking.Note,
            CancelReason = booking.CancelReason,
            CreationTime = booking.CreationTime,
            CustomerId = booking.CustomerId,
            CustomerName = booking.CustomerName,
            CustomerPhone = booking.Phone,
            GolfCourseId = booking.GolfCourseId,
            GolfCourseName = golfCourseName,
            GolfCourseAddress = golfCourseAddress,
            Caddies = details.Select(d =>
            {
                var caddie = caddies.FirstOrDefault(c => c.Id == d.CaddieId);
                return new MiniAppBookingCaddieDetailDto
                {
                    CaddieId = d.CaddieId,
                    CaddieName = caddie?.CaddieName ?? "—",
                    CaddieCode = caddie?.CaddieCode,
                    CaddieAvatar = ResolveAvatarUrl(caddie?.Avatar),
                    RatingAvg = caddie?.RatingAvg ?? 0,
                    Phone = caddie?.Phone,
                    Gender = caddie?.Gender,
                    GenderText = caddie?.Gender switch
                    {
                        (byte)CaddieGender.Male => "Nam",
                        (byte)CaddieGender.Female => "Nữ",
                        _ => null
                    },
                    Note = d.Note
                };
            }).ToList()
        };
    }

    /// <summary>
    /// POST đánh giá caddie — hỗ trợ đánh giá NHIỀU Caddie trong 1 booking (mỗi Caddie 1 đánh giá).
    /// Tương thích ngược: nếu client cũ gửi 1 đánh giá phẳng (CaddieId/OverallRating/SkillRatings) thì vẫn nhận.
    /// </summary>
    public async Task CreateRatingAsync(MiniAppCreateCaddieRatingDto input)
    {
        // Validate customer
        if (input.CustomerId == Guid.Empty)
            throw new AbpValidationException("CustomerId không hợp lệ.");

        var customer = await _customerRepo.FindAsync(input.CustomerId);
        if (customer == null)
            throw new AbpValidationException("Không tìm thấy thông tin khách hàng.");

        // Validate booking
        var booking = await _bookingRepo.GetAsync(input.BookingId);

        if (booking.CustomerId != input.CustomerId)
            throw new AbpValidationException("Bạn không có quyền đánh giá booking này.");

        if (booking.Status != (byte)CaddieBookingStatus.Completed)
            throw new AbpValidationException("Chỉ booking đã hoàn thành mới được đánh giá.");

        // Gom danh sách đánh giá: ưu tiên Ratings[]; fallback single-caddie phẳng (client cũ)
        var ratingItems = (input.Ratings != null && input.Ratings.Any())
            ? input.Ratings
            : new List<MiniAppCaddieRatingItemDto>();

        if (ratingItems.Count == 0 && input.OverallRating.HasValue)
        {
            // Backward-compat: lấy caddie phẳng, nếu không có CaddieId thì lấy caddie đầu tiên của booking
            var flatCaddieId = input.CaddieId ?? Guid.Empty;
            if (flatCaddieId == Guid.Empty)
            {
                var primaryDetail = await AsyncExecuter.FirstOrDefaultAsync(
                    (await _bookingDetailRepo.GetQueryableAsync()).Where(d => d.CaddieBookingId == input.BookingId));
                flatCaddieId = primaryDetail?.CaddieId ?? Guid.Empty;
            }
            ratingItems = new List<MiniAppCaddieRatingItemDto>
            {
                new() { CaddieId = flatCaddieId, OverallRating = input.OverallRating.Value, Comment = input.Comment, SkillRatings = input.SkillRatings ?? new() }
            };
        }

        if (ratingItems.Count == 0)
            throw new AbpValidationException("Vui lòng gửi ít nhất 1 đánh giá Caddie.");

        // Các Caddie đã đánh giá trước đó cho booking này → tránh đánh giá trùng
        var alreadyRatedCaddieIds = (await AsyncExecuter.ToListAsync(
                (await _ratingRepo.GetQueryableAsync())
                    .Where(x => x.BookingId == input.BookingId)
                    .Select(x => x.CaddieId)))
            .ToHashSet();

        // Các Caddie hợp lệ thuộc booking này
        var validCaddieIds = (await AsyncExecuter.ToListAsync(
                (await _bookingDetailRepo.GetQueryableAsync())
                    .Where(d => d.CaddieBookingId == input.BookingId)
                    .Select(d => d.CaddieId)))
            .ToHashSet();

        foreach (var item in ratingItems.GroupBy(r => r.CaddieId).Select(g => g.First()))
        {
            if (item.CaddieId == Guid.Empty)
                throw new AbpValidationException("CaddieId không hợp lệ trong danh sách đánh giá.");
            if (item.OverallRating < 1 || item.OverallRating > 5)
                throw new AbpValidationException("Đánh giá phải từ 1 đến 5 sao.");
            if (validCaddieIds.Count > 0 && !validCaddieIds.Contains(item.CaddieId))
                throw new AbpValidationException("Caddie không thuộc booking này.");
            if (alreadyRatedCaddieIds.Contains(item.CaddieId))
                continue; // đã đánh giá caddie này rồi → bỏ qua

            var rating = new AppCaddieRating
            {
                BookingId = input.BookingId,
                CustomerId = input.CustomerId,
                CaddieId = item.CaddieId,
                OverallRating = item.OverallRating,
                Comment = item.Comment,
                ApprovalStatus = (byte)CaddieRatingApprovalStatus.Pending
            };
            await _ratingRepo.InsertAsync(rating, autoSave: true);

            if (item.SkillRatings?.Any() == true)
            {
                var details = item.SkillRatings
                    .Where(s => s.Score >= 1 && s.Score <= 5)
                    .Select(s => new AppCaddieRatingDetail(_guidGenerator.Create(), rating.Id, s.SkillId, s.Score))
                    .ToList();
                if (details.Any())
                    await _ratingDetailRepo.InsertManyAsync(details, autoSave: true);
            }

            alreadyRatedCaddieIds.Add(item.CaddieId);
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

    /// <summary>
    /// GET danh sách ngôn ngữ active (cho Mini App)
    /// </summary>
    public async Task<List<MiniAppLanguageDto>> GetActiveLanguagesAsync()
    {
        var query = (await _languageRepo.GetQueryableAsync())
            .Where(x => x.Status == 1)
            .OrderBy(x => x.SortOrder);
        var items = await AsyncExecuter.ToListAsync(query);

        return items.Select(x => new MiniAppLanguageDto
        {
            Id = x.Id,
            LanguageCode = x.LanguageCode,
            LanguageName = x.LanguageName,
            NativeName = x.NativeName,
            SortOrder = x.SortOrder
        }).ToList();
    }

    private static string GetVoiceRegionText(byte region) => region switch
    {
        (byte)CaddieVoiceRegion.North => "Miền Bắc",
        (byte)CaddieVoiceRegion.Central => "Miền Trung",
        (byte)CaddieVoiceRegion.South => "Miền Nam",
        _ => "Khác"
    };

    /// <summary>Resolve avatar path to full URL using App:AppUrl config</summary>
    private string? ResolveAvatarUrl(string? url)
        => ImageHelper.NormalizeThumb(_configuration, url);
}
