using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using SalonBeautyStylistRole = Genora.MultiTenancy.Enums.SalonBeautyStylistRole;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.AppServices;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyBookingAppService :
    FeatureProtectedCrudAppService<
        SalonBeautyBooking,
        SalonBeautyBookingDetailDto,
        Guid,
        GetSalonBeautyBookingListInput,
        CreateSalonBeautyBookingDto,
        UpdateSalonBeautyBookingDto>,
    ISalonBeautyBookingAppService
{
    protected override string FeatureName => string.Empty;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.SalonBeautyBookings.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostSalonBeautyBookings.Default;
    private const string InternalNoteSeparator = "\n---INTERNAL---\n";

    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepository;
    private readonly IRepository<SalonBeautyBookingService, Guid> _bookingServiceRepository;
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyService, Guid> _serviceRepository;
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _categoryRepository;
    private readonly IRepository<SalonBeautyStylist, Guid> _stylistRepository;
    private readonly IRepository<SalonBeautyLocation, Guid> _locationRepository;
    private readonly IRepository<SalonBeautyTimeSlot, Guid> _timeSlotRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _loyaltyRepository;
    private readonly IBackgroundJobManager _jobManager;
    private readonly ILogger<SalonBeautyBookingAppService> _logger;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    private const string ZaloDateFormat = "dd/MM/yyyy";

    public SalonBeautyBookingAppService(
        IRepository<SalonBeautyBooking, Guid> bookingRepository,
        IRepository<SalonBeautyBookingService, Guid> bookingServiceRepository,
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyService, Guid> serviceRepository,
        IRepository<SalonBeautyServiceCategory, Guid> categoryRepository,
        IRepository<SalonBeautyStylist, Guid> stylistRepository,
        IRepository<SalonBeautyLocation, Guid> locationRepository,
        IRepository<SalonBeautyTimeSlot, Guid> timeSlotRepository,
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> loyaltyRepository,
        IBackgroundJobManager jobManager,
        ILogger<SalonBeautyBookingAppService> logger,
        IStringLocalizer<MultiTenancyResource> l,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(bookingRepository, currentTenant, featureChecker)
    {
        _bookingRepository = bookingRepository;
        _bookingServiceRepository = bookingServiceRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _stylistRepository = stylistRepository;
        _locationRepository = locationRepository;
        _timeSlotRepository = timeSlotRepository;
        _loyaltyRepository = loyaltyRepository;
        _jobManager = jobManager;
        _logger = logger;
        _l = l;
    }

    public override async Task<PagedResultDto<SalonBeautyBookingDetailDto>> GetListAsync(GetSalonBeautyBookingListInput input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _bookingRepository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(),
            x => (x.BookingCode != null && x.BookingCode.Contains(input.FilterText!)));
        query = query.WhereIf(input.LocationId.HasValue, x => x.LocationId == input.LocationId);
        query = query.WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId);
        query = query.WhereIf(input.StylistId.HasValue, x => x.StylistId == input.StylistId);
        query = query.WhereIf(input.Status.HasValue, x => (byte)x.Status == input.Status);
        query = query.WhereIf(input.PaymentStatus.HasValue, x => (byte)x.PaymentStatus == input.PaymentStatus);
        query = query.WhereIf(input.FromDate.HasValue, x => x.BookingDate >= input.FromDate!.Value.Date);
        query = query.WhereIf(input.ToDate.HasValue, x => x.BookingDate <= input.ToDate!.Value.Date);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderByDescending(x => x.BookingDate)
            .ThenByDescending(x => x.StartTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        var dtos = new List<SalonBeautyBookingDetailDto>();
        var bookingIds = items.Select(x => x.Id).ToList();
        var serviceMap = await BuildBookingItemsMapAsync(bookingIds);

        foreach (var item in items)
        {
            dtos.Add(await MapToBookingListDetailDto(item, serviceMap.GetValueOrDefault(item.Id) ?? new List<SalonBeautyBookingService>()));
        }

        return new PagedResultDto<SalonBeautyBookingDetailDto>
        {
            TotalCount = totalCount,
            Items = dtos
        };
    }

    public override async Task<SalonBeautyBookingDetailDto> GetAsync(Guid id)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);
        var booking = await _bookingRepository.GetAsync(id);
        return await MapToBookingDetailDto(booking);
    }

    public override async Task<SalonBeautyBookingDetailDto> CreateAsync(CreateSalonBeautyBookingDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Create,
            MultiTenancyPermissions.HostSalonBeautyBookings.Create);

        ValidateBookingItems(input.Items);

        // Nếu có TimeSlotId → lấy WorkDate / StartTime / EndTime từ slot, override input
        var bookingDate = input.BookingDate.Date;
        var startTime = input.StartTime;
        TimeSpan? endTime = input.EndTime;
        Guid? locationId = input.LocationId;
        SalonBeautyTimeSlot? timeSlot = null;
        if (input.TimeSlotId.HasValue && input.TimeSlotId.Value != Guid.Empty)
        {
            timeSlot = await _timeSlotRepository.GetAsync(input.TimeSlotId.Value);
            if (timeSlot.Status == SalonBeautyTimeSlotStatus.Off || timeSlot.Status == SalonBeautyTimeSlotStatus.Full)
                throw new UserFriendlyException("Khung giờ đã bị tắt hoặc đã đầy.");
            if (timeSlot.BookedCount >= timeSlot.Capacity)
                throw new UserFriendlyException("Khung giờ đã đầy.");

            bookingDate = timeSlot.WorkDate.Date;
            startTime = timeSlot.StartTime;
            endTime = timeSlot.EndTime;
            locationId = timeSlot.LocationId;
        }

        var resolved = await ResolveItemsAsync(input.Items);
        var totalDuration = resolved.Sum(x => x.Duration);
        endTime = endTime ?? startTime.Add(TimeSpan.FromMinutes(totalDuration));
        var subTotal = resolved.Sum(x => x.Price);
        var totalAmount = subTotal + (input.Surcharge ?? 0m) - (input.Discount ?? 0m);
        if (totalAmount < 0) totalAmount = 0;

        var bookingId = GuidGenerator.Create();

        var booking = new SalonBeautyBooking(
            bookingId,
            GenerateBookingCode(),
            input.CustomerId,
            resolved.First().ServiceId,
            input.StylistId,
            bookingDate,
            startTime,
            endTime.Value,
            totalAmount,
            SalonBeautyBookingStatus.New,
            SalonBeautyPaymentStatus.Unpaid,
            SalonBeautyCheckinStatus.NotCheckedIn,
            PackNote(input.CustomerNote, input.InternalNote),
            CurrentTenant.Id
        );
        booking.LocationId = locationId;
        booking.TimeSlotId = input.TimeSlotId;

        await _bookingRepository.InsertAsync(booking, autoSave: true);

        foreach (var item in resolved)
        {
            await _bookingServiceRepository.InsertAsync(new SalonBeautyBookingService
            {
                BookingId = bookingId,
                ServiceId = item.ServiceId,
                Price = item.Price,
                Duration = item.Duration,
                TenantId = CurrentTenant.Id
            }, autoSave: true);
        }

        // Tăng BookedCount của time slot nếu có
        if (timeSlot != null)
        {
            timeSlot.BookedCount += 1;
            if (timeSlot.BookedCount >= timeSlot.Capacity && !timeSlot.IsManualOverride)
            {
                timeSlot.Status = SalonBeautyTimeSlotStatus.Full;
            }
            await _timeSlotRepository.UpdateAsync(timeSlot, autoSave: true);
        }

        await EnqueueBookingCreatedZbsAsync(booking);

        return await MapToBookingDetailDto(booking);
    }



    public override async Task<SalonBeautyBookingDetailDto> UpdateAsync(Guid id, UpdateSalonBeautyBookingDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Edit,
            MultiTenancyPermissions.HostSalonBeautyBookings.Edit);

        ValidateBookingItems(input.Items);

        var booking = await _bookingRepository.GetAsync(id);
        EnsureBookingCanBeEdited(booking);

        var oldTimeSlotId = booking.TimeSlotId;

        // Nếu TimeSlotId thay đổi → lấy WorkDate / StartTime / EndTime / LocationId từ slot mới, override input
        var bookingDate = input.BookingDate.Date;
        var startTime = input.StartTime;
        TimeSpan? endTime = input.EndTime;
        var locationId = input.LocationId;
        SalonBeautyTimeSlot? newSlot = null;
        if (input.TimeSlotId.HasValue && input.TimeSlotId.Value != Guid.Empty
            && input.TimeSlotId.Value != oldTimeSlotId)
        {
            newSlot = await _timeSlotRepository.GetAsync(input.TimeSlotId.Value);
            if (newSlot.Status == SalonBeautyTimeSlotStatus.Off || newSlot.Status == SalonBeautyTimeSlotStatus.Full)
                throw new UserFriendlyException("Khung giờ đã bị tắt hoặc đã đầy.");
            if (newSlot.BookedCount >= newSlot.Capacity)
                throw new UserFriendlyException("Khung giờ đã đầy.");

            bookingDate = newSlot.WorkDate.Date;
            startTime = newSlot.StartTime;
            endTime = newSlot.EndTime;
            locationId = newSlot.LocationId;
        }

        var resolved = await ResolveItemsAsync(input.Items);
        var totalDuration = resolved.Sum(x => x.Duration);
        endTime = endTime ?? startTime.Add(TimeSpan.FromMinutes(totalDuration));
        var subTotal = resolved.Sum(x => x.Price);
        var totalAmount = subTotal + (input.Surcharge ?? 0m) - (input.Discount ?? 0m);

        if (totalAmount < 0)
        {
            totalAmount = 0;
        }

        booking.LocationId = locationId;
        booking.CustomerId = input.CustomerId;
        booking.StylistId = input.StylistId;
        booking.ServiceId = resolved.First().ServiceId;
        booking.BookingDate = bookingDate;
        booking.StartTime = startTime;
        booking.EndTime = endTime.Value;
        booking.TotalAmount = totalAmount;
        booking.Status = input.Status;
        booking.TimeSlotId = input.TimeSlotId;
        booking.Note = PackNote(input.CustomerNote, input.InternalNote);

        await _bookingRepository.UpdateAsync(booking, autoSave: true);

        // Đồng bộ BookedCount nếu TimeSlotId đổi
        if (oldTimeSlotId != input.TimeSlotId)
        {
            if (oldTimeSlotId.HasValue && oldTimeSlotId.Value != Guid.Empty)
            {
                var oldSlot = await _timeSlotRepository.FindAsync(oldTimeSlotId.Value);
                if (oldSlot != null)
                {
                    oldSlot.BookedCount = Math.Max(0, oldSlot.BookedCount - 1);
                    if (oldSlot.Status == SalonBeautyTimeSlotStatus.Full
                        && oldSlot.BookedCount < oldSlot.Capacity
                        && !oldSlot.IsManualOverride)
                    {
                        oldSlot.Status = SalonBeautyTimeSlotStatus.On;
                    }
                    await _timeSlotRepository.UpdateAsync(oldSlot, autoSave: true);
                }
            }

            if (newSlot != null)
            {
                newSlot.BookedCount += 1;
                if (newSlot.BookedCount >= newSlot.Capacity && !newSlot.IsManualOverride)
                {
                    newSlot.Status = SalonBeautyTimeSlotStatus.Full;
                }
                await _timeSlotRepository.UpdateAsync(newSlot, autoSave: true);
            }
        }

        var existing = await AsyncExecuter.ToListAsync(
            (await _bookingServiceRepository.GetQueryableAsync())
                .Where(x => x.BookingId == id)
        );

        foreach (var old in existing)
        {
            await _bookingServiceRepository.DeleteAsync(old.Id, autoSave: true);
        }

        foreach (var item in resolved)
        {
            await _bookingServiceRepository.InsertAsync(new SalonBeautyBookingService
            {
                BookingId = id,
                ServiceId = item.ServiceId,
                Price = item.Price,
                Duration = item.Duration,
                TenantId = CurrentTenant.Id
            }, autoSave: true);
        }

        return await MapToBookingDetailDto(booking);
    }

    public async Task<SalonBeautyBookingDetailDto> UpdateStatusAsync(Guid id, UpdateBookingStatusDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Edit,
            MultiTenancyPermissions.HostSalonBeautyBookings.Edit);

        var booking = await _bookingRepository.GetAsync(id);

        if (booking.Status == SalonBeautyBookingStatus.Cancelled)
        {
            throw new UserFriendlyException("Booking đã hủy không thể cập nhật trạng thái.");
        }

        if (booking.Status == SalonBeautyBookingStatus.Completed)
        {
            throw new UserFriendlyException("Booking đã hoàn thành không thể cập nhật trạng thái.");
        }

        if (!IsValidNextStatus(booking.Status, input.Status))
        {
            throw new UserFriendlyException("Không được nhảy trạng thái. Vui lòng cập nhật theo đúng luồng trạng thái.");
        }

        booking.Status = input.Status;

        var internalNote = GetOptionalStringProperty(input, "Note")
            ?? GetOptionalStringProperty(input, "InternalNote")
            ?? GetOptionalStringProperty(input, "Reason");

        if (!internalNote.IsNullOrWhiteSpace())
        {
            booking.Note = AppendInternalNote(booking.Note, internalNote);
        }

        if (input.Status == SalonBeautyBookingStatus.Completed
            && booking.CheckinStatus != SalonBeautyCheckinStatus.CheckedIn)
        {
            booking.CheckinStatus = SalonBeautyCheckinStatus.CheckedIn;
            booking.CheckinTime ??= DateTime.Now;
        }

        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);

        if (input.Status == SalonBeautyBookingStatus.Completed)
        {
            await EnqueueServiceReviewZbsAsync(updated);
        }

        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingDetailDto> CheckinAsync(Guid id)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Checkin,
            MultiTenancyPermissions.HostSalonBeautyBookings.Checkin);

        var booking = await _bookingRepository.GetAsync(id);
        booking.CheckinStatus = SalonBeautyCheckinStatus.CheckedIn;
        booking.CheckinTime = DateTime.Now;

        if (booking.Status == SalonBeautyBookingStatus.New)
        {
            booking.Status = SalonBeautyBookingStatus.Confirmed;
        }
        else if (booking.Status == SalonBeautyBookingStatus.Confirmed)
        {
            booking.Status = SalonBeautyBookingStatus.Processing;
        }

        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);
        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingDetailDto> UpdatePaymentAsync(Guid id, UpdateBookingPaymentDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.UpdatePayment,
            MultiTenancyPermissions.HostSalonBeautyBookings.UpdatePayment);

        var booking = await _bookingRepository.GetAsync(id);
        booking.PaymentStatus = input.PaymentStatus;
        booking.PaymentMethod = input.PaymentMethod;

        if (input.PaymentStatus == SalonBeautyPaymentStatus.Paid
            && booking.CheckinStatus == SalonBeautyCheckinStatus.CheckedIn)
        {
            booking.Status = SalonBeautyBookingStatus.Completed;
        }

        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);
        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingDetailDto> CancelAsync(Guid id, CancelBookingDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Cancel,
            MultiTenancyPermissions.HostSalonBeautyBookings.Cancel);

        var booking = await _bookingRepository.GetAsync(id);

        if (booking.Status == SalonBeautyBookingStatus.Completed)
        {
            throw new UserFriendlyException("Booking đã hoàn thành không thể hủy");
        }

        if (booking.Status == SalonBeautyBookingStatus.Cancelled)
        {
            throw new UserFriendlyException("Booking đã được hủy trước đó.");
        }

        if (booking.Status != SalonBeautyBookingStatus.New
            && booking.Status != SalonBeautyBookingStatus.Confirmed
            && booking.Status != SalonBeautyBookingStatus.Processing)
        {
            throw new UserFriendlyException("Chỉ được hủy booking ở trạng thái Chờ xác nhận, Đã xác nhận hoặc Đang thực hiện.");
        }

        if (input.CancelNote.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("Vui lòng nhập lý do");
        }

        booking.Status = SalonBeautyBookingStatus.Cancelled;
        booking.CancelReason = input.CancelReason;
        booking.CancelNote = input.CancelNote!.Trim();

        if (booking.PaymentStatus == SalonBeautyPaymentStatus.Paid)
        {
            booking.PaymentStatus = SalonBeautyPaymentStatus.Refunded;
        }

        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);
        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingDetailDto> ChangeStylistAsync(Guid id, ChangeBookingStylistDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Edit,
            MultiTenancyPermissions.HostSalonBeautyBookings.Edit);

        if (input == null || input.StylistId == Guid.Empty)
            throw new UserFriendlyException("Vui lòng chọn nhân viên (stylist).");

        var booking = await _bookingRepository.GetAsync(id);

        if (booking.Status == SalonBeautyBookingStatus.Cancelled
            || booking.Status == SalonBeautyBookingStatus.Completed)
        {
            throw new UserFriendlyException("Booking đã hoàn thành/hủy không thể đổi stylist.");
        }

        if (booking.StylistId == input.StylistId)
            return await MapToBookingDetailDto(booking);

        var newStylist = await _stylistRepository.FindAsync(input.StylistId)
            ?? throw new UserFriendlyException("Không tìm thấy nhân viên (stylist).");

        if (booking.LocationId.HasValue && newStylist.LocationId.HasValue
            && newStylist.LocationId.Value != booking.LocationId.Value)
        {
            throw new UserFriendlyException("Stylist không thuộc cơ sở của booking này.");
        }

        var oldStylist = await _stylistRepository.FindAsync(booking.StylistId);

        booking.StylistId = newStylist.Id;

        var noteText = $"Đổi stylist: {oldStylist?.DisplayName ?? "--"} → {newStylist.DisplayName}";
        if (!input.Note.IsNullOrWhiteSpace())
            noteText += $" ({input.Note!.Trim()})";

        booking.Note = AppendInternalNote(booking.Note, noteText);

        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);
        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingHistoryPageDto> GetHistoryPageAsync(GetSalonBeautyBookingHistoryInput input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        if (input == null || input.BookingId == Guid.Empty)
            throw new UserFriendlyException("Thiếu mã đặt lịch.");

        var booking = await _bookingRepository.GetAsync(input.BookingId);
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var allActivities = BuildActivities(booking);

        var actionTypeOptions = new List<SalonBeautyBookingHistoryActionTypeOptionDto>
        {
            new() { Value = "",        Text = "Tất cả thao tác" },
            new() { Value = "create",  Text = "Khởi tạo" },
            new() { Value = "status",  Text = "Cập nhật trạng thái" },
            new() { Value = "checkin", Text = "Check-in" },
            new() { Value = "stylist", Text = "Đổi stylist" },
            new() { Value = "cancel",  Text = "Hủy lịch" }
        };

        var filtered = allActivities.AsEnumerable();
        if (!input.ActionType.IsNullOrWhiteSpace())
        {
            var key = input.ActionType!.Trim().ToLowerInvariant();
            filtered = filtered.Where(a => ResolveActionTypeKey(a.Title) == key);
        }

        var totalCount = filtered.Count();
        var skip = Math.Max(0, input.SkipCount);
        var take = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var items = filtered
            .OrderByDescending(x => x.Time)
            .Skip(skip)
            .Take(take)
            .Select(a =>
            {
                var key = ResolveActionTypeKey(a.Title);
                return new SalonBeautyBookingHistoryItemDto
                {
                    Time = a.Time,
                    PerformedBy = "System",
                    ActionType = key,
                    ActionTypeText = ResolveActionTypeText(key),
                    ActionTypeClass = ResolveActionTypeClass(key, a.IsDanger),
                    Title = a.Title,
                    Description = a.Description ?? string.Empty,
                    IsDanger = a.IsDanger
                };
            })
            .ToList();

        return new SalonBeautyBookingHistoryPageDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            CustomerName = customer?.Name,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(customer?.Phone),
            Status = booking.Status,
            StatusText = GetBookingStatusText(booking.Status),
            CreationTime = booking.CreationTime,
            LastActivityTime = allActivities.Count == 0 ? null : allActivities.Max(x => x.Time),
            TotalActions = allActivities.Count,
            ActionTypeOptions = actionTypeOptions,
            PagedActivities = new PagedResultDto<SalonBeautyBookingHistoryItemDto>(totalCount, items)
        };
    }

    private static string ResolveActionTypeKey(string title)
    {
        if (title.IsNullOrWhiteSpace()) return "other";
        var t = title.ToLowerInvariant();
        if (t.Contains("khởi tạo") || t.Contains("created")) return "create";
        if (t.Contains("đổi stylist") || t.Contains("stylist")) return "stylist";
        if (t.Contains("check-in") || t.Contains("checkin")) return "checkin";
        if (t.Contains("hủy")) return "cancel";
        if (t.Contains("trạng thái") || t.StartsWith("status")) return "status";
        return "other";
    }

    private static string ResolveActionTypeText(string key) => key switch
    {
        "create"  => "Khởi tạo",
        "status"  => "Cập nhật trạng thái",
        "checkin" => "Check-in",
        "stylist" => "Đổi stylist",
        "cancel"  => "Hủy lịch",
        _         => "Thao tác khác"
    };

    private static string ResolveActionTypeClass(string key, bool isDanger)
    {
        if (isDanger) return "is-danger";
        return key switch
        {
            "create"  => "is-info",
            "status"  => "is-primary",
            "checkin" => "is-success",
            "stylist" => "is-warning",
            "cancel"  => "is-danger",
            _         => "is-muted"
        };
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Delete,
            MultiTenancyPermissions.HostSalonBeautyBookings.Delete);

        var booking = await _bookingRepository.GetAsync(id);

        if (booking.Status == SalonBeautyBookingStatus.Completed)
        {
            throw new UserFriendlyException("Booking đã hoàn thành không thể xóa.");
        }

        if (booking.Status == SalonBeautyBookingStatus.Cancelled)
        {
            throw new UserFriendlyException("Booking đã hủy không thể xóa.");
        }

        await base.DeleteAsync(id);
    }

    public async Task<List<SalonBeautyBookingCalendarDto>> GetCalendarEventsAsync(DateTime from, DateTime to, Guid? stylistId = null, Guid? serviceId = null, Guid? locationId = null)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _bookingRepository.GetQueryableAsync();
        query = query.Where(x => x.BookingDate >= from.Date && x.BookingDate <= to.Date);

        if (stylistId.HasValue)
        {
            query = query.Where(x => x.StylistId == stylistId.Value);
        }

        if (serviceId.HasValue)
        {
            query = query.Where(x => x.ServiceId == serviceId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(x => x.LocationId == locationId.Value);
        }

        var bookings = await AsyncExecuter.ToListAsync(query);
        var ids = bookings.Select(x => x.Id).ToList();
        var itemsMap = await BuildBookingItemsMapAsync(ids);
        var locationIds = bookings.Where(x => x.LocationId.HasValue).Select(x => x.LocationId!.Value).Distinct().ToList();
        var locationMap = await BuildLocationMapAsync(locationIds);

        var result = new List<SalonBeautyBookingCalendarDto>();
        foreach (var booking in bookings)
        {
            var customer = await _customerRepository.FindAsync(booking.CustomerId);
            var stylist = await _stylistRepository.FindAsync(booking.StylistId);

            var items = itemsMap.GetValueOrDefault(booking.Id) ?? new List<SalonBeautyBookingService>();
            var serviceName = await BuildServiceSummaryAsync(items, booking.ServiceId);

            var startDateTime = booking.BookingDate.Date + booking.StartTime;
            var endDateTime = booking.BookingDate.Date + booking.EndTime;

            result.Add(new SalonBeautyBookingCalendarDto
            {
                Id = booking.Id,
                BookingCode = booking.BookingCode,
                LocationId = booking.LocationId,
                LocationName = booking.LocationId.HasValue && locationMap.TryGetValue(booking.LocationId.Value, out var locName) ? locName : null,
                CustomerId = booking.CustomerId,
                CustomerName = customer?.Name ?? "--",
                CustomerPhone = customer?.Phone,
                StylistId = booking.StylistId,
                StylistName = stylist?.DisplayName ?? "--",
                ServiceName = serviceName,
                ServiceCount = items.Count > 0 ? items.Count : 1,
                Start = startDateTime,
                End = endDateTime,
                Status = booking.Status.ToString(),
                StatusText = GetBookingStatusText(booking.Status),
                StatusColor = GetStatusColor(booking.Status),
                TotalAmount = booking.TotalAmount
            });
        }

        return result;
    }

    public async Task<BookingStatisticsDto> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _bookingRepository.GetQueryableAsync();

        if (fromDate.HasValue)
            query = query.Where(x => x.BookingDate >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(x => x.BookingDate <= toDate.Value.Date);

        var bookings = await AsyncExecuter.ToListAsync(query);

        var totalBookings = bookings.Count;
        var newCount = bookings.Count(x => x.Status == SalonBeautyBookingStatus.New);
        var confirmedCount = bookings.Count(x => x.Status == SalonBeautyBookingStatus.Confirmed);
        var processingCount = bookings.Count(x => x.Status == SalonBeautyBookingStatus.Processing);
        var completedCount = bookings.Count(x => x.Status == SalonBeautyBookingStatus.Completed);
        var cancelledCount = bookings.Count(x => x.Status == SalonBeautyBookingStatus.Cancelled);
        var totalValue = bookings
            .Where(x => x.Status != SalonBeautyBookingStatus.Cancelled)
            .Sum(x => x.TotalAmount);

        var settled = bookings.Count(x => x.Status == SalonBeautyBookingStatus.Completed
            || x.Status == SalonBeautyBookingStatus.Cancelled);
        var completionRate = settled > 0 ? (decimal)completedCount / settled * 100 : 0;

        var newUnprocessed = bookings.Count(x =>
            x.Status == SalonBeautyBookingStatus.New
            && x.CheckinStatus == SalonBeautyCheckinStatus.NotCheckedIn);

        return new BookingStatisticsDto
        {
            TotalBookings = totalBookings,
            TotalBookingsChangePercent = 0m,
            TotalValue = totalValue,
            TotalValueChangePercent = 0m,
            CompletionRate = Math.Round(completionRate, 1),
            CompletionTrendText = "Ổn định",
            PendingCount = newCount,
            ConfirmedCount = confirmedCount,
            ProcessingCount = processingCount,
            CompletedCount = completedCount,
            CancelledCount = cancelledCount,
            NewUnprocessedCount = newUnprocessed
        };
    }

    public async Task<List<SalonBeautyCustomerLookupDto>> GetCustomerLookupAsync(string? filter)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _customerRepository.GetQueryableAsync();
        if (!filter.IsNullOrWhiteSpace())
        {
            var keyword = filter!.Trim().ToLower();
            query = query.Where(x =>
                (x.Name != null && x.Name.ToLower().Contains(keyword)) ||
                (x.Phone != null && x.Phone.Contains(keyword)));
        }

        var customers = await AsyncExecuter.ToListAsync(query
            .Where(x => x.Status == 1)
            .OrderBy(x => x.Name)
            .Take(50));

        return customers.Select(x => new SalonBeautyCustomerLookupDto
        {
            Id = x.Id,
            Name = x.Name,
            Phone = x.Phone,
            Avatar = x.Avatar,
            Code = x.CustomerCode
        }).ToList();
    }

    public async Task<List<SalonBeautyServiceLookupDto>> GetServiceLookupAsync()
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _serviceRepository.GetQueryableAsync();
        var services = await AsyncExecuter.ToListAsync(query
            .Where(x => x.Status == 1)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name));

        var categoryIds = services.Select(x => x.CategoryId).Distinct().ToList();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();
        var categories = await AsyncExecuter.ToListAsync(categoryQuery.Where(x => categoryIds.Contains(x.Id)));
        var categoryMap = categories.ToDictionary(x => x.Id, x => x.Name);

        return services.Select(x => new SalonBeautyServiceLookupDto
        {
            Id = x.Id,
            Name = x.Name,
            CategoryName = categoryMap.GetValueOrDefault(x.CategoryId),
            Price = x.Price,
            Duration = x.Duration
        }).ToList();
    }

    public async Task<List<SalonBeautyStylistLookupDto>> GetStylistLookupAsync(Guid? locationId = null)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _stylistRepository.GetQueryableAsync();
        query = query.Where(x => x.Status == 1);
        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        var stylists = await AsyncExecuter.ToListAsync(query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.DisplayName));

        return stylists.Select(x => new SalonBeautyStylistLookupDto
        {
            Id = x.Id,
            LocationId = x.LocationId,
            DisplayName = x.DisplayName,
            Avatar = x.Avatar,
            Role = x.Role,
            RoleText = x.Role.HasValue ? GetStylistRoleText((SalonBeautyStylistRole)x.Role.Value) : null
        }).ToList();
    }

    // ─────────── Helpers ───────────

    private static void ValidateBookingItems(List<CreateSalonBeautyBookingItemDto> items)
    {
        if (items == null || items.Count == 0)
        {
            throw new UserFriendlyException("Cần chọn ít nhất một dịch vụ cho đơn đặt lịch.");
        }
    }

    private async Task<List<CreateSalonBeautyBookingItemDto>> ResolveItemsAsync(List<CreateSalonBeautyBookingItemDto> items)
    {
        var serviceIds = items.Select(x => x.ServiceId).Distinct().ToList();
        var serviceQuery = await _serviceRepository.GetQueryableAsync();
        var services = await AsyncExecuter.ToListAsync(serviceQuery.Where(x => serviceIds.Contains(x.Id)));
        var map = services.ToDictionary(x => x.Id);

        var resolved = new List<CreateSalonBeautyBookingItemDto>();
        foreach (var item in items)
        {
            if (!map.TryGetValue(item.ServiceId, out var service))
            {
                throw new UserFriendlyException($"Dịch vụ không tồn tại: {item.ServiceId}");
            }

            resolved.Add(new CreateSalonBeautyBookingItemDto
            {
                ServiceId = item.ServiceId,
                StylistId = item.StylistId,
                Price = item.Price > 0 ? item.Price : service.Price,
                Duration = item.Duration > 0 ? item.Duration : service.Duration
            });
        }

        return resolved;
    }

    private async Task<Dictionary<Guid, List<SalonBeautyBookingService>>> BuildBookingItemsMapAsync(List<Guid> bookingIds)
    {
        if (bookingIds == null || bookingIds.Count == 0)
        {
            return new Dictionary<Guid, List<SalonBeautyBookingService>>();
        }

        var query = await _bookingServiceRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(x => bookingIds.Contains(x.BookingId)));
        return list.GroupBy(x => x.BookingId).ToDictionary(g => g.Key, g => g.ToList());
    }

    private async Task<Dictionary<Guid, string>> BuildLocationMapAsync(List<Guid> locationIds)
    {
        if (locationIds == null || locationIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var query = await _locationRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(x => locationIds.Contains(x.Id)));
        return list.ToDictionary(x => x.Id, x => x.Name);
    }

    private async Task<string> BuildServiceSummaryAsync(List<SalonBeautyBookingService> items, Guid fallbackServiceId)
    {
        if (items == null || items.Count == 0)
        {
            var fallback = await _serviceRepository.FindAsync(fallbackServiceId);
            return fallback?.Name ?? "--";
        }

        var serviceIds = items.Select(x => x.ServiceId).Distinct().ToList();
        var serviceQuery = await _serviceRepository.GetQueryableAsync();
        var services = await AsyncExecuter.ToListAsync(serviceQuery.Where(x => serviceIds.Contains(x.Id)));
        var nameMap = services.ToDictionary(x => x.Id, x => x.Name);

        var names = items
            .Select(x => nameMap.GetValueOrDefault(x.ServiceId) ?? "--")
            .ToList();

        if (names.Count <= 2)
        {
            return string.Join(" + ", names);
        }

        return $"{names[0]} (+{names.Count - 1} dịch vụ)";
    }

    private static string GetStatusColor(SalonBeautyBookingStatus status) => status switch
    {
        SalonBeautyBookingStatus.New => "#F59E0B",
        SalonBeautyBookingStatus.Confirmed => "#3B82F6",
        SalonBeautyBookingStatus.Processing => "#8B5CF6",
        SalonBeautyBookingStatus.Completed => "#10B981",
        SalonBeautyBookingStatus.Cancelled => "#EF4444",
        _ => "#9CA3AF"
    };

    private static string GetBookingStatusText(SalonBeautyBookingStatus status) => status switch
    {
        SalonBeautyBookingStatus.New => "Chờ xác nhận",
        SalonBeautyBookingStatus.Confirmed => "Đã xác nhận",
        SalonBeautyBookingStatus.Processing => "Đang thực hiện",
        SalonBeautyBookingStatus.Completed => "Hoàn thành",
        SalonBeautyBookingStatus.Cancelled => "Đã hủy",
        _ => status.ToString()
    };

    private static string GetPaymentStatusText(SalonBeautyPaymentStatus status) => status switch
    {
        SalonBeautyPaymentStatus.Unpaid => "Chưa thanh toán",
        SalonBeautyPaymentStatus.Partial => "Thanh toán một phần",
        SalonBeautyPaymentStatus.Paid => "Đã thanh toán",
        SalonBeautyPaymentStatus.Refunded => "Đã hoàn tiền",
        _ => status.ToString()
    };

    private static string GetCheckinStatusText(SalonBeautyCheckinStatus status) => status switch
    {
        SalonBeautyCheckinStatus.NotCheckedIn => "Chưa check-in",
        SalonBeautyCheckinStatus.CheckedIn => "Đã check-in",
        SalonBeautyCheckinStatus.NoShow => "Không đến",
        _ => status.ToString()
    };

    private static string GetPaymentMethodText(SalonBeautyPaymentMethod? method) => method switch
    {
        SalonBeautyPaymentMethod.Cash => "Tiền mặt",
        SalonBeautyPaymentMethod.BankTransfer => "Chuyển khoản",
        SalonBeautyPaymentMethod.Card => "Thẻ",
        _ => "--"
    };

    private static string GetStylistRoleText(SalonBeautyStylistRole role) => role switch
    {
        SalonBeautyStylistRole.HairStylist => "Hair Stylist",
        SalonBeautyStylistRole.Shampoo => "Gội đầu",
        SalonBeautyStylistRole.NailLashes => "Nail / Mi",
        SalonBeautyStylistRole.SkincareSpa => "Skincare / Spa",
        SalonBeautyStylistRole.Other => "Khác",
        _ => role.ToString()
    };

    private async Task<SalonBeautyBookingListDto> MapToBookingListDto(SalonBeautyBooking booking, List<SalonBeautyBookingService> items)
    {
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var stylist = await _stylistRepository.FindAsync(booking.StylistId);
        var servicesSummary = await BuildServiceSummaryAsync(items, booking.ServiceId);

        return new SalonBeautyBookingListDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            CustomerId = booking.CustomerId,
            CustomerName = customer?.Name,
            CustomerPhone = customer?.Phone,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(customer?.Phone),
            CustomerAvatar = customer?.Avatar,
            StylistId = booking.StylistId,
            StylistName = stylist?.DisplayName,
            ServicesSummary = servicesSummary,
            ServiceCount = items.Count > 0 ? items.Count : 1,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            StatusText = GetBookingStatusText(booking.Status),
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = GetPaymentStatusText(booking.PaymentStatus),
            CheckinStatus = booking.CheckinStatus
        };
    }

    private async Task<SalonBeautyBookingDetailDto> MapToBookingListDetailDto(SalonBeautyBooking booking, List<SalonBeautyBookingService> items)
    {
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var stylist = await _stylistRepository.FindAsync(booking.StylistId);
        var location = booking.LocationId.HasValue ? await _locationRepository.FindAsync(booking.LocationId.Value) : null;
        var servicesSummary = await BuildServiceSummaryAsync(items, booking.ServiceId);

        return new SalonBeautyBookingDetailDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            LocationId = booking.LocationId,
            LocationName = location?.Name,
            CustomerId = booking.CustomerId,
            CustomerName = customer?.Name,
            CustomerPhone = customer?.Phone,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(customer?.Phone),
            CustomerAvatar = customer?.Avatar,
            StylistId = booking.StylistId,
            StylistName = stylist?.DisplayName,
            TimeSlotId = booking.TimeSlotId,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            StatusText = GetBookingStatusText(booking.Status),
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = GetPaymentStatusText(booking.PaymentStatus),
            PaymentMethod = booking.PaymentMethod,
            PaymentMethodText = GetPaymentMethodText(booking.PaymentMethod),
            CheckinStatus = booking.CheckinStatus,
            CheckinStatusText = GetCheckinStatusText(booking.CheckinStatus),
            ServicesSummary = servicesSummary,
            ServiceCount = items.Count > 0 ? items.Count : 1,
            CreationTime = booking.CreationTime,
            LastModificationTime = booking.LastModificationTime,
            Items = new List<SalonBeautyBookingItemDto>(),
            Activities = new List<SalonBeautyBookingActivityDto>()
        };
    }

    private async Task<SalonBeautyBookingDetailDto> MapToBookingDetailDto(SalonBeautyBooking booking)
    {
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var stylist = await _stylistRepository.FindAsync(booking.StylistId);
        var location = booking.LocationId.HasValue ? await _locationRepository.FindAsync(booking.LocationId.Value) : null;

        var itemsQuery = await _bookingServiceRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(itemsQuery.Where(x => x.BookingId == booking.Id));

        var serviceIds = items.Select(x => x.ServiceId).Distinct().ToList();
        var serviceQuery = await _serviceRepository.GetQueryableAsync();
        var services = await AsyncExecuter.ToListAsync(serviceQuery.Where(x => serviceIds.Contains(x.Id)));
        var serviceMap = services.ToDictionary(x => x.Id);

        var categoryIds = services.Select(x => x.CategoryId).Distinct().ToList();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();
        var categories = await AsyncExecuter.ToListAsync(categoryQuery.Where(x => categoryIds.Contains(x.Id)));
        var categoryMap = categories.ToDictionary(x => x.Id, x => x.Name);

        var itemDtos = items.Select(x =>
        {
            var svc = serviceMap.GetValueOrDefault(x.ServiceId);
            return new SalonBeautyBookingItemDto
            {
                Id = x.Id,
                ServiceId = x.ServiceId,
                ServiceName = svc?.Name,
                ServiceCategoryName = svc != null ? categoryMap.GetValueOrDefault(svc.CategoryId) : null,
                StylistId = booking.StylistId,
                StylistName = stylist?.DisplayName,
                Price = x.Price,
                Duration = x.Duration
            };
        }).ToList();

        if (itemDtos.Count == 0)
        {
            var svc = await _serviceRepository.FindAsync(booking.ServiceId);
            if (svc != null)
            {
                itemDtos.Add(new SalonBeautyBookingItemDto
                {
                    Id = Guid.Empty,
                    ServiceId = svc.Id,
                    ServiceName = svc.Name,
                    ServiceCategoryName = categoryMap.GetValueOrDefault(svc.CategoryId),
                    StylistId = booking.StylistId,
                    StylistName = stylist?.DisplayName,
                    Price = svc.Price,
                    Duration = svc.Duration
                });
            }
        }

        var (customerNote, internalNote) = UnpackNote(booking.Note);
        var subTotal = itemDtos.Sum(x => x.Price);

        var loyalty = await _loyaltyRepository.FirstOrDefaultAsync(x => x.CustomerId == booking.CustomerId);

        return new SalonBeautyBookingDetailDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            LocationId = booking.LocationId,
            LocationName = location?.Name,
            CustomerId = booking.CustomerId,
            CustomerName = customer?.Name,
            CustomerPhone = customer?.Phone,
            CustomerPhoneMasked = PhoneHelper.MaskPhone(customer?.Phone),
            CustomerAvatar = customer?.Avatar,
            CustomerCode = customer?.CustomerCode,
            CustomerLoyaltyPoint = loyalty?.CurrentPoint ?? 0,
            StylistId = booking.StylistId,
            StylistName = stylist?.DisplayName,
            StylistAvatar = stylist?.Avatar,
            StylistRoleText = stylist?.Role.HasValue == true ? GetStylistRoleText((SalonBeautyStylistRole)stylist.Role.Value) : null,
            TimeSlotId = booking.TimeSlotId,
            ServicesSummary = itemDtos.Count == 0 ? null : string.Join(", ", itemDtos.Select(x => x.ServiceName).Where(x => !string.IsNullOrWhiteSpace(x))),
            ServiceCount = itemDtos.Count,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            SubTotal = subTotal,
            Surcharge = 0m,
            Discount = Math.Max(0m, subTotal - booking.TotalAmount),
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            StatusText = GetBookingStatusText(booking.Status),
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = GetPaymentStatusText(booking.PaymentStatus),
            PaymentMethod = booking.PaymentMethod,
            PaymentMethodText = GetPaymentMethodText(booking.PaymentMethod),
            CheckinStatus = booking.CheckinStatus,
            CheckinStatusText = GetCheckinStatusText(booking.CheckinStatus),
            CheckinTime = booking.CheckinTime,
            PaidTime = booking.PaymentStatus == SalonBeautyPaymentStatus.Paid ? booking.LastModificationTime : null,
            CustomerNote = customerNote,
            InternalNote = internalNote,
            CancelReason = booking.CancelReason,
            CancelNote = booking.CancelNote,
            CreationTime = booking.CreationTime,
            LastModificationTime = booking.LastModificationTime,
            Items = itemDtos,
            Activities = BuildActivities(booking)
        };
    }

    private static List<SalonBeautyBookingActivityDto> BuildActivities(SalonBeautyBooking booking)
    {
        var list = new List<SalonBeautyBookingActivityDto>
        {
            new()
            {
                Title = "Lịch đặt được khởi tạo",
                Description = $"Booking {booking.BookingCode}",
                Time = booking.CreationTime
            }
        };

        if (booking.CheckinTime.HasValue)
        {
            list.Add(new SalonBeautyBookingActivityDto
            {
                Title = "Khách đã check-in",
                Description = booking.CheckinTime.Value.ToString("HH:mm dd/MM"),
                Time = booking.CheckinTime.Value
            });
        }

        if (booking.Status == SalonBeautyBookingStatus.Cancelled)
        {
            list.Add(new SalonBeautyBookingActivityDto
            {
                Title = "Lịch đặt đã hủy",
                Description = booking.CancelNote,
                Time = booking.LastModificationTime ?? booking.CreationTime,
                IsDanger = true
            });
        }

        if (booking.LastModificationTime.HasValue
            && booking.LastModificationTime.Value > booking.CreationTime
            && booking.Status != SalonBeautyBookingStatus.Cancelled)
        {
            list.Insert(0, new SalonBeautyBookingActivityDto
            {
                Title = $"Trạng thái: {GetBookingStatusText(booking.Status)}",
                Description = "Cập nhật gần nhất",
                Time = booking.LastModificationTime.Value
            });
        }

        return list.OrderByDescending(x => x.Time).ToList();
    }

    private static string GenerateBookingCode()
    {
        return $"BK-{DateTime.Now:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
    }

    private static string? PackNote(string? customerNote, string? internalNote)
    {
        var hasCustomer = !string.IsNullOrWhiteSpace(customerNote);
        var hasInternal = !string.IsNullOrWhiteSpace(internalNote);

        if (!hasCustomer && !hasInternal) return null;
        if (hasCustomer && !hasInternal) return customerNote!.Trim();
        if (!hasCustomer && hasInternal) return InternalNoteSeparator + internalNote!.Trim();
        return customerNote!.Trim() + InternalNoteSeparator + internalNote!.Trim();
    }

    private static (string? Customer, string? Internal) UnpackNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note)) return (null, null);

        var idx = note.IndexOf(InternalNoteSeparator, StringComparison.Ordinal);
        if (idx < 0) return (note, null);

        var customer = idx == 0 ? null : note.Substring(0, idx);
        var internalText = note.Substring(idx + InternalNoteSeparator.Length);
        return (customer, internalText);
    }


    private static void EnsureBookingCanBeEdited(SalonBeautyBooking booking)
    {
        if (booking.Status == SalonBeautyBookingStatus.Completed)
        {
            throw new UserFriendlyException("Booking đã hoàn thành không thể sửa.");
        }

        if (booking.Status == SalonBeautyBookingStatus.Cancelled)
        {
            throw new UserFriendlyException("Booking đã hủy không thể sửa.");
        }
    }

    private static bool IsValidNextStatus(SalonBeautyBookingStatus current, SalonBeautyBookingStatus next)
    {
        return current switch
        {
            SalonBeautyBookingStatus.New => next == SalonBeautyBookingStatus.Confirmed,
            SalonBeautyBookingStatus.Confirmed => next == SalonBeautyBookingStatus.Processing,
            SalonBeautyBookingStatus.Processing => next == SalonBeautyBookingStatus.Completed,
            _ => false
        };
    }

    private static string? GetOptionalStringProperty(object input, string propertyName)
    {
        var property = input.GetType().GetProperty(propertyName);
        if (property == null) return null;
        return property.GetValue(input)?.ToString();
    }

    private static string? AppendInternalNote(string? existingNote, string? newInternalNote)
    {
        if (newInternalNote.IsNullOrWhiteSpace()) return existingNote;

        var (customerNote, internalNote) = UnpackNote(existingNote);
        var line = $"[{DateTime.Now:dd/MM/yyyy HH:mm}] {newInternalNote!.Trim()}";
        var newInternal = internalNote.IsNullOrWhiteSpace()
            ? line
            : internalNote!.Trim() + Environment.NewLine + line;

        return PackNote(customerNote, newInternal);
    }

    private async Task CheckBookingPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty booking permission.");

        await AuthorizationService.CheckAsync(permission);
    }

    private async Task EnqueueBookingCreatedZbsAsync(SalonBeautyBooking booking)
    {
        try
        {
            var customer = await _customerRepository.FindAsync(booking.CustomerId);
            if (customer == null || string.IsNullOrWhiteSpace(customer.Phone))
                return;

            var address = "";
            if (booking.LocationId.HasValue && booking.LocationId.Value != Guid.Empty)
            {
                var location = await _locationRepository.FindAsync(booking.LocationId.Value);
                address = location?.Address ?? "";
            }

            var scheduleTime = $"{booking.BookingDate.ToString(ZaloDateFormat, CultureInfo.InvariantCulture)} {booking.StartTime:hh\\:mm}";

            await _jobManager.EnqueueAsync(
                new ZbsSendJobArgs
                {
                    TenantId = CurrentTenant.Id,
                    TemplateKey = "BookingCreated",
                    Phone = customer.Phone,
                    TrackingId = booking.Id.ToString(),
                    TemplateData = new
                    {
                        customer_name = customer.Name ?? "",
                        booking_code = booking.BookingCode,
                        schedule_time = scheduleTime,
                        address = address
                    }
                },
                priority: BackgroundJobPriority.Normal
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[ZBS][Salon] Enqueue BookingCreated failed. BookingId={BookingId}, BookingCode={BookingCode}, TenantId={TenantId}",
                booking.Id,
                booking.BookingCode,
                CurrentTenant.Id
            );
        }
    }

    private async Task EnqueueServiceReviewZbsAsync(SalonBeautyBooking booking)
    {
        try
        {
            var customer = await _customerRepository.FindAsync(booking.CustomerId);
            if (customer == null || string.IsNullOrWhiteSpace(customer.Phone))
                return;

            var scheduleTime = $"{booking.BookingDate.ToString(ZaloDateFormat, CultureInfo.InvariantCulture)} {booking.StartTime:hh\\:mm}";

            await _jobManager.EnqueueAsync(
                new ZbsSendJobArgs
                {
                    TenantId = CurrentTenant.Id,
                    TemplateKey = "ServiceReview",
                    Phone = customer.Phone,
                    TrackingId = booking.Id.ToString(),
                    TemplateData = new
                    {
                        customer_name = customer.Name ?? "",
                        schedule_time = scheduleTime
                    }
                },
                priority: BackgroundJobPriority.Normal
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[ZBS][Salon] Enqueue ServiceReview failed. BookingId={BookingId}, BookingCode={BookingCode}, TenantId={TenantId}",
                booking.Id,
                booking.BookingCode,
                CurrentTenant.Id
            );
        }
    }
}
