using System;
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
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly IRepository<AppCaddieSchedule, Guid> _scheduleRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly IGuidGenerator _guidGenerator;

    public CaddieBookingAppService(
        IRepository<AppCaddieBooking, Guid> bookingRepo,
        IRepository<AppCaddie, Guid> caddieRepo,
        IRepository<AppCaddieSchedule, Guid> scheduleRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IGuidGenerator guidGenerator)
    {
        _bookingRepo = bookingRepo;
        _caddieRepo = caddieRepo;
        _scheduleRepo = scheduleRepo;
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

        if (input.CaddieId.HasValue)
            query = query.Where(x => x.CaddieId == input.CaddieId.Value);

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

        // Load caddie names
        var caddieIds = items.Select(x => x.CaddieId).Distinct().ToList();
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CaddieName, x.CaddieCode });
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        var dtos = items.Select(x =>
        {
            var caddie = caddies.FirstOrDefault(c => c.Id == x.CaddieId);
            return MapToDto(x, caddie?.CaddieName, caddie?.CaddieCode);
        }).ToList();

        return new PagedResultDto<CaddieBookingDto>(totalCount, dtos);
    }

    public async Task<CaddieBookingDto> GetAsync(Guid id)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieBookings.Default, MultiTenancyPermissions.HostAppCaddieBookings.Default));

        var booking = await _bookingRepo.GetAsync(id);
        var caddie = await _caddieRepo.GetAsync(booking.CaddieId);
        return MapToDto(booking, caddie.CaddieName, caddie.CaddieCode);
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

            // BR-04: Cancel requires reason
            if (newStatus == (byte)CaddieBookingStatus.Cancelled && string.IsNullOrWhiteSpace(input.CancelReason))
                throw new UserFriendlyException("Vui lòng nhập lý do hủy booking.");

            booking.Status = newStatus;
            if (newStatus == (byte)CaddieBookingStatus.Cancelled)
            {
                booking.CancelReason = input.CancelReason;
                // Release schedule slot
                await ReleaseScheduleSlotAsync(booking.ScheduleId);
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

        var booking = await _bookingRepo.GetAsync(id);
        await ReleaseScheduleSlotAsync(booking.ScheduleId);
        await _bookingRepo.DeleteAsync(id);
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

    private static CaddieBookingDto MapToDto(AppCaddieBooking entity, string? caddieName, string? caddieCode)
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
            CaddieId = entity.CaddieId,
            CaddieName = caddieName,
            CaddieCode = caddieCode,
            ScheduleId = entity.ScheduleId,
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
