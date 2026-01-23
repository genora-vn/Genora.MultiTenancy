using DocumentFormat.OpenXml.EMMA;
using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
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

    private readonly IStringLocalizer<MultiTenancyResource> _l;

    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
    private readonly IRepository<BookingPlayer, Guid> _playerRepository;
    private readonly IRepository<CalendarSlot, Guid> _calendarSlotRepository;
    private readonly IAppEmailSenderService _appEmailSenderService;
    private readonly AppBookingExcelExporter _excelExporter;
    private readonly AppBookingExcelImporter _excelImporter;
    private readonly AppBookingExcelTemplateGenerator _templateGenerator;
    private readonly IRepository<OptionExtend, Guid> _optionExtendRepo;
    private readonly IRepository<CustomerType, Guid> _customerType;
    private readonly ISettingProvider _settingProvider;
    public AppBookingService(
        IRepository<Booking, Guid> repository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        IRepository<BookingPlayer, Guid> playerRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        AppBookingExcelExporter excelExporter,
        AppBookingExcelImporter excelImporter,
        AppBookingExcelTemplateGenerator templateGenerator,
        IRepository<CalendarSlot, Guid> calendarSlotRepository,
        IStringLocalizer<MultiTenancyResource> l,
        IAppEmailSenderService appEmailSenderService,
        IRepository<OptionExtend, Guid> optionExtendRepo,
        IRepository<CustomerType, Guid> customerType,
        ISettingProvider settingProvider)
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
        _calendarSlotRepository = calendarSlotRepository;
        _l = l;
        _appEmailSenderService = appEmailSenderService;
        _optionExtendRepo = optionExtendRepo;
        _customerType = customerType;
        _settingProvider = settingProvider;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<AppBookingDto>> GetListAsync(GetBookingListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        var customerQueryable = await _customerRepository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(b =>
                 b.BookingCode.Contains(filter)
                 || customerQueryable.Any(c =>
                     c.Id == b.CustomerId
                     && (
                         c.FullName.Contains(filter)
                         || (c.CustomerCode != null && c.CustomerCode.Contains(filter))
                         || (c.PhoneNumber != null && c.PhoneNumber.Contains(filter))
                     )
                 )
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

        var calendarSlotIds = items
            .Where(x => x.CalendarSlotId.HasValue && x.CalendarSlotId.Value != Guid.Empty)
            .Select(x => x.CalendarSlotId!.Value)
            .Distinct()
            .ToList();

        var slots = calendarSlotIds.Count == 0
            ? new List<CalendarSlot>()
            : await _calendarSlotRepository.GetListAsync(s => calendarSlotIds.Contains(s.Id));

        var slotDict = slots.ToDictionary(s => s.Id, s => s);

        var dtoList = new List<AppBookingDto>();

        foreach (var b in items)
        {
            customerDict.TryGetValue(b.CustomerId, out var c);
            golfDict.TryGetValue(b.GolfCourseId, out var g);
            slotDict.TryGetValue(b.CalendarSlotId ?? Guid.Empty, out var slot);

            var ct = await _customerType.FindAsync(x => x.Id == b.Customer.CustomerTypeId);
            var ctName = ct?.Name ?? ct?.Code ?? "N/A";

            dtoList.Add(new AppBookingDto
            {
                Id = b.Id,
                TenantId = b.TenantId,
                BookingCode = b.BookingCode,
                CustomerId = b.CustomerId,
                CustomerType = ctName,
                CustomerName = c?.FullName,
                CustomerPhone = c?.PhoneNumber,
                GolfCourseId = b.GolfCourseId,
                GolfCourseName = g?.Name,
                CalendarSlotId = b.CalendarSlotId,
                PlayDate = b.PlayDate,
                TimeFrom = slot?.TimeFrom,
                TimeTo = slot?.TimeTo,
                NumberOfGolfers = b.NumberOfGolfers,
                PricePerGolfer = b.PricePerGolfer,
                TotalAmount = b.TotalAmount,
                IsExportInvoice = b.IsExportInvoice,
                CompanyName = b.CompanyName,
                TaxCode = b.TaxCode,
                CompanyAddress = b.CompanyAddress,
                InvoiceEmail = b.InvoiceEmail,
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
        var calendarSlot = await _calendarSlotRepository.FindAsync(p => p.Id == booking.CalendarSlotId);

        var ct = await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId);
        var ctName = ct?.Name ?? ct?.Code ?? "N/A";

        var dto = new AppBookingDto
        {
            Id = booking.Id,
            TenantId = booking.TenantId,
            BookingCode = booking.BookingCode,
            CustomerId = booking.CustomerId,
            CustomerType = ctName,
            CustomerName = customer?.FullName,
            CustomerPhone = customer?.PhoneNumber,
            GolfCourseId = booking.GolfCourseId,
            GolfCourseName = golfCourse?.Name,
            CalendarSlotId = booking.CalendarSlotId,
            PlayDate = booking.PlayDate,
            TimeFrom = calendarSlot?.TimeFrom,
            TimeTo = calendarSlot?.TimeTo,
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
            IsExportInvoice = booking.IsExportInvoice,
            CompanyName = booking.CompanyName,
            TaxCode = booking.TaxCode,
            CompanyAddress = booking.CompanyAddress,
            InvoiceEmail = booking.InvoiceEmail,
            Players = players.Select(p => new AppBookingPlayerDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                CustomerId = p.CustomerId,
                PlayerName = p.PlayerName,
                PricePerPlayer = p.PricePerPlayer,
                VgaCode = p.VgaCode,
                Notes = p.Notes
            }).ToList()
        };

        return dto;
    }

    public override async Task<AppBookingDto> CreateAsync(CreateUpdateAppBookingDto input)
    {
        await CheckCreatePolicyAsync();

        var customer = await _customerRepository.GetAsync(input.CustomerId);

        if (!input.CalendarSlotId.HasValue || input.CalendarSlotId.Value == Guid.Empty)
            throw new UserFriendlyException("CalendarSlotId is required");

        var calendarSlot = await _calendarSlotRepository.GetAsync(input.CalendarSlotId.Value);

        var bookingCode = await GenerateBookingCodeAsync(customer.CustomerCode, input.PlayDate);

        var entity = new Booking(
            GuidGenerator.Create(),
            bookingCode,
            input.CustomerId,
            input.GolfCourseId,
            input.CalendarSlotId.Value,
            input.PlayDate,
            input.NumberOfGolfers,
            input.PricePerGolfer ?? 0m,
            input.TotalAmount,
            input.PaymentMethod,
            input.Status,
            input.Source
        );

        entity = await Repository.InsertAsync(entity, autoSave: true);

        await SavePlayersAsync(entity.Id, input.Players);
        await CurrentUnitOfWork.SaveChangesAsync();

        // ✅ Map CustomerTypeSummary chuẩn theo CustomerTypeId
        var ct = await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId);
        var ctName = ct?.Name ?? ct?.Code ?? "N/A";
        var customerTypeSummary = $"{ctName}";

        // ✅ Enqueue email (không block)
        try
        {
            static string ToHHmm(TimeSpan? ts) => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : "";
            static string ToDDMMYYYY(DateTime dt) => dt.ToString("dd/MM/yyyy");

            var otherRequestsText = await BuildOtherRequestsTextAsync(entity.Utility);
            var model = new BookingNewRequestEmailModelDto
            {
                BookingCode = entity.BookingCode,
                BookerName = customer.FullName ?? "N/A",
                BookerPhone = customer.PhoneNumber ?? "N/A",

                // giữ DateTime gốc để audit
                PlayDate = entity.PlayDate,
                PlayDateText = ToDDMMYYYY(entity.PlayDate),

                // giờ chơi (TimeSpan -> HH:mm)
                TeeTimeFromText = ToHHmm(calendarSlot?.TimeFrom),
                TeeTimeToText = ToHHmm(calendarSlot?.TimeTo),

                // nếu vẫn muốn TeeTime gộp
                TeeTime = $"{ToHHmm(calendarSlot?.TimeFrom)} - {ToHHmm(calendarSlot?.TimeTo)}",

                NumberOfGolfers = entity.NumberOfGolfers,
                CustomerTypeSummary = customerTypeSummary,

                TotalAmount = entity.TotalAmount,
                TotalAmountText = $"{entity.TotalAmount:N0} đ",

                PaymentMethod = entity.PaymentMethod.ToString(),

                OtherRequests = otherRequestsText,

                IsExportInvoice = entity.IsExportInvoice,
                CompanyName = entity.CompanyName,
                TaxCode = entity.TaxCode,
                CompanyAddress = entity.CompanyAddress,
                InvoiceEmail = entity.InvoiceEmail
            };

            // lấy cấu hình động theo tenant
            var cfg = await GetEmailConfigAsync(
                AppEmailSettingNames.BookingNew_ToEmails,
                AppEmailSettingNames.BookingNew_CcEmails,
                AppEmailSettingNames.BookingNew_BccEmails,
                AppEmailSettingNames.BookingNew_SubjectTemplate,
                entity.BookingCode,
                fallbackTo: "sales@montgomerielinks.com" // fallback nếu chưa set
            );

            await _appEmailSenderService.EnqueueTemplateAsync(
                templateName: AppEmailTemplateNames.BookingNewRequest,
                model: model,
                toEmails: cfg.To,
                subject: cfg.Subject,
                cc: cfg.Cc,
                bcc: cfg.Bcc,
                bookingId: entity.Id,
                bookingCode: entity.BookingCode
            );
        }
        catch
        {
            // ✅ không throw để tránh ảnh hưởng kết quả booking của mini app
        }

        return await GetAsync(entity.Id);
    }

    public override async Task<AppBookingDto> UpdateAsync(Guid id, CreateUpdateAppBookingDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);

        if (entity.Status == BookingStatus.CancelledRefund || entity.Status == BookingStatus.CancelledNoRefund) throw new UserFriendlyException("Booking đã hủy, không thể thao tác.");

        // ===== BEFORE SNAPSHOT =====
        var oldPlayers = await _playerRepository.GetListAsync(p => p.BookingId == id);

        var oldStatus = entity.Status;
        var oldPaymentMethod = entity.PaymentMethod;
        var oldNumberOfGolfers = entity.NumberOfGolfers;

        var oldStatusText = _l[$"BookingStatus:{oldStatus}"];
        var oldPaymentText = oldPaymentMethod.HasValue
            ? _l[$"PaymentMethod:{oldPaymentMethod.Value}"]
            : "N/A";

        // customer của booking (để giữ nguyên tên KH)
        var oldCustomer = await _customerRepository.GetAsync(entity.CustomerId);

        CalendarSlot? oldSlot = null;
        if (entity.CalendarSlotId.HasValue && entity.CalendarSlotId.Value != Guid.Empty)
            oldSlot = await _calendarSlotRepository.FindAsync(entity.CalendarSlotId.Value);

        static string ToDDMMYYYY(DateTime dt) => dt.ToString("dd/MM/yyyy");
        static string ToHHmm(TimeSpan? ts) => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : "";

        var oldPlayDateText = ToDDMMYYYY(entity.PlayDate);
        var oldTeeFromText = ToHHmm(oldSlot?.TimeFrom);
        var oldTeeToText = ToHHmm(oldSlot?.TimeTo);

        // ===== APPLY UPDATE =====
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

        // ===== AFTER SNAPSHOT =====
        var newPlayers = await _playerRepository.GetListAsync(p => p.BookingId == id);

        var newStatus = entity.Status;
        var newPaymentMethod = entity.PaymentMethod;
        var newNumberOfGolfers = entity.NumberOfGolfers;

        var newStatusText = _l[$"BookingStatus:{newStatus}"];
        var newPaymentText = newPaymentMethod.HasValue
            ? _l[$"PaymentMethod:{newPaymentMethod.Value}"]
            : "N/A";

        var newCustomer = await _customerRepository.GetAsync(entity.CustomerId);

        CalendarSlot? newSlot = null;
        if (entity.CalendarSlotId.HasValue && entity.CalendarSlotId.Value != Guid.Empty)
            newSlot = await _calendarSlotRepository.FindAsync(entity.CalendarSlotId.Value);

        var newPlayDateText = ToDDMMYYYY(entity.PlayDate);
        var newTeeFromText = ToHHmm(newSlot?.TimeFrom);
        var newTeeToText = ToHHmm(newSlot?.TimeTo);

        // ===== helpers compare =====
        static string PlayersSig(IEnumerable<BookingPlayer> ps) =>
            string.Join("|",
                ps.Select(p =>
                    $"{(p.PlayerName ?? "").Trim()}#{(p.VgaCode ?? "").Trim()}#{(p.PricePerPlayer ?? 0m):0.##}"
                ).OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            );

        var hasPlayerChanges =
            !string.Equals(PlayersSig(oldPlayers), PlayersSig(newPlayers), StringComparison.OrdinalIgnoreCase);

        var hasHeaderChanges =
            oldStatus != newStatus
            || oldPaymentMethod != newPaymentMethod
            || oldNumberOfGolfers != newNumberOfGolfers
            || oldPlayDateText != newPlayDateText
            || oldTeeFromText != newTeeFromText
            || oldTeeToText != newTeeToText;

        // ✅ chỉ gửi email hủy khi chuyển từ status KHÁC -> 4/5
        var becameCancelled = (oldStatus != BookingStatus.CancelledRefund && oldStatus != BookingStatus.CancelledNoRefund)
                   && (newStatus == BookingStatus.CancelledRefund || newStatus == BookingStatus.CancelledNoRefund);

        // ===== ENQUEUE EMAIL (NON-BLOCKING) =====
        try
        {
            if (becameCancelled)
            {
                // ✅ Requester = admin thao tác (CurrentUser), format "FullName (UserName)"
                var fullName = (CurrentUser?.Name ?? "").Trim();
                var userName = (CurrentUser?.UserName ?? "").Trim();

                string requesterName;
                if (!string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(userName))
                    requesterName = $"{fullName} ({userName})";
                else if (!string.IsNullOrWhiteSpace(fullName))
                    requesterName = fullName;
                else if (!string.IsNullOrWhiteSpace(userName))
                    requesterName = userName;
                else
                    requesterName = "N/A";

                var requesterPhone =
                    CurrentUser?.FindClaim("phone_number")?.Value
                    ?? CurrentUser?.FindClaim(System.Security.Claims.ClaimTypes.MobilePhone)?.Value
                    ?? CurrentUser?.FindClaim(System.Security.Claims.ClaimTypes.HomePhone)?.Value
                    ?? "N/A";

                // ✅ giữ nguyên customer của booking (oldCustomer) để không đổi tên KH
                var customerName = oldCustomer?.FullName ?? "N/A";
                var customerPhone = oldCustomer?.PhoneNumber ?? "N/A";

                var cancelModel = new BookingCancelRequestEmailModelDto
                {
                    BookingCode = entity.BookingCode,

                    BookerName = customerName,
                    BookerPhone = customerPhone,

                    CancelRequesterName = requesterName,
                    CancelRequesterPhone = requesterPhone,

                    PlayDateText = newPlayDateText,
                    TeeTimeFromText = newTeeFromText,
                    TeeTimeToText = newTeeToText,
                    NumberOfGolfers = entity.NumberOfGolfers,

                    CancelStatusText = newStatusText
                };

                // lấy cấu hình động theo tenant
                var cfg = await GetEmailConfigAsync(
                    AppEmailSettingNames.BookingCancel_ToEmails,
                    AppEmailSettingNames.BookingCancel_CcEmails,
                    AppEmailSettingNames.BookingCancel_BccEmails,
                    AppEmailSettingNames.BookingCancel_SubjectTemplate,
                    entity.BookingCode,
                    fallbackTo: "sales@montgomerielinks.com"
                );

                await _appEmailSenderService.EnqueueTemplateAsync(
                    templateName: AppEmailTemplateNames.BookingCancelRequest,
                    model: cancelModel,
                    toEmails: cfg.To,
                    subject: cfg.Subject,
                    cc: cfg.Cc,
                    bcc: cfg.Bcc,
                    bookingId: entity.Id,
                    bookingCode: entity.BookingCode
                );
            }
            else
            {
                var changeModel = new BookingChangeRequestEmailModelDto
                {
                    BookingCode = entity.BookingCode,
                    BookerName = newCustomer?.FullName ?? "N/A",
                    BookerPhone = newCustomer?.PhoneNumber ?? "N/A",

                    OldStatusText = oldStatusText,
                    OldPaymentMethodText = oldPaymentText,
                    OldNumberOfGolfers = oldNumberOfGolfers,

                    OldPlayersText = BuildPlayersText(oldPlayers),
                    NewPlayersText = BuildPlayersText(newPlayers),

                    NewStatusText = newStatusText,
                    NewPaymentMethodText = newPaymentText,
                    NewNumberOfGolfers = newNumberOfGolfers,

                    HasPlayerChanges = hasPlayerChanges,
                    HasHeaderChanges = hasHeaderChanges
                };

                // lấy cấu hình động theo tenant
                var cfg = await GetEmailConfigAsync(
                   AppEmailSettingNames.BookingChange_ToEmails,
                   AppEmailSettingNames.BookingChange_CcEmails,
                   AppEmailSettingNames.BookingChange_BccEmails,
                   AppEmailSettingNames.BookingChange_SubjectTemplate,
                   entity.BookingCode,
                   fallbackTo: "sales@montgomerielinks.com"
               );

                await _appEmailSenderService.EnqueueTemplateAsync(
                    templateName: AppEmailTemplateNames.BookingChangeRequest,
                    model: changeModel,
                    toEmails: cfg.To,
                    subject: cfg.Subject,
                    cc: cfg.Cc,
                    bcc: cfg.Bcc,
                    bookingId: entity.Id,
                    bookingCode: entity.BookingCode
                );
            }
        }
        catch
        {
            // không throw để không ảnh hưởng Update booking
        }

        return await GetAsync(entity.Id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var entity = await Repository.GetAsync(id);
        if (entity.Status == BookingStatus.CancelledRefund || entity.Status == BookingStatus.CancelledNoRefund) throw new UserFriendlyException("Booking đã hủy, không thể thao tác.");

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
                p.PricePerPlayer,
                p.VgaCode,
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
        var customerQueryable = await _customerRepository.GetQueryableAsync();

        // ✅ FilterText: BookingCode OR FullName OR CustomerCode OR Phone
        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();

            query = query.Where(b =>
                b.BookingCode.Contains(filter)
                || customerQueryable.Any(c =>
                    c.Id == b.CustomerId
                    && (
                        c.FullName.Contains(filter)
                        || (c.CustomerCode != null && c.CustomerCode.Contains(filter))
                        || (c.PhoneNumber != null && c.PhoneNumber.Contains(filter))
                    )
                )
            );
        }

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (input.Source.HasValue)
            query = query.Where(x => x.Source == input.Source.Value);

        if (input.PlayDateFrom.HasValue)
            query = query.Where(x => x.PlayDate >= input.PlayDateFrom.Value);

        if (input.PlayDateTo.HasValue)
            query = query.Where(x => x.PlayDate <= input.PlayDateTo.Value);

        var list = await AsyncExecuter.ToListAsync(query);

        // Customers
        var customerIds = list.Select(x => x.CustomerId).Distinct().ToList();
        var customers = customerIds.Count == 0
            ? new List<Customer>()
            : await _customerRepository.GetListAsync(x => customerIds.Contains(x.Id));
        var customerDict = customers.ToDictionary(x => x.Id, x => x);

        // Calendar slots (to get TimeFrom/TimeTo)
        var slotIds = list
            .Where(x => x.CalendarSlotId.HasValue && x.CalendarSlotId.Value != Guid.Empty)
            .Select(x => x.CalendarSlotId!.Value)
            .Distinct()
            .ToList();

        var slots = slotIds.Count == 0
            ? new List<CalendarSlot>()
            : await _calendarSlotRepository.GetListAsync(s => slotIds.Contains(s.Id));
        var slotDict = slots.ToDictionary(s => s.Id, s => s);

        static string ToHHmm(TimeSpan? ts)
            => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : "";

        // Localize helpers (tối ưu: map ngay ở Service, ExcelExporter chỉ render text)
        string LStatus(BookingStatus s) => _l[$"BookingStatus:{s}"];
        string LPayment(PaymentMethod? pm)
        {
            if (!pm.HasValue) return "";
            return _l[$"PaymentMethod:{pm.Value}"];
        }
        string LSource(BookingSource src) => _l[$"BookingSource:{src}"];

        var rows = list.Select(b =>
        {
            customerDict.TryGetValue(b.CustomerId, out var c);

            CalendarSlot? slot = null;
            if (b.CalendarSlotId.HasValue)
                slotDict.TryGetValue(b.CalendarSlotId.Value, out slot);

            var customerDisplay = (c?.FullName ?? "")
                + (!string.IsNullOrWhiteSpace(c?.PhoneNumber) ? $" ({c!.PhoneNumber})" : "");

            var from = ToHHmm(slot?.TimeFrom);
            var to = ToHHmm(slot?.TimeTo);
            var playTime = (!string.IsNullOrWhiteSpace(from) || !string.IsNullOrWhiteSpace(to))
                ? (string.IsNullOrWhiteSpace(from) ? to : (string.IsNullOrWhiteSpace(to) ? from : $"{from} - {to}"))
                : "";

            return new AppBookingExcelRowDto
            {
                BookingCode = b.BookingCode,
                Customer = customerDisplay,
                PlayDate = b.PlayDate,
                PlayTime = playTime,
                NumberOfGolfers = b.NumberOfGolfers,
                TotalAmount = b.TotalAmount,
                IsExportInvoice = b.IsExportInvoice,

                CompanyName = b.CompanyName,
                TaxCode = b.TaxCode,
                CompanyAddress = b.CompanyAddress,
                InvoiceEmail = b.InvoiceEmail,

                // ✅ Localized text (quan trọng)
                PaymentMethod = LPayment(b.PaymentMethod),
                Status = LStatus(b.Status),
                Source = LSource(b.Source)
            };
        }).ToList();

        return _excelExporter.Export(rows);
    }

    private static string? NullIfEmpty(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Trim();
    }

    private static string ApplyTemplate(string? template, string bookingCode)
    {
        template ??= "[ZALO MINI APP] YÊU CẦU ĐẶT CHỖ MỚI – {BookingCode}";
        return template.Replace("{BookingCode}", bookingCode ?? "");
    }

    private static string ApplySubjectTemplate(string? template, string bookingCode)
    {
        template ??= "{BookingCode}";
        return template.Replace("{BookingCode}", bookingCode ?? "");
    }

    private async Task<(string To, string? Cc, string? Bcc, string Subject)> GetEmailConfigAsync(
        string toKey, string ccKey, string bccKey, string subjectKey,
        string bookingCode, string fallbackTo)
    {
        var to = await _settingProvider.GetOrNullAsync(toKey);
        var cc = await _settingProvider.GetOrNullAsync(ccKey);
        var bcc = await _settingProvider.GetOrNullAsync(bccKey);
        var subjectTpl = await _settingProvider.GetOrNullAsync(subjectKey);

        return (
            To: to ?? fallbackTo,
            Cc: NullIfEmpty(cc),
            Bcc: NullIfEmpty(bcc),
            Subject: ApplySubjectTemplate(subjectTpl, bookingCode)
        );
    }

    private static List<int> ParseUtilityIds(string? utility)
    {
        if (string.IsNullOrWhiteSpace(utility)) return new List<int>();

        return utility
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var n) ? n : (int?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();
    }

    private async Task<string> BuildOtherRequestsTextAsync(string? utility)
    {
        var ids = ParseUtilityIds(utility);
        if (ids.Count == 0) return ""; // để tpl tự in "Không có"

        var query = await _optionExtendRepo.GetQueryableAsync();

        // chỉ lấy tiện ích sân golf (Type=1) và match theo OptionId
        var names = query
            .Where(x => x.Type == OptionExtendTypeEnum.GolfCourseUlitity.Value && ids.Contains(x.OptionId))
            .Select(x => x.OptionName)
            .ToList();

        // giữ thứ tự theo ids (optional)
        var nameDict = query
            .Where(x => x.Type == OptionExtendTypeEnum.GolfCourseUlitity.Value && ids.Contains(x.OptionId))
            .ToDictionary(x => x.OptionId, x => x.OptionName);

        var ordered = ids
            .Where(id => nameDict.ContainsKey(id))
            .Select(id => nameDict[id])
            .ToList();

        // format bullet lines
        return string.Join("\n", ordered.Select(n => $"- {n}"));
    }

    private static string BuildPlayersText(List<BookingPlayer> players)
    {
        if (players == null || players.Count == 0) return "";

        static string Money(decimal? v) => v.HasValue ? $"{v.Value:N0} đ" : "N/A";
        static string Text(string? s) => string.IsNullOrWhiteSpace(s) ? "N/A" : s.Trim();

        // Format đúng yêu cầu:
        // Người chơi 1:
        // • Tên người chơi cũ: ...
        // • Mã hội viên cũ: ...
        // • Giá / golfer cũ: ...
        // • Ghi chú cũ: ...
        //
        // Người chơi 2: ...
        var lines = new List<string>();

        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            lines.Add($"Người chơi {i + 1}:");
            lines.Add($"• Tên người chơi: {Text(p.PlayerName)}");
            lines.Add($"• Mã hội viên: {Text(p.VgaCode)}");
            lines.Add($"• Giá / golfer: {Money(p.PricePerPlayer)}");
            lines.Add($"• Ghi chú: {Text(p.Notes)}");

            if (i < players.Count - 1) lines.Add(""); // ngăn cách giữa các người chơi
        }

        return string.Join(Environment.NewLine, lines);
    }

}