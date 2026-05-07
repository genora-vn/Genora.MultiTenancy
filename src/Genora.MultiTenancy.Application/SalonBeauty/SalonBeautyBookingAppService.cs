using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.SalonBeauty;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.Application.SalonBeauty;

[Authorize]
public class SalonBeautyBookingAppService : ApplicationService, ISalonBeautyBookingAppService
{
    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepository;
    private readonly IRepository<SalonBeautyCustomer, Guid> _customerRepository;
    private readonly IRepository<SalonBeautyService, Guid> _serviceRepository;
    private readonly IRepository<SalonBeautyStylist, Guid> _stylistRepository;

    public SalonBeautyBookingAppService(
        IRepository<SalonBeautyBooking, Guid> bookingRepository,
        IRepository<SalonBeautyCustomer, Guid> customerRepository,
        IRepository<SalonBeautyService, Guid> serviceRepository,
        IRepository<SalonBeautyStylist, Guid> stylistRepository)
    {
        _bookingRepository = bookingRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _stylistRepository = stylistRepository;
    }

    public async Task<PagedResultDto<SalonBeautyBookingListDto>> GetListAsync(GetSalonBeautyBookingListInput input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.Default);

        var query = await _bookingRepository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(),
            x => x.BookingCode != null && x.BookingCode.Contains(input.FilterText));
        query = query.WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId);
        query = query.WhereIf(input.StylistId.HasValue, x => x.StylistId == input.StylistId);
        query = query.WhereIf(input.Status.HasValue, x => (byte)x.Status == input.Status);
        query = query.WhereIf(input.FromDate.HasValue, x => x.BookingDate >= input.FromDate);
        query = query.WhereIf(input.ToDate.HasValue, x => x.BookingDate <= input.ToDate);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderByDescending(x => x.BookingDate)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        var dtos = new List<SalonBeautyBookingListDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToBookingListDto(item));
        }

        return new PagedResultDto<SalonBeautyBookingListDto>
        {
            TotalCount = totalCount,
            Items = dtos
        };
    }

    public async Task<SalonBeautyBookingDetailDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.Default);
        var booking = await _bookingRepository.GetAsync(id);
        return await MapToBookingDetailDto(booking);
    }

    public async Task<SalonBeautyBookingDetailDto> CreateAsync(CreateSalonBeautyBookingDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.Create);

        var booking = new SalonBeautyBooking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = input.CustomerId,
            ServiceId = input.ServiceId,
            StylistId = input.StylistId,
            BookingDate = input.BookingDate,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            TotalAmount = input.TotalAmount,
            Status = SalonBeautyBookingStatus.New,
            PaymentStatus = SalonBeautyPaymentStatus.Unpaid,
            CheckinStatus = SalonBeautyCheckinStatus.NotCheckedIn,
            Note = input.Note
        };

        var created = await _bookingRepository.InsertAsync(booking);
        return await MapToBookingDetailDto(created);
    }

    public async Task<SalonBeautyBookingDetailDto> UpdateAsync(Guid id, UpdateSalonBeautyBookingDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.Edit);

        var booking = await _bookingRepository.GetAsync(id);
        booking.CustomerId = input.CustomerId;
        booking.ServiceId = input.ServiceId;
        booking.StylistId = input.StylistId;
        booking.BookingDate = input.BookingDate;
        booking.StartTime = input.StartTime;
        booking.EndTime = input.EndTime;
        booking.TotalAmount = input.TotalAmount;
        booking.Status = input.Status;
        booking.Note = input.Note;

        var updated = await _bookingRepository.UpdateAsync(booking);
        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingDetailDto> CheckinAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.Checkin);

        var booking = await _bookingRepository.GetAsync(id);
        booking.CheckinStatus = SalonBeautyCheckinStatus.CheckedIn;
        booking.CheckinTime = DateTime.Now;

        if (booking.Status == SalonBeautyBookingStatus.New)
        {
            booking.Status = SalonBeautyBookingStatus.Confirmed;
        }

        var updated = await _bookingRepository.UpdateAsync(booking);
        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingDetailDto> UpdatePaymentAsync(Guid id, UpdateBookingPaymentDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.UpdatePayment);

        var booking = await _bookingRepository.GetAsync(id);
        booking.PaymentStatus = input.PaymentStatus;
        booking.PaymentMethod = input.PaymentMethod;

        if (input.PaymentStatus == SalonBeautyPaymentStatus.Paid &&
            booking.CheckinStatus == SalonBeautyCheckinStatus.CheckedIn)
        {
            booking.Status = SalonBeautyBookingStatus.Completed;
        }

        var updated = await _bookingRepository.UpdateAsync(booking);
        return await MapToBookingDetailDto(updated);
    }

    public async Task<SalonBeautyBookingDetailDto> CancelAsync(Guid id, CancelBookingDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.Cancel);

        var booking = await _bookingRepository.GetAsync(id);
        booking.Status = SalonBeautyBookingStatus.Cancelled;
        booking.CancelReason = input.CancelReason;
        booking.CancelNote = input.CancelNote;

        if (booking.PaymentStatus == SalonBeautyPaymentStatus.Paid)
        {
            booking.PaymentStatus = SalonBeautyPaymentStatus.Refunded;
        }

        var updated = await _bookingRepository.UpdateAsync(booking);
        return await MapToBookingDetailDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyBookings.Delete);
        await _bookingRepository.DeleteAsync(id);
    }

    private async Task<SalonBeautyBookingListDto> MapToBookingListDto(SalonBeautyBooking booking)
    {
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var service = await _serviceRepository.FindAsync(booking.ServiceId);
        var stylist = await _stylistRepository.FindAsync(booking.StylistId);

        return new SalonBeautyBookingListDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            CustomerId = booking.CustomerId,
            CustomerName = customer?.Name,
            ServiceId = booking.ServiceId,
            ServiceName = service?.Name,
            StylistId = booking.StylistId,
            StylistName = stylist?.DisplayName,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            PaymentStatus = booking.PaymentStatus,
            CheckinStatus = booking.CheckinStatus
        };
    }

    private async Task<SalonBeautyBookingDetailDto> MapToBookingDetailDto(SalonBeautyBooking booking)
    {
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var service = await _serviceRepository.FindAsync(booking.ServiceId);
        var stylist = await _stylistRepository.FindAsync(booking.StylistId);

        return new SalonBeautyBookingDetailDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            CustomerId = booking.CustomerId,
            CustomerName = customer?.Name,
            ServiceId = booking.ServiceId,
            ServiceName = service?.Name,
            StylistId = booking.StylistId,
            StylistName = stylist?.DisplayName,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            PaymentStatus = booking.PaymentStatus,
            PaymentMethod = booking.PaymentMethod,
            CheckinStatus = booking.CheckinStatus,
            CheckinTime = booking.CheckinTime,
            Note = booking.Note,
            CancelReason = booking.CancelReason,
            CancelNote = booking.CancelNote
        };
    }

    private string GenerateBookingCode()
    {
        return $"BK{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }

    private async Task CheckPolicyAsync(string permission)
        => await AuthorizationService.CheckAsync(permission);
}
