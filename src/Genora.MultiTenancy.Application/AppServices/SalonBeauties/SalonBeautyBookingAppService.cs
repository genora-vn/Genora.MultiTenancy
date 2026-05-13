using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauty;

[Authorize]
public class SalonBeautyBookingAppService : ApplicationService, ISalonBeautyBookingAppService
{
    private const string InternalNoteSeparator = "\n---INTERNAL---\n";

    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepository;
    private readonly IRepository<SalonBeautyBookingService, Guid> _bookingServiceRepository;
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyService, Guid> _serviceRepository;
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _categoryRepository;
    private readonly IRepository<SalonBeautyStylist, Guid> _stylistRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _loyaltyRepository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public SalonBeautyBookingAppService(
        IRepository<SalonBeautyBooking, Guid> bookingRepository,
        IRepository<SalonBeautyBookingService, Guid> bookingServiceRepository,
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyService, Guid> serviceRepository,
        IRepository<SalonBeautyServiceCategory, Guid> categoryRepository,
        IRepository<SalonBeautyStylist, Guid> stylistRepository,
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> loyaltyRepository,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _bookingRepository = bookingRepository;
        _bookingServiceRepository = bookingServiceRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _stylistRepository = stylistRepository;
        _loyaltyRepository = loyaltyRepository;
        _l = l;
    }

    public async Task<PagedResultDto<SalonBeautyBookingListDto>> GetListAsync(GetSalonBeautyBookingListInput input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _bookingRepository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(),
            x => (x.BookingCode != null && x.BookingCode.Contains(input.FilterText!)));
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

        var dtos = new List<SalonBeautyBookingListDto>();
        var bookingIds = items.Select(x => x.Id).ToList();
        var serviceMap = await BuildBookingItemsMapAsync(bookingIds);

        foreach (var item in items)
        {
            dtos.Add(await MapToBookingListDto(item, serviceMap.GetValueOrDefault(item.Id) ?? new List<SalonBeautyBookingService>()));
        }

