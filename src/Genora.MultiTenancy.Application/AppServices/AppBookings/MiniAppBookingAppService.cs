using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppDtos.AppBookings;
public class MiniAppBookingAppService : ApplicationService, IMiniAppBookingAppService
{
    private readonly IRepository<Booking, Guid> _bookingRepo;
    private readonly IRepository<BookingPlayer, Guid> _playerRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<GolfCourse, Guid> _golfcourseRepo;
    private readonly IRepository<CalendarSlot, Guid> _calendarSlotRepo;
    public MiniAppBookingAppService(
        IRepository<Booking, Guid> bookingRepo,
        IRepository<BookingPlayer, Guid> playerRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<GolfCourse, Guid> golfcourseRepo,
        IRepository<CalendarSlot, Guid> calendarSlotRepo)
    {
        _bookingRepo = bookingRepo;
        _playerRepo = playerRepo;
        _customerRepo = customerRepo;
        _golfcourseRepo = golfcourseRepo;
        _calendarSlotRepo = calendarSlotRepo;
    }

    public async Task<AppBookingDto> CreateFromMiniAppAsync(MiniAppCreateBookingDto input)
    {
        var customer = await _customerRepo.GetAsync(input.CustomerId);

        // BookingCode: CustomerCode + ddMMyy + serial/day (reset theo ngày)
        var datePart = input.PlayDate.ToString("ddMMyy");

        var countInDay = await _bookingRepo.CountAsync(x => x.PlayDate.Date == input.PlayDate.Date);
        var serial = (countInDay + 1).ToString("D3");

        var bookingCode = $"{customer.CustomerCode}-{datePart}-{serial}";

        var booking = new Booking(
             GuidGenerator.Create(),
             bookingCode,
             input.CustomerId,
             input.GolfCourseId,
             input.CalendarSlotId,
             input.PlayDate.Date,
             input.NumberOfGolfers,
             input.PricePerGolfer,
             input.TotalAmount,
             input.PaymentMethod,
             input.Status,
             input.Source
         );

        await _bookingRepo.InsertAsync(booking, autoSave: true);

        if (input.Players != null && input.Players.Any())
        {
            foreach (var p in input.Players)
            {
                var player = new BookingPlayer(
                    GuidGenerator.Create(),
                    booking.Id,
                    p.CustomerId,
                    p.PlayerName,
                    p.Notes
                );

                await _playerRepo.InsertAsync(player, autoSave: true);
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        // trả về dto đầy đủ (kèm players)
        var dto = ObjectMapper.Map<Booking, AppBookingDto>(booking);
        var players = await _playerRepo.GetListAsync(x => x.BookingId == booking.Id);
        dto.Players = ObjectMapper.Map<System.Collections.Generic.List<BookingPlayer>, System.Collections.Generic.List<AppBookingPlayerDto>>(players);

        return dto;
    }

    [DisableValidation]
    public async Task<MiniAppBookingListDto> GetListMiniAppAsync(GetMiniAppBookingListInput input)
    {
        try
        {
            if (input.CustomerId == Guid.Empty) throw new MemberAccessException("Vui lòng đăng nhập trước khi truy cập");
            // Mini app chỉ xem booking của chính customer
            var query = await _bookingRepo.WithDetailsAsync(x => x.CalendarSlot);
            query = query.Where(x => x.CustomerId == input.CustomerId);
            
            if (input.PlayDateFrom.HasValue)
            {
                query = query.Where(x => (x.PlayDate > input.PlayDateFrom.Value.Date) || (x.CalendarSlotId.HasValue && x.PlayDate.Date == input.PlayDateFrom.Value.Date && x.CalendarSlot.TimeFrom > input.PlayDateFrom.Value.TimeOfDay));
            }
                
            if (input.PlayDateTo.HasValue)
            {
                query = query.Where(x => (x.PlayDate < input.PlayDateTo.Value.Date) || ((x.CalendarSlotId.HasValue && x.PlayDate.Date == input.PlayDateTo.Value.Date && x.CalendarSlot.TimeFrom < input.PlayDateTo.Value.TimeOfDay)));
            }

            if (input.Status.HasValue)
                query = query.Where(x => x.Status == input.Status.Value);

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(Booking.CreationTime) + " desc"
                : input.Sorting;

            query = query.OrderBy(sorting);

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            var dto = ObjectMapper.Map<System.Collections.Generic.List<Booking>, System.Collections.Generic.List<BookingListData>>(items);
            var calendars = await _calendarSlotRepo.GetQueryableAsync(); 
            foreach (var item in dto)
            {
                item.VNDayOfWeek = FormatDateTimeHelper.GetVietnameseDayOfWeek(item.PlayDate);
                if (item.CalendarSlotId.HasValue && item.CalendarSlotId.Value != Guid.Empty)
                {
                    var calendar = calendars.FirstOrDefault(x  => x.Id == item.CalendarSlotId.Value);
                    if (calendar != null)
                    {
                        item.FrameTimes = $"{calendar.TimeFrom} - {calendar.TimeTo}";
                    }
                }
            }
            var result = new PagedResultDto<BookingListData>(total, dto);
            return new MiniAppBookingListDto { Data = result, Error = 0, Message = "Success" };
        }catch (Exception e)
        {
            return new MiniAppBookingListDto { Error = 400, Message = e.Message };
        }
    }

    public async Task<MiniAppBookingDetailDto> GetMiniAppAsync(Guid id, Guid customerId)
    {
        try
        {
            if (customerId == Guid.Empty) throw new MemberAccessException("Vui lòng đăng nhập trước khi truy cập");
            var booking = await _bookingRepo.FindAsync(x => x.Id == id);
            if (booking == null)
                throw new EntityNotFoundException(typeof(Booking), id);

            var dto = ObjectMapper.Map<Booking, BookingDetailData>(booking);

            dto.VNDayOfWeek = FormatDateTimeHelper.GetVietnameseDayOfWeek(dto.PlayDate);
            var players = await _playerRepo.GetListAsync(x => x.BookingId == id);
            dto.Players = ObjectMapper.Map<System.Collections.Generic.List<BookingPlayer>, System.Collections.Generic.List<AppBookingPlayerDto>>(players);
            dto.Utilities = string.IsNullOrEmpty(booking.Utility) ? new List<int>() : booking.Utility.Split(",").Select(int.Parse).ToList();
            dto.NumberHoles = booking.NumberHole;
            if (dto.CalendarSlotId.HasValue && dto.CalendarSlotId.Value != Guid.Empty)
            {
                var calendar = await _calendarSlotRepo.FirstOrDefaultAsync(x => x.Id == dto.CalendarSlotId.Value);
                if (calendar != null)
                {
                    dto.FrameTimes = $"{calendar.TimeFrom} - {calendar.TimeTo}";
                }
            }
            return new MiniAppBookingDetailDto { Data = dto, Error = 0, Message = "Success" };
        }
        catch (Exception e)
        {
            return new MiniAppBookingDetailDto { Error = (int)HttpStatusCode.BadRequest, Message = e.Message };
        }
    }
    //public async Task<MiniAppBookingListDto> GetBookingHistoryAsync(GetMiniAppBookingListInput input)
    //{
    //    try
    //    {
    //        if (input.CustomerId == Guid.Empty) throw new Exception("Vui lòng đăng nhập");
    //        var queries = await _bookingRepo.GetQueryableAsync();
    //        queries = queries.Where(b => b.CustomerId == input.CustomerId);

    //        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
    //           ? nameof(Customer.CreationTime) + " DESC"
    //           : input.Sorting;

    //        queries = queries.OrderBy(sorting);

    //        var totalCount = await AsyncExecuter.CountAsync(queries);

    //        var items = await AsyncExecuter.ToListAsync(
    //            queries.Skip(input.SkipCount).Take(input.MaxResultCount)
    //        );

    //        var result = new PagedResultDto<AppBookingDto>(
    //            totalCount,
    //            ObjectMapper.Map<List<Booking>, List<AppBookingDto>>(items)
    //        );
    //        return new MiniAppBookingListDto { Data = result, Error = 0, Message = "Success" };
    //    }catch (Exception e)
    //    {
    //        return new MiniAppBookingListDto {  Error = (int)HttpStatusCode.BadRequest, Message = e.Message };
    //    }
    //}
}