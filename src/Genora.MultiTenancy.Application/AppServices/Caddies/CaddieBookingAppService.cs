using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.Caddie;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.Caddies;

[Authorize]
public class CaddieBookingAppService : ApplicationService
{
    private readonly IRepository<AppCaddieBooking, Guid> _bookingRepo;
    private readonly IRepository<AppCaddieBookingDetail, Guid> _bookingDetailRepo;
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<AppCaddieSchedule, Guid> _scheduleRepo;
    private readonly IRepository<AppCaddieRating, Guid> _ratingRepo;
    private readonly IRepository<AppCaddieRatingDetail, Guid> _ratingDetailRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly IGuidGenerator _guidGenerator;

    public CaddieBookingAppService(
        IRepository<AppCaddieBooking, Guid> bookingRepo,
        IRepository<AppCaddieBookingDetail, Guid> bookingDetailRepo,
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieSchedule, Guid> scheduleRepo,
        IRepository<AppCaddieRating, Guid> ratingRepo,
        IRepository<AppCaddieRatingDetail, Guid> ratingDetailRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IGuidGenerator guidGenerator)
    {
        _bookingRepo = bookingRepo;
        _bookingDetailRepo = bookingDetailRepo;
        _caddieRepo = caddieRepo;
        _scheduleRepo = scheduleRepo;
        _ratingRepo = ratingRepo;
        _ratingDetailRepo = ratingDetailRepo;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        _guidGenerator = guidGenerator;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.IsAvailable ? tenantPerm : hostPerm;

    private async Task EnsureFeatureAsync()
    {
        if (!_currentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(CaddieFeatures.Management))
            throw new AbpAuthorizationException($"Feature '{CaddieFeatures.Management}' is disabled for this tenant.");
    }

    public async Task<PagedResultDto<CaddieBookingDto>> GetListAsync(GetCaddieBookingListInput input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieBookings.Default, MultiTenancyPermissions.HostAppCaddieBookings.Default));