        return new PagedResultDto<SalonBeautyBookingListDto>
        {
            TotalCount = totalCount,
            Items = dtos
        };
    }

    public async Task<SalonBeautyBookingDetailDto> GetAsync(Guid id)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);
        var booking = await _bookingRepository.GetAsync(id);
        return await MapToBookingDetailDto(booking);
    }

    public async Task<SalonBeautyBookingDetailDto> CreateAsync(CreateSalonBeautyBookingDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Create,
            MultiTenancyPermissions.HostSalonBeautyBookings.Create);
        ValidateBookingItems(input.Items);

        var resolved = await ResolveItemsAsync(input.Items);
        var totalDuration = resolved.Sum(x => x.Duration);
        var endTime = input.EndTime ?? input.StartTime.Add(TimeSpan.FromMinutes(totalDuration));
        var subTotal = resolved.Sum(x => x.Price);
        var totalAmount = subTotal + (input.Surcharge ?? 0m) - (input.Discount ?? 0m);
        if (totalAmount < 0) totalAmount = 0;

        var booking = new SalonBeautyBooking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = input.CustomerId,
            ServiceId = resolved.First().ServiceId,
            StylistId = input.StylistId,
            BookingDate = input.BookingDate.Date,
            StartTime = input.StartTime,
            EndTime = endTime,
            TotalAmount = totalAmount,
            Status = SalonBeautyBookingStatus.New,
            PaymentStatus = SalonBeautyPaymentStatus.Unpaid,
            CheckinStatus = SalonBeautyCheckinStatus.NotCheckedIn,
            Note = PackNote(input.CustomerNote, input.InternalNote)
        };

        var created = await _bookingRepository.InsertAsync(booking, autoSave: true);

        foreach (var item in resolved)
        {
            await _bookingServiceRepository.InsertAsync(new SalonBeautyBookingService
            {
                BookingId = created.Id,
                ServiceId = item.ServiceId,
                Price = item.Price,
                Duration = item.Duration
            }, autoSave: true);
        }

        return await MapToBookingDetailDto(created);
    }

    public async Task<SalonBeautyBookingDetailDto> UpdateAsync(Guid id, UpdateSalonBeautyBookingDto input)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Edit,
            MultiTenancyPermissions.HostSalonBeautyBookings.Edit);
        ValidateBookingItems(input.Items);

        var booking = await _bookingRepository.GetAsync(id);

        var resolved = await ResolveItemsAsync(input.Items);
        var totalDuration = resolved.Sum(x => x.Duration);
        var endTime = input.EndTime ?? input.StartTime.Add(TimeSpan.FromMinutes(totalDuration));
        var subTotal = resolved.Sum(x => x.Price);
        var totalAmount = subTotal + (input.Surcharge ?? 0m) - (input.Discount ?? 0m);
        if (totalAmount < 0) totalAmount = 0;

        booking.CustomerId = input.CustomerId;
        booking.StylistId = input.StylistId;
        booking.ServiceId = resolved.First().ServiceId;
        booking.BookingDate = input.BookingDate.Date;
        booking.StartTime = input.StartTime;
        booking.EndTime = endTime;
        booking.TotalAmount = totalAmount;
        booking.Status = input.Status;
        booking.Note = PackNote(input.CustomerNote, input.InternalNote);

        await _bookingRepository.UpdateAsync(booking, autoSave: true);

        var existing = await AsyncExecuter.ToListAsync(
            (await _bookingServiceRepository.GetQueryableAsync()).Where(x => x.BookingId == id));

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
                Duration = item.Duration
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
        booking.Status = input.Status;

        if (input.Status == SalonBeautyBookingStatus.Completed
            && booking.CheckinStatus != SalonBeautyCheckinStatus.CheckedIn)
        {
            booking.CheckinStatus = SalonBeautyCheckinStatus.CheckedIn;
            booking.CheckinTime ??= DateTime.Now;
        }

        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);
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
        booking.Status = SalonBeautyBookingStatus.Cancelled;
        booking.CancelReason = input.CancelReason;
        booking.CancelNote = input.CancelNote;

        if (booking.PaymentStatus == SalonBeautyPaymentStatus.Paid)
        {
            booking.PaymentStatus = SalonBeautyPaymentStatus.Refunded;
        }

        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);
        return await MapToBookingDetailDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Delete,
            MultiTenancyPermissions.HostSalonBeautyBookings.Delete);

        var existing = await AsyncExecuter.ToListAsync(
            (await _bookingServiceRepository.GetQueryableAsync()).Where(x => x.BookingId == id));

        foreach (var item in existing)
        {
            await _bookingServiceRepository.DeleteAsync(item.Id, autoSave: true);
        }

        await _bookingRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<List<SalonBeautyBookingCalendarDto>> GetCalendarEventsAsync(DateTime from, DateTime to, Guid? stylistId = null, Guid? serviceId = null)
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

        var bookings = await AsyncExecuter.ToListAsync(query);
        var ids = bookings.Select(x => x.Id).ToList();
        var itemsMap = await BuildBookingItemsMapAsync(ids);

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
            ProcessingCount = confirmedCount,
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

    public async Task<List<SalonBeautyStylistLookupDto>> GetStylistLookupAsync()
    {
        await CheckBookingPolicyAsync(
            MultiTenancyPermissions.SalonBeautyBookings.Default,
            MultiTenancyPermissions.HostSalonBeautyBookings.Default);

        var query = await _stylistRepository.GetQueryableAsync();
        var stylists = await AsyncExecuter.ToListAsync(query
            .Where(x => x.Status == 1)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.DisplayName));

        return stylists.Select(x => new SalonBeautyStylistLookupDto
        {
            Id = x.Id,
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
        SalonBeautyBookingStatus.Completed => "#10B981",
        SalonBeautyBookingStatus.Cancelled => "#EF4444",
        _ => "#9CA3AF"
    };

    private static string GetBookingStatusText(SalonBeautyBookingStatus status) => status switch
    {
        SalonBeautyBookingStatus.New => "Chờ xác nhận",
        SalonBeautyBookingStatus.Confirmed => "Đã xác nhận",
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
        SalonBeautyStylistRole.Junior => "Junior",
        SalonBeautyStylistRole.Senior => "Senior",
        SalonBeautyStylistRole.Manager => "Manager",
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

    private async Task<SalonBeautyBookingDetailDto> MapToBookingDetailDto(SalonBeautyBooking booking)
    {
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var stylist = await _stylistRepository.FindAsync(booking.StylistId);

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

    private async Task CheckBookingPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty booking permission.");

        await AuthorizationService.CheckAsync(permission);
    }
}
