using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppBookings;

[Authorize]
public class AppBookingService :
        FeatureProtectedCrudAppService<
            Booking,
            AppBookingDto,
            Guid,
            GetBookingListInput,
            CreateUpdateAppBookingDto>,
        IAppBookingService
{
    protected override string FeatureName => AppBookingFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppBookings.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppBookings.Default;

    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
    private readonly IRepository<BookingPlayer, Guid> _playerRepository;

    public AppBookingService(
        IRepository<Booking, Guid> repository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        IRepository<BookingPlayer, Guid> playerRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        _customerRepository = customerRepository;
        _golfCourseRepository = golfCourseRepository;
        _playerRepository = playerRepository;

        GetPolicyName = MultiTenancyPermissions.AppBookings.Default;
        GetListPolicyName = MultiTenancyPermissions.AppBookings.Default;
        CreatePolicyName = MultiTenancyPermissions.AppBookings.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppBookings.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppBookings.Delete;
    }

    // LIST cho admin
    [DisableValidation]
    public override async Task<PagedResultDto<AppBookingDto>> GetListAsync(GetBookingListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(b =>
                b.BookingCode.Contains(filter)
            );
        }

        if (input.CustomerId.HasValue)
        {
            query = query.Where(b => b.CustomerId == input.CustomerId.Value);
        }

        if (input.GolfCourseId.HasValue)
        {
            query = query.Where(b => b.GolfCourseId == input.GolfCourseId.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(b => b.Status == input.Status.Value);
        }

        if (input.Source.HasValue)
        {
            query = query.Where(b => b.Source == input.Source.Value);
        }

        if (input.PlayDateFrom.HasValue)
        {
            query = query.Where(b => b.PlayDate >= input.PlayDateFrom.Value);
        }

        if (input.PlayDateTo.HasValue)
        {
            query = query.Where(b => b.PlayDate <= input.PlayDateTo.Value);
        }

        var sorting = input.Sorting.IsNullOrWhiteSpace()
            ? nameof(Booking.CreationTime) + " desc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        // Map thủ công + load thêm Customer/GolfCourse nếu cần
        var customerIds = items.Select(x => x.CustomerId).Distinct().ToList();
        var golfCourseIds = items.Select(x => x.GolfCourseId).Distinct().ToList();

        var customers = await _customerRepository.GetListAsync(c => customerIds.Contains(c.Id));
        var golfCourses = await _golfCourseRepository.GetListAsync(g => golfCourseIds.Contains(g.Id));

        var customerDict = customers.ToDictionary(c => c.Id, c => c);
        var golfDict = golfCourses.ToDictionary(g => g.Id, g => g);

        var dtoList = new List<AppBookingDto>();

        foreach (var b in items)
        {
            customerDict.TryGetValue(b.CustomerId, out var c);
            golfDict.TryGetValue(b.GolfCourseId, out var g);

            dtoList.Add(new AppBookingDto
            {
                Id = b.Id,
                TenantId = b.TenantId,
                BookingCode = b.BookingCode,
                CustomerId = b.CustomerId,
                CustomerName = c?.FullName,
                CustomerPhone = c?.PhoneNumber,
                GolfCourseId = b.GolfCourseId,
                GolfCourseName = g?.Name,
                CalendarSlotId = b.CalendarSlotId,
                PlayDate = b.PlayDate,
                NumberOfGolfers = b.NumberOfGolfers,
                PricePerGolfer = b.PricePerGolfer,
                TotalAmount = b.TotalAmount,
                PaymentMethod = b.PaymentMethod,
                Status = b.Status,
                Source = b.Source,
                CreationTime = b.CreationTime,
                CreatorId = b.CreatorId,
                LastModificationTime = b.LastModificationTime,
                LastModifierId = b.LastModifierId
            });
        }

        return new PagedResultDto<AppBookingDto>(totalCount, dtoList);
    }

    public override async Task<AppBookingDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var booking = await Repository.GetAsync(id);
        var customer = await _customerRepository.FindAsync(booking.CustomerId);
        var golfCourse = await _golfCourseRepository.FindAsync(booking.GolfCourseId);
        var players = await _playerRepository.GetListAsync(p => p.BookingId == id);

        var dto = new AppBookingDto
        {
            Id = booking.Id,
            TenantId = booking.TenantId,
            BookingCode = booking.BookingCode,
            CustomerId = booking.CustomerId,
            CustomerName = customer?.FullName,
            CustomerPhone = customer?.PhoneNumber,
            GolfCourseId = booking.GolfCourseId,
            GolfCourseName = golfCourse?.Name,
            CalendarSlotId = booking.CalendarSlotId,
            PlayDate = booking.PlayDate,
            NumberOfGolfers = booking.NumberOfGolfers,
            PricePerGolfer = booking.PricePerGolfer,
            TotalAmount = booking.TotalAmount,
            PaymentMethod = booking.PaymentMethod,
            Status = booking.Status,
            Source = booking.Source,
            CreationTime = booking.CreationTime,
            CreatorId = booking.CreatorId,
            LastModificationTime = booking.LastModificationTime,
            LastModifierId = booking.LastModifierId,
            Players = players.Select(p => new AppBookingPlayerDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                CustomerId = p.CustomerId,
                PlayerName = p.PlayerName,
                Notes = p.Notes
            }).ToList()
        };

        return dto;
    }

    // Admin Create (ít dùng, chủ yếu Mini App tạo)
    public override async Task<AppBookingDto> CreateAsync(CreateUpdateAppBookingDto input)
    {
        await CheckCreatePolicyAsync();

        var customer = await _customerRepository.GetAsync(input.CustomerId);
        var bookingCode = await GenerateBookingCodeAsync(customer.CustomerCode, input.PlayDate);

        var entity = new Booking(
            GuidGenerator.Create(),
            bookingCode,
            input.CustomerId,
            input.GolfCourseId,
            input.CalendarSlotId.Value,
            input.PlayDate,
            input.NumberOfGolfers,
            input.PricePerGolfer.Value,
            input.TotalAmount,
            input.PaymentMethod,
            input.Status,
            input.Source
        );

        entity = await Repository.InsertAsync(entity, autoSave: true);

        await SavePlayersAsync(entity.Id, input.Players);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(entity.Id);
    }

    public override async Task<AppBookingDto> UpdateAsync(Guid id, CreateUpdateAppBookingDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);

        entity.CustomerId = input.CustomerId;
        entity.GolfCourseId = input.GolfCourseId;
        entity.CalendarSlotId = input.CalendarSlotId;
        entity.PlayDate = input.PlayDate;
        entity.NumberOfGolfers = input.NumberOfGolfers;
        entity.PricePerGolfer = input.PricePerGolfer;
        entity.TotalAmount = input.TotalAmount;
        entity.PaymentMethod = input.PaymentMethod;
        entity.Status = input.Status;
        entity.Source = input.Source;

        entity = await Repository.UpdateAsync(entity, autoSave: true);

        await SavePlayersAsync(entity.Id, input.Players);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(entity.Id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        await _playerRepository.DeleteAsync(p => p.BookingId == id);
        await Repository.DeleteAsync(id);
    }

    // ==== Helper: sinh BookingCode ====
    private async Task<string> GenerateBookingCodeAsync(string customerCode, DateTime playDate)
    {
        var dayPart = playDate.ToString("ddMMyy"); // 121225

        var prefix = $"{customerCode}{dayPart}";

        var queryable = await Repository.GetQueryableAsync();

        var sameDayCodes = await AsyncExecuter.ToListAsync(
            queryable
                .Where(b => b.PlayDate.Date == playDate.Date && b.BookingCode.StartsWith(prefix))
                .Select(b => b.BookingCode)
        );

        var maxSeq = 0;
        foreach (var code in sameDayCodes)
        {
            var suffix = code.Substring(prefix.Length);
            if (int.TryParse(suffix, out var n) && n > maxSeq)
            {
                maxSeq = n;
            }
        }

        var nextSeq = maxSeq + 1;
        var seqPart = nextSeq.ToString("D3"); // 001

        return prefix + seqPart;
    }

    private async Task SavePlayersAsync(Guid bookingId, List<CreateUpdateBookingPlayerDto> players)
    {
        await _playerRepository.DeleteAsync(p => p.BookingId == bookingId);

        if (players == null || !players.Any())
        {
            return;
        }

        foreach (var p in players)
        {
            var player = new BookingPlayer(
                GuidGenerator.Create(),
                bookingId,
                p.CustomerId,
                p.PlayerName,
                p.Notes
            );

            await _playerRepository.InsertAsync(player, autoSave: true);
        }
    }
}