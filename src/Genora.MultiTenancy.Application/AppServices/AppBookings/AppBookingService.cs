using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
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
    private readonly AppBookingExcelExporter _excelExporter;
    private readonly AppBookingExcelImporter _excelImporter;
    private readonly AppBookingExcelTemplateGenerator _templateGenerator;

    public AppBookingService(
        IRepository<Booking, Guid> repository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        IRepository<BookingPlayer, Guid> playerRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        AppBookingExcelExporter excelExporter,
        AppBookingExcelImporter excelImporter,
        AppBookingExcelTemplateGenerator templateGenerator)
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
        _excelExporter = excelExporter;
        _excelImporter = excelImporter;
        _templateGenerator = templateGenerator;
    }

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
            Utilities = booking.Utility,
            NumberHoles = booking.NumberHole,
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

    // Tải file mẫu
    public Task<IRemoteStreamContent> DownloadTemplateAsync()
    {
        var rows = new List<AppBookingExcelRowDto>();
        return Task.FromResult(_excelExporter.Export(rows));
    }

    public Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        return Task.FromResult(_templateGenerator.GenerateTemplate());
    }

    public async Task ImportExcelAsync(ImportBookingExcelInput input)
    {
        await CheckUpdatePolicyAsync();

        using var stream = input.File.GetStream();
        var rows = _excelImporter.Read(stream);

        foreach (var (rowNumber, r) in rows)
        {
            // ===== VALIDATE =====
            // ===== Check các trường dữ liệu theo đúng entity hoặc cần thì valid các trường bắt buộc, đây là a làm demo cho bảng Booking các bảng khác tương tự =====
            if (r.PlayDate == null)
            {
                throw new UserFriendlyException(
                    "Import Excel lỗi",
                    $"Dòng {rowNumber}: PlayDate là bắt buộc"
                );
            }

            if (!Enum.TryParse<BookingStatus>(r.Status, out var status))
                throw new UserFriendlyException(
                    "Import Excel lỗi",
                    $"Dòng {rowNumber}: Status không hợp lệ");

            if (!Enum.TryParse<PaymentMethod>(r.PaymentMethod, out var payment))
                throw new UserFriendlyException(
                    "Import Excel lỗi",
                    $"Dòng {rowNumber}: PaymentMethod không hợp lệ");

            if (!Enum.TryParse<BookingSource>(r.Source, out var source))
                throw new UserFriendlyException(
                    "Import Excel lỗi",
                    $"Dòng {rowNumber}: Source không hợp lệ");

            var booking = await Repository.FirstOrDefaultAsync(
                x => x.BookingCode == r.BookingCode
            );

            if (booking == null)
            {
                var datePart = r.PlayDate.ToString("ddMMyy");

                var countInDay = await Repository.CountAsync(x => x.PlayDate.Date == r.PlayDate.Date);
                var serial = (countInDay + 1).ToString("D3");

                var bookingCode = $"KH000001-{datePart}-{serial}";
                // ===== INSERT =====
                booking = new Booking(
                    GuidGenerator.Create(),
                    bookingCode,
                    new Guid("8DA661EA-D7A2-1692-5B49-3A1E329C52B3"),              // Mapping Customer
                    new Guid("C38585C6-8996-11C8-B721-3A1E329BAF6E"),              // Mapping GolfCourse
                    Guid.Empty,                                                    // Mapping Calendar
                    r.PlayDate,
                    r.NumberOfGolfers,
                    0,
                    r.TotalAmount,
                    payment,
                    status,
                    source
                );

                await Repository.InsertAsync(booking, autoSave: true);
            }
            else
            {
                // ===== UPDATE =====
                booking.NumberOfGolfers = r.NumberOfGolfers;
                booking.TotalAmount = r.TotalAmount;
                booking.Status = status;
                booking.PaymentMethod = payment;
                booking.Source = source;

                await Repository.UpdateAsync(booking, autoSave: true);
            }
        }
    }

    [DisableValidation]
    public async Task<IRemoteStreamContent> ExportExcelAsync(GetBookingListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = await Repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
            query = query.Where(x => x.BookingCode.Contains(input.FilterText));

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (input.Source.HasValue)
            query = query.Where(x => x.Source == input.Source.Value);

        var list = await AsyncExecuter.ToListAsync(query);

        var customerIds = list.Select(x => x.CustomerId).Distinct();
        var golfIds = list.Select(x => x.GolfCourseId).Distinct();

        var customers = await _customerRepository.GetListAsync(x => customerIds.Contains(x.Id));
        var golfs = await _golfCourseRepository.GetListAsync(x => golfIds.Contains(x.Id));

        var rows = list.Select(b =>
        {
            var c = customers.FirstOrDefault(x => x.Id == b.CustomerId);
            var g = golfs.FirstOrDefault(x => x.Id == b.GolfCourseId);

            return new AppBookingExcelRowDto
            {
                BookingCode = b.BookingCode,
                CustomerName = c?.FullName,
                CustomerPhone = c?.PhoneNumber,
                GolfCourseName = g?.Name,
                PlayDate = b.PlayDate,
                NumberOfGolfers = b.NumberOfGolfers,
                TotalAmount = b.TotalAmount,
                PaymentMethod = b.PaymentMethod?.ToString(),
                Status = b.Status.ToString(),
                Source = b.Source.ToString()
            };
        }).ToList();

        return _excelExporter.Export(rows);
    }
}