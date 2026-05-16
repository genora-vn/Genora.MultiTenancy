using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyBookingAppService : ApplicationService, IMiniAppSalonBeautyBookingAppService
{
    private const string InternalNoteSeparator = "\n---INTERNAL---\n";

    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepository;
    private readonly IRepository<SalonBeautyBookingService, Guid> _bookingServiceRepository;
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyService, Guid> _serviceRepository;
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _categoryRepository;
    private readonly IRepository<SalonBeautyStylist, Guid> _stylistRepository;
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _loyaltyRepository;

    public MiniAppSalonBeautyBookingAppService(
        IRepository<SalonBeautyBooking, Guid> bookingRepository,
        IRepository<SalonBeautyBookingService, Guid> bookingServiceRepository,
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyService, Guid> serviceRepository,
        IRepository<SalonBeautyServiceCategory, Guid> categoryRepository,
        IRepository<SalonBeautyStylist, Guid> stylistRepository,
        IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> loyaltyRepository)
    {
        _bookingRepository = bookingRepository;
        _bookingServiceRepository = bookingServiceRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _stylistRepository = stylistRepository;
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<PagedResultDto<SalonBeautyBookingDetailDto>> GetListMiniAppAsync(GetSalonBeautyBookingListInput input)
    {
        input.MaxResultCount = input.MaxResultCount <= 0 ? 20 : Math.Min(input.MaxResultCount, 100);

        var query = await _bookingRepository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(), x => x.BookingCode.Contains(input.FilterText!));
        query = query.WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId!.Value);
        query = query.WhereIf(input.StylistId.HasValue, x => x.StylistId == input.StylistId!.Value);
        query = query.WhereIf(input.Status.HasValue, x => (byte)x.Status == input.Status!.Value);
        query = query.WhereIf(input.PaymentStatus.HasValue, x => (byte)x.PaymentStatus == input.PaymentStatus!.Value);
        query = query.WhereIf(input.FromDate.HasValue, x => x.BookingDate >= input.FromDate!.Value.Date);
        query = query.WhereIf(input.ToDate.HasValue, x => x.BookingDate <= input.ToDate!.Value.Date);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.BookingDate).ThenByDescending(x => x.StartTime)
                .Skip(input.SkipCount).Take(input.MaxResultCount));

        var result = new List<SalonBeautyBookingDetailDto>();
        foreach (var item in items)
        {
            result.Add(await MapToDtoAsync(item, lightweight: true));
        }

        return new PagedResultDto<SalonBeautyBookingDetailDto>(total, result);
    }

    public async Task<SalonBeautyBookingDetailDto> GetMiniAppAsync(Guid id)
    {
        return await MapToDtoAsync(await _bookingRepository.GetAsync(id));
    }

    public async Task<SalonBeautyBookingDetailDto> CreateMiniAppAsync(CreateSalonBeautyBookingDto input)
    {
        if (input.CustomerId == Guid.Empty) throw new UserFriendlyException("CustomerId is required.");
        if (input.StylistId == Guid.Empty) throw new UserFriendlyException("StylistId is required.");
        if (input.Items == null || input.Items.Count == 0) throw new UserFriendlyException("At least one service is required.");

        var customer = await _customerRepository.GetAsync(input.CustomerId);
        if (customer.Status != 1) throw new UserFriendlyException("Customer is inactive.");

        var stylist = await _stylistRepository.GetAsync(input.StylistId);
        if (stylist.Status != 1) throw new UserFriendlyException("Stylist is inactive.");

        var serviceIds = input.Items
            .Select(x => x.ServiceId)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var services = await _serviceRepository.GetListAsync(x => serviceIds.Contains(x.Id) && x.Status == 1);
        if (services.Count != serviceIds.Count) throw new UserFriendlyException("Invalid service list.");

        var serviceMap = services.ToDictionary(x => x.Id);
        var resolvedItems = input.Items
            .Where(x => x.ServiceId != Guid.Empty)
            .Select(x =>
            {
                var service = serviceMap[x.ServiceId];

                return new
                {
                    ServiceId = service.Id,
                    Price = x.Price > 0 ? x.Price : service.Price,
                    Duration = x.Duration > 0 ? x.Duration : service.Duration
                };
            })
            .ToList();

        var firstService = services.First();
        var totalDuration = resolvedItems.Sum(x => x.Duration);
        var endTime = input.EndTime ?? input.StartTime.Add(TimeSpan.FromMinutes(totalDuration));
        var subTotal = resolvedItems.Sum(x => x.Price);
        var totalAmount = subTotal + (input.Surcharge ?? 0m) - (input.Discount ?? 0m);
        if (totalAmount < 0) totalAmount = 0;

        var booking = new SalonBeautyBooking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = input.CustomerId,
            StylistId = input.StylistId,
            ServiceId = firstService.Id,
            BookingDate = input.BookingDate.Date,
            StartTime = input.StartTime,
            EndTime = endTime,
            TotalAmount = totalAmount,
            Status = SalonBeautyBookingStatus.New,
            PaymentStatus = SalonBeautyPaymentStatus.Unpaid,
            CheckinStatus = SalonBeautyCheckinStatus.NotCheckedIn,
            Note = PackNote(input.CustomerNote, input.InternalNote),

            // API Mini App là AllowAnonymous, nên ưu tiên tenant hiện tại nếu đã resolve được từ domain;
            // nếu không có thì dùng TenantId của customer để tránh tạo booking lệch tenant.
            TenantId = CurrentTenant.Id ?? customer.TenantId
        };

        var bookingServices = new List<SalonBeautyBookingService>();

        try
        {
            // Theo cách đã fix được ở FNB: lưu cha autoSave=true trước.
            var created = await _bookingRepository.InsertAsync(booking, autoSave: true);

            foreach (var item in resolvedItems)
            {
                bookingServices.Add(new SalonBeautyBookingService
                {
                    BookingId = created.Id,
                    ServiceId = item.ServiceId,
                    Price = item.Price,
                    Duration = item.Duration
                });
            }

            // Không gọi CurrentUnitOfWork.SaveChangesAsync thủ công ở giữa.
            // Lưu detail bằng InsertManyAsync autoSave=true để EF xử lý flush đồng bộ.
            await _bookingServiceRepository.InsertManyAsync(bookingServices, autoSave: true);

            return await MapToDtoAsync(created);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "SALON_BOOKING_SAVE_FAILED | CurrentTenantId={CurrentTenantId} | CustomerTenantId={CustomerTenantId} | BookingId={BookingId}",
                CurrentTenant.Id,
                customer.TenantId,
                booking.Id
            );

            throw;
        }
    }

    public async Task<SalonBeautyBookingDetailDto> CancelMiniAppAsync(Guid id, CancelBookingDto input)
    {
        var booking = await _bookingRepository.GetAsync(id);
        if (booking.Status == SalonBeautyBookingStatus.Completed)
            throw new UserFriendlyException("Completed booking cannot be cancelled.");

        booking.Status = SalonBeautyBookingStatus.Cancelled;
        booking.CancelReason = input.CancelReason;
        booking.CancelNote = input.CancelNote;
        var updated = await _bookingRepository.UpdateAsync(booking, autoSave: true);
        return await MapToDtoAsync(updated);
    }

    private async Task<SalonBeautyBookingDetailDto> MapToDtoAsync(SalonBeautyBooking booking, bool lightweight = false)
    {
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var stylist = await _stylistRepository.FindAsync(booking.StylistId);
        var items = await _bookingServiceRepository.GetListAsync(x => x.BookingId == booking.Id);

        var serviceIds = items.Select(x => x.ServiceId).Distinct().ToList();
        if (serviceIds.Count == 0 && booking.ServiceId != Guid.Empty) serviceIds.Add(booking.ServiceId);

        var services = serviceIds.Count == 0 ? new List<SalonBeautyService>() : await _serviceRepository.GetListAsync(x => serviceIds.Contains(x.Id));
        var serviceMap = services.ToDictionary(x => x.Id);

        var categories = services.Count == 0
            ? new List<SalonBeautyServiceCategory>()
            : await _categoryRepository.GetListAsync(x => services.Select(s => s.CategoryId).Contains(x.Id));
        var categoryMap = categories.ToDictionary(x => x.Id, x => x.Name);

        var itemDtos = items.Select(x =>
        {
            serviceMap.TryGetValue(x.ServiceId, out var svc);
            return new SalonBeautyBookingItemDto
            {
                Id = x.Id,
                ServiceId = x.ServiceId,
                ServiceName = svc?.Name,
                ServiceCategoryName = svc != null && categoryMap.TryGetValue(svc.CategoryId, out var cat) ? cat : null,
                StylistId = booking.StylistId,
                StylistName = stylist?.DisplayName,
                Price = x.Price,
                Duration = x.Duration
            };
        }).ToList();

        if (itemDtos.Count == 0 && services.Count > 0)
        {
            var svc = services.First();
            itemDtos.Add(new SalonBeautyBookingItemDto
            {
                ServiceId = svc.Id,
                ServiceName = svc.Name,
                ServiceCategoryName = categoryMap.TryGetValue(svc.CategoryId, out var cat) ? cat : null,
                StylistId = booking.StylistId,
                StylistName = stylist?.DisplayName,
                Price = svc.Price,
                Duration = svc.Duration
            });
        }

        var loyalty = await _loyaltyRepository.FirstOrDefaultAsync(x => x.CustomerId == booking.CustomerId);
        var (customerNote, internalNote) = UnpackNote(booking.Note);
        var servicesSummary = itemDtos.Count == 0 ? null : string.Join(", ", itemDtos.Select(x => x.ServiceName).Where(x => !string.IsNullOrWhiteSpace(x)));

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
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            SubTotal = itemDtos.Sum(x => x.Price),
            TotalAmount = booking.TotalAmount,
            Discount = Math.Max(0m, itemDtos.Sum(x => x.Price) - booking.TotalAmount),
            Status = booking.Status,
            StatusText = booking.Status.ToString(),
            PaymentStatus = booking.PaymentStatus,
            PaymentStatusText = booking.PaymentStatus.ToString(),
            PaymentMethod = booking.PaymentMethod,
            PaymentMethodText = booking.PaymentMethod?.ToString(),
            CheckinStatus = booking.CheckinStatus,
            CheckinStatusText = booking.CheckinStatus.ToString(),
            CheckinTime = booking.CheckinTime,
            PaidTime = booking.PaymentStatus == SalonBeautyPaymentStatus.Paid ? booking.LastModificationTime : null,
            CustomerNote = customerNote,
            InternalNote = internalNote,
            CancelReason = booking.CancelReason,
            CancelNote = booking.CancelNote,
            ServicesSummary = servicesSummary,
            ServiceCount = itemDtos.Count,
            CreationTime = booking.CreationTime,
            LastModificationTime = booking.LastModificationTime,
            Items = lightweight ? new List<SalonBeautyBookingItemDto>() : itemDtos,
            Activities = new List<SalonBeautyBookingActivityDto>()
        };
    }

    private static string GenerateBookingCode() => $"SB{DateTime.Now:yyMMdd}{Random.Shared.Next(1000, 9999)}";

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
        var customer = idx == 0 ? null : note[..idx];
        var internalText = note[(idx + InternalNoteSeparator.Length)..];
        return (customer, internalText);
    }
}