        var query = await _bookingRepo.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var keyword = input.Filter.Trim().ToLower();
            query = query.Where(x =>
                x.BookingCode.ToLower().Contains(keyword) ||
                x.CustomerName.ToLower().Contains(keyword) ||
                x.Phone.Contains(keyword));
        }

        // Filter by CaddieId through BookingDetails
        if (input.CaddieId.HasValue)
        {
            var detailQuery = (await _bookingDetailRepo.GetQueryableAsync())
                .Where(d => d.CaddieId == input.CaddieId.Value)
                .Select(d => d.CaddieBookingId);
            var matchingBookingIds = await AsyncExecuter.ToListAsync(detailQuery);
            query = query.Where(x => matchingBookingIds.Contains(x.Id));
        }

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (input.PaymentStatus.HasValue)
            query = query.Where(x => x.PaymentStatus == input.PaymentStatus.Value);

        if (input.CheckinStatus.HasValue)
            query = query.Where(x => x.CheckinStatus == input.CheckinStatus.Value);

        if (input.FromDate.HasValue)
            query = query.Where(x => x.BookingDate >= input.FromDate.Value);

        if (input.ToDate.HasValue)
            query = query.Where(x => x.BookingDate <= input.ToDate.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "BookingDate DESC, CreationTime DESC" : input.Sorting;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Load booking details to get caddie info
        var bookingIds = items.Select(x => x.Id).ToList();
        var allBookingDetails = await AsyncExecuter.ToListAsync(
            (await _bookingDetailRepo.GetQueryableAsync())
                .Where(d => bookingIds.Contains(d.CaddieBookingId)));

        // Load caddie names from details
        var caddieIds = allBookingDetails.Select(d => d.CaddieId).Distinct().ToList();
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CaddieName, x.CaddieCode });
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        // Load ratings for these bookings
        var ratingsQuery = (await _ratingRepo.GetQueryableAsync())
            .Where(x => bookingIds.Contains(x.BookingId))
            .Select(x => new { x.Id, x.BookingId });
        var ratings = await AsyncExecuter.ToListAsync(ratingsQuery);

        var ratingIds = ratings.Select(r => r.Id).ToList();
        var detailsQuery = (await _ratingDetailRepo.GetQueryableAsync())
            .Where(x => ratingIds.Contains(x.RatingId))
            .Select(x => new { x.RatingId, x.Score });
        var allRatingDetails = await AsyncExecuter.ToListAsync(detailsQuery);

        var bookingRatingMap = new Dictionary<Guid, decimal>();
        foreach (var rating in ratings)
        {
            var details = allRatingDetails.Where(d => d.RatingId == rating.Id).ToList();
            if (details.Count > 0)
                bookingRatingMap[rating.BookingId] = Math.Round((decimal)details.Average(d => d.Score), 1);
        }

        var dtos = items.Select(x =>
        {
            // Get first caddie from booking details (primary)
            var firstDetail = allBookingDetails.FirstOrDefault(d => d.CaddieBookingId == x.Id);
            var caddie = firstDetail != null ? caddies.FirstOrDefault(c => c.Id == firstDetail.CaddieId) : null;
            var dto = MapToDto(x, caddie?.CaddieName, caddie?.CaddieCode, firstDetail?.CaddieId ?? Guid.Empty);
            dto.BookingRatingAvg = bookingRatingMap.TryGetValue(x.Id, out var avg) ? avg : (decimal?)null;
            return dto;
        }).ToList();

        return new PagedResultDto<CaddieBookingDto>(totalCount, dtos);
    }

    public async Task<CaddieBookingDto> GetAsync(Guid id)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieBookings.Default, MultiTenancyPermissions.HostAppCaddieBookings.Default));

        var booking = await _bookingRepo.GetAsync(id);

        // Get primary caddie from details
        var detailQuery = (await _bookingDetailRepo.GetQueryableAsync())
            .Where(d => d.CaddieBookingId == id);
        var firstDetail = await AsyncExecuter.FirstOrDefaultAsync(detailQuery);

        string? caddieName = null;
        string? caddieCode = null;
        Guid caddieId = Guid.Empty;
        if (firstDetail != null)
        {
            var caddie = await _caddieRepo.FindAsync(firstDetail.CaddieId);
            caddieName = caddie?.CaddieName;
            caddieCode = caddie?.CaddieCode;
            caddieId = firstDetail.CaddieId;
        }

        return MapToDto(booking, caddieName, caddieCode, caddieId);
    }

    public async Task UpdateStatusAsync(Guid id, UpdateCaddieBookingStatusDto input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieBookings.Edit, MultiTenancyPermissions.HostAppCaddieBookings.Edit));

        var booking = await _bookingRepo.GetAsync(id);

        if (input.Status.HasValue)
        {
            var newStatus = input.Status.Value;
            ValidateStatusTransition(booking.Status, newStatus);

            if (newStatus == (byte)CaddieBookingStatus.Cancelled && string.IsNullOrWhiteSpace(input.CancelReason))
                throw new UserFriendlyException("Vui lòng nhập lý do hủy booking.");

            booking.Status = newStatus;
            if (newStatus == (byte)CaddieBookingStatus.Cancelled)
            {
                booking.CancelReason = input.CancelReason;
                // Release all schedule slots from details
                await ReleaseAllScheduleSlotsAsync(id);
            }
        }

        if (input.PaymentStatus.HasValue)
            booking.PaymentStatus = input.PaymentStatus.Value;

        if (input.CheckinStatus.HasValue)
        {
            if (booking.Status == (byte)CaddieBookingStatus.Cancelled)
                throw new UserFriendlyException("Không thể check-in booking đã hủy.");

            booking.CheckinStatus = input.CheckinStatus.Value;
            if (input.CheckinStatus.Value == (byte)CaddieCheckinStatus.CheckedIn)
                booking.CheckinTime = DateTime.UtcNow;
        }

        await _bookingRepo.UpdateAsync(booking, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieBookings.Delete, MultiTenancyPermissions.HostAppCaddieBookings.Delete));

        await ReleaseAllScheduleSlotsAsync(id);

        // Delete booking details
        var detailQuery = (await _bookingDetailRepo.GetQueryableAsync())
            .Where(d => d.CaddieBookingId == id);
        var details = await AsyncExecuter.ToListAsync(detailQuery);
        if (details.Any())
            await _bookingDetailRepo.DeleteManyAsync(details, autoSave: true);

        await _bookingRepo.DeleteAsync(id);
    }

    public async Task ChangeCaddyAsync(Guid bookingId, Guid newCaddieId, string? note)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieBookings.Edit, MultiTenancyPermissions.HostAppCaddieBookings.Edit));

        var booking = await _bookingRepo.GetAsync(bookingId);

        if (booking.Status == (byte)CaddieBookingStatus.Completed || booking.Status == (byte)CaddieBookingStatus.Cancelled)
            throw new UserFriendlyException("Không thể đổi Caddy cho booking đã hoàn thành hoặc đã hủy.");

        // Get first detail (primary caddie)
        var detailQuery = (await _bookingDetailRepo.GetQueryableAsync())
            .Where(d => d.CaddieBookingId == bookingId);
        var firstDetail = await AsyncExecuter.FirstOrDefaultAsync(detailQuery);

        if (firstDetail == null)
            throw new UserFriendlyException("Booking không có thông tin caddie.");

        if (firstDetail.CaddieId == newCaddieId)
            throw new UserFriendlyException("Caddy mới phải khác Caddy hiện tại.");

        var newCaddie = await _caddieRepo.GetAsync(newCaddieId);
        if (newCaddie.Status != (byte)CaddieStatus.Active)
            throw new UserFriendlyException("Caddy mới không khả dụng.");

        // Release old schedule slot
        await ReleaseScheduleSlotAsync(firstDetail.ScheduleId);

        // Find available slot for new caddie
        var newScheduleQuery = (await _scheduleRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == newCaddieId
                && x.WorkDate == booking.BookingDate
                && x.SlotStatus == (byte)CaddieSlotStatus.Available
                && x.StartTime <= booking.StartTime
                && x.EndTime > booking.StartTime);
        var newSchedule = await AsyncExecuter.FirstOrDefaultAsync(newScheduleQuery);

        if (newSchedule == null)
            throw new UserFriendlyException("Caddy mới không có lịch trống vào thời gian này. Vui lòng tạo lịch trước.");

        // Update detail
        firstDetail.CaddieId = newCaddieId;
        firstDetail.ScheduleId = newSchedule.Id;
        await _bookingDetailRepo.UpdateAsync(firstDetail, autoSave: true);

        if (!string.IsNullOrWhiteSpace(note))
        {
            booking.Note = (booking.Note ?? "") + $"\n[Đổi caddy: {note}]";
            await _bookingRepo.UpdateAsync(booking, autoSave: true);
        }

        // Lock new schedule slot
        newSchedule.SlotStatus = (byte)CaddieSlotStatus.Booked;
        newSchedule.BookingId = booking.Id;
        await _scheduleRepo.UpdateAsync(newSchedule, autoSave: true);
    }

    private void ValidateStatusTransition(byte currentStatus, byte newStatus)
    {
        var current = (CaddieBookingStatus)currentStatus;
        var next = (CaddieBookingStatus)newStatus;

        var valid = (current, next) switch
        {
            (CaddieBookingStatus.New, CaddieBookingStatus.Confirmed) => true,
            (CaddieBookingStatus.New, CaddieBookingStatus.Cancelled) => true,
            (CaddieBookingStatus.Confirmed, CaddieBookingStatus.Completed) => true,
            (CaddieBookingStatus.Confirmed, CaddieBookingStatus.Cancelled) => true,
            _ => false
        };

        if (!valid)
            throw new UserFriendlyException($"Không thể chuyển trạng thái từ '{GetStatusText(currentStatus)}' sang '{GetStatusText(newStatus)}'.");
    }

    private async Task ReleaseAllScheduleSlotsAsync(Guid bookingId)
    {
        var detailQuery = (await _bookingDetailRepo.GetQueryableAsync())
            .Where(d => d.CaddieBookingId == bookingId);
        var details = await AsyncExecuter.ToListAsync(detailQuery);

        foreach (var detail in details)
        {
            await ReleaseScheduleSlotAsync(detail.ScheduleId);
        }
    }

    private async Task ReleaseScheduleSlotAsync(Guid scheduleId)
    {
        try
        {
            var schedule = await _scheduleRepo.GetAsync(scheduleId);
            schedule.SlotStatus = (byte)CaddieSlotStatus.Available;
            schedule.BookingId = null;
            await _scheduleRepo.UpdateAsync(schedule, autoSave: true);
        }
        catch { /* Schedule may not exist */ }
    }

    private static CaddieBookingDto MapToDto(AppCaddieBooking entity, string? caddieName, string? caddieCode, Guid caddieId)
    {
        return new CaddieBookingDto
        {
            Id = entity.Id,
            BookingCode = entity.BookingCode,
            CustomerId = entity.CustomerId,
            CustomerName = entity.CustomerName,
            Phone = entity.Phone,
            PhoneMasked = MaskPhone(entity.Phone),
            GolfCourseId = entity.GolfCourseId,
            CaddieId = caddieId,
            CaddieName = caddieName,
            CaddieCode = caddieCode,
            BookingDate = entity.BookingDate,
            StartTime = entity.StartTime,
            NumberOfHoles = entity.NumberOfHoles,
            Note = entity.Note,
            Status = entity.Status,
            StatusText = GetStatusText(entity.Status),
            PaymentStatus = entity.PaymentStatus,
            PaymentStatusText = entity.PaymentStatus switch
            {
                (byte)CaddiePaymentStatus.Unpaid => "Chưa thanh toán",
                (byte)CaddiePaymentStatus.Paid => "Đã thanh toán",
                _ => "Khác"
            },
            TotalCaddieFee = entity.TotalCaddieFee,
            PaymentMethod = entity.PaymentMethod,
            PaymentMethodText = entity.PaymentMethod switch
            {
                0 => "Thanh toán tại quầy",
                1 => "Thanh toán online",
                2 => "Chuyển khoản",
                _ => "Khác"
            },
            CheckinStatus = entity.CheckinStatus,
            CheckinStatusText = entity.CheckinStatus switch
            {
                (byte)CaddieCheckinStatus.NotCheckedIn => "Chưa check-in",
                (byte)CaddieCheckinStatus.CheckedIn => "Đã check-in",
                _ => "Khác"
            },
            CheckinTime = entity.CheckinTime,
            CancelReason = entity.CancelReason,
            CreationTime = entity.CreationTime
        };
    }

    private static string GetStatusText(byte status) => status switch
    {
        (byte)CaddieBookingStatus.New => "Mới",
        (byte)CaddieBookingStatus.Confirmed => "Đã xác nhận",
        (byte)CaddieBookingStatus.Completed => "Hoàn thành",
        (byte)CaddieBookingStatus.Cancelled => "Đã hủy",
        _ => "Khác"
    };

    private static string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
            return phone;
        return phone[..3] + " " + phone.Substring(3, 3) + " " + new string('x', phone.Length - 7) + phone[^1];
    }
}
