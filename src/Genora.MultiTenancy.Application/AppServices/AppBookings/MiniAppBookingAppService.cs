using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
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

    public MiniAppBookingAppService(
        IRepository<Booking, Guid> bookingRepo,
        IRepository<BookingPlayer, Guid> playerRepo,
        IRepository<Customer, Guid> customerRepo)
    {
        _bookingRepo = bookingRepo;
        _playerRepo = playerRepo;
        _customerRepo = customerRepo;
    }

    public async Task<AppBookingDto> CreateFromMiniAppAsync(MiniAppCreateBookingDto input)
    {
        // TODO (optional): validate calendar slot tồn tại / price / max slots...

        var customer = await _customerRepo.GetAsync(input.CustomerId);

        // BookingCode: CustomerCode + ddMMyy + serial/day (reset theo ngày)
        var datePart = input.PlayDate.ToString("ddMMyy");

        // serial theo ngày (toàn hệ thống). Nếu bạn muốn theo sân/tenant -> đổi predicate
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
    public async Task<PagedResultDto<AppBookingDto>> GetListMiniAppAsync(GetMiniAppBookingListInput input)
    {
        // Mini app chỉ xem booking của chính customer
        var query = await _bookingRepo.GetQueryableAsync();
        query = query.Where(x => x.CustomerId == input.CustomerId);

        if (input.PlayDateFrom.HasValue)
            query = query.Where(x => x.PlayDate >= input.PlayDateFrom.Value.Date);

        if (input.PlayDateTo.HasValue)
            query = query.Where(x => x.PlayDate <= input.PlayDateTo.Value.Date);

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(Booking.CreationTime) + " desc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dto = ObjectMapper.Map<System.Collections.Generic.List<Booking>, System.Collections.Generic.List<AppBookingDto>>(items);
        return new PagedResultDto<AppBookingDto>(total, dto);
    }

    public async Task<AppBookingDto> GetMiniAppAsync(Guid id, Guid customerId)
    {
        var booking = await _bookingRepo.FindAsync(x => x.Id == id && x.CustomerId == customerId);
        if (booking == null)
            throw new EntityNotFoundException(typeof(Booking), id);

        var dto = ObjectMapper.Map<Booking, AppBookingDto>(booking);

        // include players
        var players = await _playerRepo.GetListAsync(x => x.BookingId == id);
        dto.Players = ObjectMapper.Map<System.Collections.Generic.List<BookingPlayer>, System.Collections.Generic.List<AppBookingPlayerDto>>(players);

        return dto;
    }
}