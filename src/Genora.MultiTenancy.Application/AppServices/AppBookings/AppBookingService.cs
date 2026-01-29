using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Content;
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

    // ✅ enqueue ZBS via Background Job
    private readonly IBackgroundJobManager _jobManager;

    // NOTE: format yêu cầu bởi Zalo template (không phụ thuộc ngôn ngữ UI)
    private const string ZaloDateFormat = "dd/MM/yyyy";

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
        ISettingProvider settingProvider,
        IBackgroundJobManager jobManager)
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
        _jobManager = jobManager;
    }

    private string NA() => _l["Common:NA"].Value;

    private string CurrencySuffix() => _l["Common:CurrencySuffix"].Value;

    private string F(string code, params object[] args)
    {
        var template = _l[code].Value;
        if (string.IsNullOrWhiteSpace(template)) template = code;

        if (args == null || args.Length == 0) return template;

        try { return string.Format(CultureInfo.CurrentCulture, template, args); }
        catch { return template; }
    }

    private string MoneyText(decimal? v)
    {
        if (!v.HasValue) return NA();
        return string.Format(CultureInfo.CurrentCulture, "{0:N0} {1}", v.Value, CurrencySuffix()).Trim();
    }

    private string CustomerDisplayText(Customer? c)
    {
        if (c == null) return NA();

        var name = (c.FullName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = NA();

        var phone = (c.PhoneNumber ?? "").Trim();
        if (string.IsNullOrWhiteSpace(phone)) return name;

        return F("Common:NameWithPhone", name, phone);
    }

    private string ToDDMMYYYY(DateTime dt) => dt.ToString(ZaloDateFormat, CultureInfo.InvariantCulture);

    private string ToHHmm(TimeSpan? ts) => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : "";

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

        if (input.CustomerId.HasValue) query = query.Where(b => b.CustomerId == input.CustomerId.Value);
        if (input.GolfCourseId.HasValue) query = query.Where(b => b.GolfCourseId == input.GolfCourseId.Value);
        if (input.Status.HasValue) query = query.Where(b => b.Status == input.Status.Value);
        if (input.Source.HasValue) query = query.Where(b => b.Source == input.Source.Value);
        if (input.PlayDateFrom.HasValue) query = query.Where(b => b.PlayDate >= input.PlayDateFrom.Value);
        if (input.PlayDateTo.HasValue) query = query.Where(b => b.PlayDate <= input.PlayDateTo.Value);

        var sorting = input.Sorting.IsNullOrWhiteSpace()
            ? nameof(Booking.CreationTime) + " desc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

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

            var ct = c?.CustomerTypeId.HasValue == true
                ? await _customerType.FindAsync(x => x.Id == c.CustomerTypeId)
                : null;

            var ctName = ct?.Name ?? ct?.Code ?? NA();

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
        var calendarSlot = booking.CalendarSlotId.HasValue
            ? await _calendarSlotRepository.FindAsync(p => p.Id == booking.CalendarSlotId)
            : null;

        var ct = customer?.CustomerTypeId.HasValue == true
            ? await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId)
            : null;

        var ctName = ct?.Name ?? ct?.Code ?? NA();

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
        {
            throw new BusinessException(BookingErrorCodes.CalendarSlotRequired)
                .WithData("Field", "CalendarSlotId");
        }

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

        try
        {
            if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
            {
                await _jobManager.EnqueueAsync(
                    new ZbsSendJobArgs
                    {
                        TenantId = CurrentTenant.Id,
                        TemplateKey = "BookingCreated",
                        Phone = customer.PhoneNumber,
                        TrackingId = entity.Id.ToString(),
                        TemplateData = new
                        {
                            customer_name = customer.FullName,
                            booking_id = entity.BookingCode,
                            tee_off_date = entity.PlayDate.ToString(ZaloDateFormat, CultureInfo.InvariantCulture),
                            tee_off_time = $"{calendarSlot.TimeFrom:hh\\:mm}",
                            number_of_player = entity.NumberOfGolfers
                        }
                    },
                    priority: BackgroundJobPriority.Normal
                );
            }
        }
        catch { }

        var ct = await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId);
        var ctName = ct?.Name ?? ct?.Code ?? NA();
        var customerTypeSummary = $"{ctName}";

        try
        {
            var otherRequestsText = await BuildOtherRequestsTextAsync(entity.Utility);

            var paymentText = _l[$"PaymentMethod:{entity.PaymentMethod}"].Value;
            if (string.IsNullOrWhiteSpace(paymentText) || paymentText.StartsWith("PaymentMethod:", StringComparison.OrdinalIgnoreCase))
                paymentText = entity.PaymentMethod.ToString();

            var model = new BookingNewRequestEmailModelDto
            {
                BookingCode = entity.BookingCode,
                BookerName = (customer.FullName ?? "").Trim().IsNullOrWhiteSpace() ? NA() : customer.FullName,
                BookerPhone = (customer.PhoneNumber ?? "").Trim().IsNullOrWhiteSpace() ? NA() : customer.PhoneNumber,

                PlayDate = entity.PlayDate,
                PlayDateText = entity.PlayDate.ToString(ZaloDateFormat, CultureInfo.InvariantCulture),

                TeeTimeFromText = ToHHmm(calendarSlot?.TimeFrom),
                TeeTimeToText = ToHHmm(calendarSlot?.TimeTo),
                TeeTime = $"{ToHHmm(calendarSlot?.TimeFrom)} - {ToHHmm(calendarSlot?.TimeTo)}",

                NumberOfGolfers = entity.NumberOfGolfers,
                CustomerTypeSummary = customerTypeSummary,

                TotalAmount = entity.TotalAmount,
                TotalAmountText = MoneyText(entity.TotalAmount),

                PaymentMethod = paymentText,
                OtherRequests = otherRequestsText,

                IsExportInvoice = entity.IsExportInvoice,
                CompanyName = entity.CompanyName,
                TaxCode = entity.TaxCode,
                CompanyAddress = entity.CompanyAddress,
                InvoiceEmail = entity.InvoiceEmail
            };

            var cfg = await GetEmailConfigAsync(
                AppEmailSettingNames.BookingNew_ToEmails,
                AppEmailSettingNames.BookingNew_CcEmails,
                AppEmailSettingNames.BookingNew_BccEmails,
                AppEmailSettingNames.BookingNew_SubjectTemplate,
                entity.BookingCode,
                fallbackTo: "sales@montgomerielinks.com"
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
        catch { }

        return await GetAsync(entity.Id);
    }

    public override async Task<AppBookingDto> UpdateAsync(Guid id, CreateUpdateAppBookingDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);

        if (entity.Status == BookingStatus.CancelledRefund || entity.Status == BookingStatus.CancelledNoRefund)
        {
            throw new BusinessException(BookingErrorCodes.BookingCancelledReadonly)
                .WithData("BookingId", id)
                .WithData("Status", entity.Status.ToString());
        }

        var oldPlayers = await _playerRepository.GetListAsync(p => p.BookingId == id);

        var oldStatus = entity.Status;
        var oldPaymentMethod = entity.PaymentMethod;
        var oldNumberOfGolfers = entity.NumberOfGolfers;

        var oldStatusText = _l[$"BookingStatus:{oldStatus}"].Value;
        if (string.IsNullOrWhiteSpace(oldStatusText) || oldStatusText.StartsWith("BookingStatus:", StringComparison.OrdinalIgnoreCase))
            oldStatusText = oldStatus.ToString();

        var oldPaymentText = oldPaymentMethod.HasValue
            ? _l[$"PaymentMethod:{oldPaymentMethod.Value}"].Value
            : NA();

        if (oldPaymentMethod.HasValue && (string.IsNullOrWhiteSpace(oldPaymentText) || oldPaymentText.StartsWith("PaymentMethod:", StringComparison.OrdinalIgnoreCase)))
            oldPaymentText = oldPaymentMethod.Value.ToString();

        var oldCustomer = await _customerRepository.GetAsync(entity.CustomerId);

        CalendarSlot? oldSlot = null;
        if (entity.CalendarSlotId.HasValue && entity.CalendarSlotId.Value != Guid.Empty)
            oldSlot = await _calendarSlotRepository.FindAsync(entity.CalendarSlotId.Value);

        var oldPlayDateText = entity.PlayDate.ToString(ZaloDateFormat, CultureInfo.InvariantCulture);
        var oldTeeFromText = ToHHmm(oldSlot?.TimeFrom);
        var oldTeeToText = ToHHmm(oldSlot?.TimeTo);

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

        var newPlayers = await _playerRepository.GetListAsync(p => p.BookingId == id);

        var newStatus = entity.Status;
        var newPaymentMethod = entity.PaymentMethod;
        var newNumberOfGolfers = entity.NumberOfGolfers;

        var newStatusText = _l[$"BookingStatus:{newStatus}"].Value;
        if (string.IsNullOrWhiteSpace(newStatusText) || newStatusText.StartsWith("BookingStatus:", StringComparison.OrdinalIgnoreCase))
            newStatusText = newStatus.ToString();

        var newPaymentText = newPaymentMethod.HasValue
            ? _l[$"PaymentMethod:{newPaymentMethod.Value}"].Value
            : NA();

        if (newPaymentMethod.HasValue && (string.IsNullOrWhiteSpace(newPaymentText) || newPaymentText.StartsWith("PaymentMethod:", StringComparison.OrdinalIgnoreCase)))
            newPaymentText = newPaymentMethod.Value.ToString();

        var newCustomer = await _customerRepository.GetAsync(entity.CustomerId);

        CalendarSlot? newSlot = null;
        if (entity.CalendarSlotId.HasValue && entity.CalendarSlotId.Value != Guid.Empty)
            newSlot = await _calendarSlotRepository.FindAsync(entity.CalendarSlotId.Value);

        var newPlayDateText = entity.PlayDate.ToString(ZaloDateFormat, CultureInfo.InvariantCulture);
        var newTeeFromText = ToHHmm(newSlot?.TimeFrom);
        var newTeeToText = ToHHmm(newSlot?.TimeTo);

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

        var becameCancelled =
            (oldStatus != BookingStatus.CancelledRefund && oldStatus != BookingStatus.CancelledNoRefund)
            && (newStatus == BookingStatus.CancelledRefund || newStatus == BookingStatus.CancelledNoRefund);

        try
        {
            if (becameCancelled)
            {
                if (!string.IsNullOrWhiteSpace(oldCustomer.PhoneNumber))
                {
                    await _jobManager.EnqueueAsync(
                        new ZbsSendJobArgs
                        {
                            TenantId = CurrentTenant.Id,
                            TemplateKey = "BookingCancelled",
                            Phone = oldCustomer.PhoneNumber,
                            TrackingId = entity.Id.ToString(),
                            TemplateData = new
                            {
                                customer_name = oldCustomer.FullName,
                                booking_code = entity.BookingCode,
                                tee_off_date = newPlayDateText,
                                tee_off_time = newTeeFromText
                            }
                        },
                        priority: BackgroundJobPriority.Normal
                    );
                }

                var fullName = (CurrentUser?.Name ?? "").Trim();
                var userName = (CurrentUser?.UserName ?? "").Trim();

                string requesterName;
                if (!string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(userName))
                    requesterName = F("Common:RequesterNameWithUserName", fullName, userName);
                else if (!string.IsNullOrWhiteSpace(fullName))
                    requesterName = fullName;
                else if (!string.IsNullOrWhiteSpace(userName))
                    requesterName = userName;
                else
                    requesterName = NA();

                var requesterPhone = CurrentUser?.FindClaim("phone_number")?.Value
                    ?? CurrentUser?.FindClaim(System.Security.Claims.ClaimTypes.MobilePhone)?.Value
                    ?? CurrentUser?.FindClaim(System.Security.Claims.ClaimTypes.HomePhone)?.Value
                    ?? NA();

                var customerName = oldCustomer?.FullName ?? NA();
                var customerPhone = oldCustomer?.PhoneNumber ?? NA();

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
                if (!string.IsNullOrWhiteSpace(newCustomer.PhoneNumber))
                {
                    await _jobManager.EnqueueAsync(
                        new ZbsSendJobArgs
                        {
                            TenantId = CurrentTenant.Id,
                            TemplateKey = "BookingChanged",
                            Phone = newCustomer.PhoneNumber,
                            TrackingId = entity.Id.ToString(),
                            TemplateData = new
                            {
                                customer_name = newCustomer.FullName,
                                booking_code = entity.BookingCode,
                                tee_off_date = newPlayDateText,
                                tee_off_time = $"{newTeeFromText}",
                                number_of_player = newNumberOfGolfers
                            }
                        },
                        priority: BackgroundJobPriority.Normal
                    );
                }

                var changeModel = new BookingChangeRequestEmailModelDto
                {
                    BookingCode = entity.BookingCode,
                    BookerName = newCustomer?.FullName ?? NA(),
                    BookerPhone = newCustomer?.PhoneNumber ?? NA(),

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
        if (entity.Status == BookingStatus.CancelledRefund || entity.Status == BookingStatus.CancelledNoRefund)
        {
            throw new BusinessException(BookingErrorCodes.BookingCancelledReadonly)
                .WithData("BookingId", id)
                .WithData("Status", entity.Status.ToString());
        }

        await _playerRepository.DeleteAsync(p => p.BookingId == id);
        await Repository.DeleteAsync(id);
    }

    private async Task<string> GenerateBookingCodeAsync(string customerCode, DateTime playDate)
    {
        var dayPart = playDate.ToString("ddMMyy", CultureInfo.InvariantCulture);
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
            if (int.TryParse(suffix, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n > maxSeq) maxSeq = n;
        }

        var nextSeq = maxSeq + 1;
        var seqPart = nextSeq.ToString("D3", CultureInfo.InvariantCulture);

        return prefix + seqPart;
    }

    private async Task SavePlayersAsync(Guid bookingId, List<CreateUpdateBookingPlayerDto> players)
    {
        await _playerRepository.DeleteAsync(p => p.BookingId == bookingId);

        if (players == null || !players.Any()) return;

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
            if (r.PlayDate == DateTime.MinValue)
            {
                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.PlayDateRequired, rowNumber, "PlayDate", null);
            }

            if (r.NumberOfGolfers <= 0)
            {
                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.NumberOfGolfersInvalid, rowNumber, "NumberOfGolfers", r.NumberOfGolfers);
            }

            if (r.TotalAmount <= 0)
            {
                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.TotalAmountInvalid, rowNumber, "TotalAmount", r.TotalAmount);
            }

            var statusRaw = (r.Status ?? "").Trim();
            if (string.IsNullOrWhiteSpace(statusRaw))
            {
                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.StatusRequired, rowNumber, "Status", null, null);
            }

            if (!Enum.TryParse<BookingStatus>(statusRaw, ignoreCase: true, out var status))
            {
                var allowed = string.Join(", ", Enum.GetNames(typeof(BookingStatus)));
                var detail = F("BookingImport:StatusInvalid_Data", statusRaw, allowed);

                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.StatusInvalid, rowNumber, "Status", statusRaw, detail)
                    .WithData("Allowed", allowed);
            }

            var paymentRaw = (r.PaymentMethod ?? "").Trim();
            if (string.IsNullOrWhiteSpace(paymentRaw))
            {
                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.PaymentMethodRequired, rowNumber, "PaymentMethod", null);
            }

            if (!Enum.TryParse<PaymentMethod>(paymentRaw, ignoreCase: true, out var payment))
            {
                var allowed = string.Join(", ", Enum.GetNames(typeof(PaymentMethod)));
                var detail = F("BookingImport:PaymentMethodInvalid_Data", paymentRaw, allowed);

                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.PaymentMethodInvalid, rowNumber, "PaymentMethod", paymentRaw, detail)
                    .WithData("Allowed", allowed);
            }

            var sourceRaw = (r.Source ?? "").Trim();
            if (string.IsNullOrWhiteSpace(sourceRaw))
            {
                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.SourceRequired, rowNumber, "Source", null);
            }

            if (!Enum.TryParse<BookingSource>(sourceRaw, ignoreCase: true, out var source))
            {
                var allowed = string.Join(", ", Enum.GetNames(typeof(BookingSource)));
                var detail = F("BookingImport:SourceInvalid_Data", sourceRaw, allowed);

                throw ErrorHelper.ImportError(_l, BookingImportErrorCodes.SourceInvalid, rowNumber, "Source", sourceRaw, detail)
                    .WithData("Allowed", allowed);
            }

            var bookingCodeInFile = (r.BookingCode ?? "").Trim();
            Booking? booking = null;

            if (!string.IsNullOrWhiteSpace(bookingCodeInFile))
            {
                booking = await Repository.FirstOrDefaultAsync(x => x.BookingCode == bookingCodeInFile);
            }

            if (booking == null)
            {
                var datePart = r.PlayDate.ToString("ddMMyy", CultureInfo.InvariantCulture);
                var countInDay = await Repository.CountAsync(x => x.PlayDate.Date == r.PlayDate.Date);
                var serial = (countInDay + 1).ToString("D3", CultureInfo.InvariantCulture);

                var bookingCode = $"KH000001-{datePart}-{serial}";

                booking = new Booking(
                    GuidGenerator.Create(),
                    bookingCode,
                    new Guid("8DA661EA-D7A2-1692-5B49-3A1E329C52B3"),
                    new Guid("C38585C6-8996-11C8-B721-3A1E329BAF6E"),
                    Guid.Empty,
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

        if (input.Status.HasValue) query = query.Where(x => x.Status == input.Status.Value);
        if (input.Source.HasValue) query = query.Where(x => x.Source == input.Source.Value);
        if (input.PlayDateFrom.HasValue) query = query.Where(x => x.PlayDate >= input.PlayDateFrom.Value);
        if (input.PlayDateTo.HasValue) query = query.Where(x => x.PlayDate <= input.PlayDateTo.Value);

        var list = await AsyncExecuter.ToListAsync(query);

        var customerIds = list.Select(x => x.CustomerId).Distinct().ToList();
        var customers = customerIds.Count == 0 ? new List<Customer>() : await _customerRepository.GetListAsync(x => customerIds.Contains(x.Id));
        var customerDict = customers.ToDictionary(x => x.Id, x => x);

        var slotIds = list.Where(x => x.CalendarSlotId.HasValue && x.CalendarSlotId.Value != Guid.Empty)
            .Select(x => x.CalendarSlotId!.Value)
            .Distinct()
            .ToList();

        var slots = slotIds.Count == 0 ? new List<CalendarSlot>() : await _calendarSlotRepository.GetListAsync(s => slotIds.Contains(s.Id));
        var slotDict = slots.ToDictionary(s => s.Id, s => s);

        string LStatus(BookingStatus s)
        {
            var t = _l[$"BookingStatus:{s}"].Value;
            return (string.IsNullOrWhiteSpace(t) || t.StartsWith("BookingStatus:", StringComparison.OrdinalIgnoreCase)) ? s.ToString() : t;
        }

        string LPayment(PaymentMethod? pm)
        {
            if (!pm.HasValue) return "";
            var t = _l[$"PaymentMethod:{pm.Value}"].Value;
            return (string.IsNullOrWhiteSpace(t) || t.StartsWith("PaymentMethod:", StringComparison.OrdinalIgnoreCase)) ? pm.Value.ToString() : t;
        }

        string LSource(BookingSource src)
        {
            var t = _l[$"BookingSource:{src}"].Value;
            return (string.IsNullOrWhiteSpace(t) || t.StartsWith("BookingSource:", StringComparison.OrdinalIgnoreCase)) ? src.ToString() : t;
        }

        var rows = list.Select(b =>
        {
            customerDict.TryGetValue(b.CustomerId, out var c);

            CalendarSlot? slot = null;
            if (b.CalendarSlotId.HasValue)
                slotDict.TryGetValue(b.CalendarSlotId.Value, out slot);

            var customerDisplay = CustomerDisplayText(c);

            var from = ToHHmm(slot?.TimeFrom);
            var to = ToHHmm(slot?.TimeTo);
            var playTime = (!string.IsNullOrWhiteSpace(from) || !string.IsNullOrWhiteSpace(to))
                ? (string.IsNullOrWhiteSpace(from) ? to : (string.IsNullOrWhiteSpace(to) ? from : F("Common:TimeRange", from, to)))
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
        if (ids.Count == 0) return "";

        var query = await _optionExtendRepo.GetQueryableAsync();

        var nameDict = query
            .Where(x => x.Type == OptionExtendTypeEnum.GolfCourseUlitity.Value && ids.Contains(x.OptionId))
            .ToDictionary(x => x.OptionId, x => x.OptionName);

        var ordered = ids
            .Where(id => nameDict.ContainsKey(id))
            .Select(id => nameDict[id])
            .ToList();

        var prefix = _l["Common:BulletPrefix"].Value;
        if (string.IsNullOrWhiteSpace(prefix)) prefix = "-";

        return string.Join("\n", ordered.Select(n => $"{prefix} {n}"));
    }

    private string BuildPlayersText(List<BookingPlayer> players)
    {
        if (players == null || players.Count == 0) return "";

        string Text(string? s) => string.IsNullOrWhiteSpace(s) ? NA() : s.Trim();

        var lines = new List<string>();

        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];

            lines.Add(F("BookingEmail:PlayerBlockTitle", i + 1));
            lines.Add(F("BookingEmail:PlayerNameLine", Text(p.PlayerName)));
            lines.Add(F("BookingEmail:PlayerVgaLine", Text(p.VgaCode)));
            lines.Add(F("BookingEmail:PlayerPriceLine", MoneyText(p.PricePerPlayer)));
            lines.Add(F("BookingEmail:PlayerNotesLine", Text(p.Notes)));

            if (i < players.Count - 1) lines.Add("");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
