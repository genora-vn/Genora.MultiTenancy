using DocumentFormat.OpenXml.ExtendedProperties;
using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppPromotionPolicies;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class MiniAppBookingAppService : ApplicationService, IMiniAppBookingAppService
{
    private readonly IRepository<Booking, Guid> _bookingRepo;
    private readonly IRepository<BookingPlayer, Guid> _playerRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<CalendarSlot, Guid> _calendarSlotRepo;
    private readonly IRepository<CalendarSlotPrice, Guid> _calendarSlotPriceRepo;
    private readonly IRepository<CustomerType, Guid> _customerType;
    private readonly IAppEmailSenderService _appEmailSenderService;
    private readonly IRepository<OptionExtend, Guid> _optionExtendRepo;
    private readonly ISettingProvider _settingProvider;
    private readonly IBackgroundJobManager _jobManager;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepo;
    private readonly IRepository<Genora.MultiTenancy.DomainModels.AppPromotionTypes.PromotionType, Guid> _promotionTypeRepository;
    private readonly IRepository<PromotionPolicy, Guid> _promotionPolicyRepo;

    public MiniAppBookingAppService(
        IRepository<Booking, Guid> bookingRepo,
        IRepository<BookingPlayer, Guid> playerRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<GolfCourse, Guid> golfcourseRepo,
        IRepository<CalendarSlot, Guid> calendarSlotRepo,
        IRepository<CalendarSlotPrice, Guid> calendarSlotPriceRepo,
        IRepository<CustomerType, Guid> customerType,
        IAppEmailSenderService appEmailSenderService,
        IRepository<OptionExtend, Guid> optionExtendRepo,
        ISettingProvider settingProvider,
        IBackgroundJobManager jobManager,
        IRepository<GolfCourse, Guid> golfCourseRepo,
        IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> promotionTypeRepository,
        IRepository<PromotionPolicy, Guid> promotionPolicyRepo)
    {
        _bookingRepo = bookingRepo;
        _playerRepo = playerRepo;
        _customerRepo = customerRepo;
        _calendarSlotRepo = calendarSlotRepo;
        _calendarSlotPriceRepo = calendarSlotPriceRepo;
        _customerType = customerType;
        _appEmailSenderService = appEmailSenderService;
        _optionExtendRepo = optionExtendRepo;
        _settingProvider = settingProvider;
        _jobManager = jobManager;
        _golfCourseRepo = golfCourseRepo;
        _promotionTypeRepository = promotionTypeRepository;
        _promotionPolicyRepo = promotionPolicyRepo;
    }

    public async Task<MiniAppBookingDetailDto> CreateFromMiniAppAsync(MiniAppCreateBookingDto input)
    {
        var customer = await _customerRepo.GetAsync(input.CustomerId);
        if (customer == null)
            return new MiniAppBookingDetailDto { Error = (int)HttpStatusCode.Unauthorized, Message = "Quý khách chưa đăng nhập dịch vụ" };

        if (input.IsExportInvoice)
        {
            if (string.IsNullOrWhiteSpace(input.CompanyName))
                throw new AbpValidationException("Vui lòng nhập Tên công ty"); 

            if (string.IsNullOrWhiteSpace(input.TaxCode))
                throw new AbpValidationException("Vui lòng nhập Mã số thuế");

            if (string.IsNullOrWhiteSpace(input.CompanyAddress))
                throw new AbpValidationException("Vui lòng nhập Địa chỉ");

            if (string.IsNullOrWhiteSpace(input.InvoiceEmail))
                throw new AbpValidationException("Vui lòng nhập Email nhận hóa đơn");
        }

        var slotWithPrices = await _calendarSlotRepo.WithDetailsAsync(c => c.Prices);
        var calendarSlot = slotWithPrices.FirstOrDefault(c => c.Id == input.CalendarSlotId);
        if (calendarSlot == null)
            return new MiniAppBookingDetailDto { Error = (int)HttpStatusCode.NotFound, Message = "Không tìm thấy giờ chơi" };

        // ── Kiểm tra slot còn trống ──────────────────────────────────────────
        if (calendarSlot.SlotAvailable <= 0)
            return new MiniAppBookingDetailDto
            {
                Error   = 1,
                Message = "Rất tiếc, tee-time này đã đủ số lượng khách. Quý khách vui lòng chọn khung giờ khác."
            };

        if (calendarSlot.SlotAvailable < input.NumberOfGolfers)
            return new MiniAppBookingDetailDto
            {
                Error   = 1,
                Message = $"Khung giờ này chỉ còn {calendarSlot.SlotAvailable} chỗ trống. Quý khách vui lòng điều chỉnh số lượng người chơi."
            };

        var datePart = calendarSlot.ApplyDate.ToString("ddMMyy");

        var countInDay = await _bookingRepo.CountAsync(x => x.PlayDate.Date == calendarSlot.ApplyDate.Date);
        var serial = (countInDay + 1).ToString("D3");

        var bookingCode = $"{customer.CustomerCode}-{datePart}-{serial}";
        if (await _bookingRepo.AnyAsync(b => b.BookingCode == bookingCode))
        {
            bookingCode = $"{customer.CustomerCode}-{datePart}-{(countInDay + 2).ToString("D3")}";
        }

        var myPriceRow = calendarSlot.Prices.FirstOrDefault(x => x.CustomerTypeId == customer.CustomerTypeId);

        if (myPriceRow == null)
        {
            var visType = await _customerType.FirstOrDefaultAsync(c => c.Code == "VIS");
            if (visType != null)
                myPriceRow = calendarSlot.Prices.FirstOrDefault(x => x.CustomerTypeId == visType.Id);

            myPriceRow ??= calendarSlot.Prices.FirstOrDefault();
        }

        input.PricePerGolfer = myPriceRow != null
            ? PriceByHoleHelper.GetPriceByNumberHoles(myPriceRow, input.NumberHoles)
            : 0m;

        input.TotalAmount = input.PricePerGolfer * input.NumberOfGolfers;

        var booking = new Booking(
            GuidGenerator.Create(),
            bookingCode,
            input.CustomerId,
            input.GolfCourseId,
            input.CalendarSlotId,
            calendarSlot.ApplyDate,
            input.NumberOfGolfers,
            input.PricePerGolfer,
            input.TotalAmount,
            input.PaymentMethod,
            BookingStatus.Processing,
            input.Source
        );

        booking.Utility = (input.Utilities != null && input.Utilities.Count > 0) ? string.Join(",", input.Utilities) : string.Empty;
        booking.NumberHole = input.NumberHoles;
        booking.IsExportInvoice = input.IsExportInvoice;

        if (input.IsExportInvoice)
        {
            booking.CompanyName = input.CompanyName?.Trim();
            booking.TaxCode = input.TaxCode?.Trim();
            booking.CompanyAddress = input.CompanyAddress?.Trim();
            booking.InvoiceEmail = input.InvoiceEmail?.Trim();
        }
        else
        {
            booking.CompanyName = null;
            booking.TaxCode = null;
            booking.CompanyAddress = null;
            booking.InvoiceEmail = null;
        }

        await _bookingRepo.InsertAsync(booking, autoSave: true);

        // ── Giảm SlotAvailable theo số golfer vừa đặt ──────────────────────
        calendarSlot.SlotAvailable = Math.Max(0, calendarSlot.SlotAvailable - input.NumberOfGolfers);
        await _calendarSlotRepo.UpdateAsync(calendarSlot, autoSave: true);

        if (input.Players != null && input.Players.Any())
        {
            foreach (var p in input.Players)
            {
                var player = new BookingPlayer(
                    GuidGenerator.Create(),
                    booking.Id,
                    p.CustomerId,
                    p.PlayerName,
                    p.PricePerGolfer,
                    p.VgaCode,
                    p.Notes
                );

                player.VgaCode = p.VgaCode;
                player.PricePerPlayer = p.PricePerGolfer;

                await _playerRepo.InsertAsync(player, autoSave: true);
            }
        }

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
                        TrackingId = booking.Id.ToString(),
                        TemplateData = new
                        {
                            customer_name = customer.FullName,
                            booking_id = booking.BookingCode,
                            tee_off_date = booking.PlayDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                            tee_off_time = $"{calendarSlot.TimeFrom:hh\\:mm}",
                            number_of_player = booking.NumberOfGolfers
                        }
                    },
                    priority: BackgroundJobPriority.Normal
                );
            }
        }
        catch
        {
            // không throw để không ảnh hưởng luồng create booking
        }

        var ct = await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId);
        var ctName = ct?.Name ?? ct?.Code ?? "N/A";
        var customerTypeSummary = $"{ctName}";

        try
        {
            static string ToHHmm(TimeSpan? ts) => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : "";
            static string ToDDMMYYYY(DateTime dt) => dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var otherRequestsText = await BuildOtherRequestsTextAsync(booking.Utility);

            var golfInfo = await GetGolfCourseInfoAsync(booking.GolfCourseId);
            var promotionText = await GetPromotionNameAsync(booking.CalendarSlotId);
            var savedPlayers = await _playerRepo.GetListAsync(x => x.BookingId == booking.Id);
            var playersText = BuildPlayersInlineText(savedPlayers, customer.FullName);

            var priceBreakdownItems = await BuildPriceBreakdownItemsAsync(
                customer,
                booking.CalendarSlotId,
                booking.NumberHole,
                booking.NumberOfGolfers,
                booking.GolfCourseId
            );

            var emailTotalAmount = SumPriceBreakdownItems(priceBreakdownItems);
            if (emailTotalAmount <= 0m)
            {
                emailTotalAmount = booking.TotalAmount;
            }

            var model = new BookingNewRequestEmailModelDto
            {
                BookingCode = booking.BookingCode,
                BookerName = string.IsNullOrWhiteSpace(customer.FullName) ? "N/A" : customer.FullName.Trim(),
                BookerPhone = string.IsNullOrWhiteSpace(customer.PhoneNumber) ? "N/A" : customer.PhoneNumber.Trim(),

                GolfCourseName = golfInfo.Name,
                GolfCourseHotline = golfInfo.Phone,
                GolfCourseAddress = golfInfo.Address,

                PlayDate = booking.PlayDate,
                PlayDateText = ToDDMMYYYY(booking.PlayDate),

                TeeTimeFromText = ToHHmm(calendarSlot?.TimeFrom),
                TeeTimeToText = ToHHmm(calendarSlot?.TimeTo),
                TeeTime = $"{ToHHmm(calendarSlot?.TimeFrom)} - {ToHHmm(calendarSlot?.TimeTo)}",

                NumberOfGolfers = booking.NumberOfGolfers,
                CustomerTypeSummary = customerTypeSummary,
                PlayersText = playersText,
                PromotionText = promotionText,

                PricePerGolfer = booking.PricePerGolfer ?? 0m,
                PricePerGolferText = $"{(booking.PricePerGolfer ?? 0m):N0}",

                HasPriceBreakdownItems = priceBreakdownItems.Any(),
                PriceBreakdownItems = priceBreakdownItems,

                TotalAmount = emailTotalAmount,
                TotalAmountText = $"{emailTotalAmount:N0}",

                PaymentMethod = booking.PaymentMethod.ToString(),
                OtherRequests = otherRequestsText,

                IsExportInvoice = booking.IsExportInvoice,
                CompanyName = booking.CompanyName,
                TaxCode = booking.TaxCode,
                CompanyAddress = booking.CompanyAddress,
                InvoiceEmail = booking.InvoiceEmail
            };

            var toEmails = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_ToEmails);
            var ccEmails = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_CcEmails);
            var bccEmails = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_BccEmails);
            var subjectTpl = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingNew_SubjectTemplate);

            var finalToEmails = EmailHelper.NormalizeEmailList(toEmails);
            if (string.IsNullOrWhiteSpace(finalToEmails))
            {
                finalToEmails = EmailHelper.NormalizeEmailList("tandv@baygolf.vn");
            }

            var subject = ApplyTemplate(subjectTpl, booking.BookingCode);
            if (string.IsNullOrWhiteSpace(subject))
            {
                subject = $"[ZALO MINI APP] YÊU CẦU ĐẶT CHỖ MỚI – {booking.BookingCode}";
            }

            await _appEmailSenderService.EnqueueTemplateAsync(
                templateName: AppEmailTemplateNames.BookingNewRequest,
                model: model,
                toEmails: finalToEmails,
                subject: subject,
                cc: NullIfEmpty(EmailHelper.NormalizeEmailList(ccEmails)),
                bcc: NullIfEmpty(EmailHelper.NormalizeEmailList(bccEmails)),
                bookingId: booking.Id,
                bookingCode: booking.BookingCode
            );

            Logger.LogInformation(
                "[BookingEmail] Enqueued booking created email successfully. BookingId={BookingId}, BookingCode={BookingCode}, TenantId={TenantId}, To={To}",
                booking.Id,
                booking.BookingCode,
                CurrentTenant.Id,
                finalToEmails
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "[BookingEmail] Failed to enqueue booking created email. BookingId={BookingId}, BookingCode={BookingCode}, TenantId={TenantId}",
                booking.Id,
                booking.BookingCode,
                CurrentTenant.Id
            );
        }

        var dto = ObjectMapper.Map<Booking, BookingDetailData>(booking);
        dto.NumberHoles = booking.NumberHole;
        dto.Utilities = input.Utilities;
        dto.FrameTimes = $"{calendarSlot.TimeFrom} - {calendarSlot.TimeTo}";
        var players = await _playerRepo.GetListAsync(x => x.BookingId == booking.Id);
        dto.Players = ObjectMapper.Map<List<BookingPlayer>, List<AppBookingPlayerDto>>(players);

        return new MiniAppBookingDetailDto { Error = 0, Message = "Success", Data = dto };
    }

    public async Task<MiniAppBookingDetailDto> UpdateFromMiniAppAsync(Guid id, MiniAppUpdateBookingDto input)
    {
        try
        {
            if (input.CustomerId == Guid.Empty)
                throw new MemberAccessException("Vui lòng đăng nhập trước khi truy cập");

            var booking = await _bookingRepo.FindAsync(x => x.Id == id);
            if (booking == null)
                throw new EntityNotFoundException(typeof(Booking), id);

            if (booking.CustomerId != input.CustomerId)
                throw new MemberAccessException("Bạn không có quyền cập nhật booking này");

            if (booking.Status == BookingStatus.CancelledRefund || booking.Status == BookingStatus.CancelledNoRefund)
                throw new AbpValidationException("Booking đã hủy, không thể cập nhật");

            var customer = await _customerRepo.GetAsync(input.CustomerId);
            if (customer == null)
                throw new MemberAccessException("Quý khách chưa đăng nhập dịch vụ");

            if (input.IsExportInvoice)
            {
                if (string.IsNullOrWhiteSpace(input.CompanyName))
                    throw new AbpValidationException("Vui lòng nhập Tên công ty");

                if (string.IsNullOrWhiteSpace(input.TaxCode))
                    throw new AbpValidationException("Vui lòng nhập Mã số thuế");

                if (string.IsNullOrWhiteSpace(input.CompanyAddress))
                    throw new AbpValidationException("Vui lòng nhập Địa chỉ");

                if (string.IsNullOrWhiteSpace(input.InvoiceEmail))
                    throw new AbpValidationException("Vui lòng nhập Email nhận hóa đơn");
            }

            var oldPlayers = await _playerRepo.GetListAsync(p => p.BookingId == id);

            CalendarSlot? oldSlot = null;
            if (booking.CalendarSlotId.HasValue && booking.CalendarSlotId.Value != Guid.Empty)
                oldSlot = await _calendarSlotRepo.FindAsync(booking.CalendarSlotId.Value);

            var oldPlayDateText = booking.PlayDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var oldTeeFromText = oldSlot?.TimeFrom.ToString(@"hh\:mm") ?? "";
            var oldTeeToText = oldSlot?.TimeTo.ToString(@"hh\:mm") ?? "";

            var oldStatusText = booking.Status.ToString();
            var oldPaymentMethodText = booking.PaymentMethod?.ToString() ?? "N/A";
            var oldNumberOfGolfers = booking.NumberOfGolfers;

            var oldCustomerType = customer.CustomerTypeId.HasValue
                ? await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId.Value)
                : null;
            var oldCustomerTypeText = oldCustomerType?.Name ?? oldCustomerType?.Code ?? "N/A";

            var oldPromotionText = await GetPromotionNameAsync(booking.CalendarSlotId);

            var slotWithPrices = await _calendarSlotRepo.WithDetailsAsync(c => c.Prices);
            var newCalendarSlot = slotWithPrices.FirstOrDefault(c => c.Id == input.CalendarSlotId);
            if (newCalendarSlot == null)
            {
                return new MiniAppBookingDetailDto
                {
                    Error = (int)HttpStatusCode.NotFound,
                    Message = "Không tìm thấy giờ chơi"
                };
            }

            // ── Kiểm tra và cập nhật SlotAvailable khi đổi slot hoặc đổi số golfer ─
            var oldSlotId = booking.CalendarSlotId;
            var oldGolfers = booking.NumberOfGolfers;
            bool isSlotChanged = oldSlotId != input.CalendarSlotId;

            if (isSlotChanged)
            {
                // Đổi sang slot mới — kiểm tra slot mới còn đủ chỗ
                if (newCalendarSlot.SlotAvailable <= 0)
                    return new MiniAppBookingDetailDto
                    {
                        Error   = 1,
                        Message = "Rất tiếc, tee-time này đã đủ số lượng khách. Quý khách vui lòng chọn khung giờ khác."
                    };

                if (newCalendarSlot.SlotAvailable < input.NumberOfGolfers)
                    return new MiniAppBookingDetailDto
                    {
                        Error   = 1,
                        Message = $"Khung giờ này chỉ còn {newCalendarSlot.SlotAvailable} chỗ trống. Quý khách vui lòng điều chỉnh số lượng người chơi."
                    };

                // Hoàn slot cũ
                if (oldSlotId.HasValue)
                {
                    var previousSlot = await _calendarSlotRepo.FindAsync(oldSlotId.Value);
                    if (previousSlot != null)
                    {
                        previousSlot.SlotAvailable = Math.Min(previousSlot.MaxSlots, previousSlot.SlotAvailable + oldGolfers);
                        await _calendarSlotRepo.UpdateAsync(previousSlot, autoSave: true);
                    }
                }

                // Trừ slot mới
                newCalendarSlot.SlotAvailable = Math.Max(0, newCalendarSlot.SlotAvailable - input.NumberOfGolfers);
                await _calendarSlotRepo.UpdateAsync(newCalendarSlot, autoSave: true);
            }
            else if (input.NumberOfGolfers != oldGolfers)
            {
                // Cùng slot nhưng đổi số golfer — điều chỉnh chênh lệch
                var diff = input.NumberOfGolfers - oldGolfers;
                if (diff > 0 && newCalendarSlot.SlotAvailable < diff)
                    return new MiniAppBookingDetailDto
                    {
                        Error   = 1,
                        Message = $"Khung giờ này chỉ còn {newCalendarSlot.SlotAvailable} chỗ trống. Quý khách vui lòng điều chỉnh số lượng người chơi."
                    };

                newCalendarSlot.SlotAvailable = Math.Clamp(
                    newCalendarSlot.SlotAvailable - diff,
                    0, newCalendarSlot.MaxSlots);
                await _calendarSlotRepo.UpdateAsync(newCalendarSlot, autoSave: true);
            }

            var myPriceRow = newCalendarSlot.Prices.FirstOrDefault(x => x.CustomerTypeId == customer.CustomerTypeId);

            if (myPriceRow == null)
            {
                var visType = await _customerType.FirstOrDefaultAsync(c => c.Code == "VIS");
                if (visType != null)
                    myPriceRow = newCalendarSlot.Prices.FirstOrDefault(x => x.CustomerTypeId == visType.Id);

                myPriceRow ??= newCalendarSlot.Prices.FirstOrDefault();
            }

            var recalculatedPricePerGolfer = myPriceRow != null
                ? PriceByHoleHelper.GetPriceByNumberHoles(myPriceRow, input.NumberHoles)
                : 0m;

            booking.CalendarSlotId = input.CalendarSlotId;
            booking.GolfCourseId = newCalendarSlot.GolfCourseId;
            booking.PlayDate = newCalendarSlot.ApplyDate;
            booking.NumberOfGolfers = input.NumberOfGolfers;
            booking.NumberHole = input.NumberHoles;
            booking.PricePerGolfer = recalculatedPricePerGolfer;
            booking.TotalAmount = recalculatedPricePerGolfer * input.NumberOfGolfers;
            booking.Utility = (input.Utilities != null && input.Utilities.Count > 0)
                ? string.Join(",", input.Utilities)
                : string.Empty;

            booking.IsExportInvoice = input.IsExportInvoice;
            if (input.IsExportInvoice)
            {
                booking.CompanyName = input.CompanyName?.Trim();
                booking.TaxCode = input.TaxCode?.Trim();
                booking.CompanyAddress = input.CompanyAddress?.Trim();
                booking.InvoiceEmail = input.InvoiceEmail?.Trim();
            }
            else
            {
                booking.CompanyName = null;
                booking.TaxCode = null;
                booking.CompanyAddress = null;
                booking.InvoiceEmail = null;
            }

            await _bookingRepo.UpdateAsync(booking, autoSave: true);

            await ReplacePlayersAsync(booking.Id, input.Players, booking.PricePerGolfer);

            await CurrentUnitOfWork.SaveChangesAsync();

            var newPlayers = await _playerRepo.GetListAsync(p => p.BookingId == id);

            var newPlayDateText = booking.PlayDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var newTeeFromText = newCalendarSlot.TimeFrom.ToString(@"hh\:mm");
            var newTeeToText = newCalendarSlot.TimeTo.ToString(@"hh\:mm");

            var newStatusText = booking.Status.ToString();
            var newPaymentMethodText = booking.PaymentMethod?.ToString() ?? "N/A";
            var newNumberOfGolfers = booking.NumberOfGolfers;

            var newCustomerType = customer.CustomerTypeId.HasValue
                ? await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId.Value)
                : null;
            var newCustomerTypeText = newCustomerType?.Name ?? newCustomerType?.Code ?? "N/A";

            var newPromotionText = await GetPromotionNameAsync(booking.CalendarSlotId);

            static string PlayersSig(IEnumerable<BookingPlayer> ps) =>
                string.Join("|",
                    ps.Select(p =>
                        $"{(p.PlayerName ?? "").Trim()}#{(p.VgaCode ?? "").Trim()}#{(p.PricePerPlayer ?? 0m):0.##}"
                    ).OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                );

            var hasPlayerChanges =
                !string.Equals(PlayersSig(oldPlayers), PlayersSig(newPlayers), StringComparison.OrdinalIgnoreCase);

            var hasHeaderChanges =
                oldPlayDateText != newPlayDateText
                || oldTeeFromText != newTeeFromText
                || oldTeeToText != newTeeToText
                || oldNumberOfGolfers != newNumberOfGolfers;

            try
            {
                if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
                {
                    await _jobManager.EnqueueAsync(
                        new ZbsSendJobArgs
                        {
                            TenantId = CurrentTenant.Id,
                            TemplateKey = "BookingChanged",
                            Phone = customer.PhoneNumber,
                            TrackingId = booking.Id.ToString(),
                            TemplateData = new
                            {
                                customer_name = customer.FullName,
                                booking_code = booking.BookingCode,
                                tee_off_date = newPlayDateText,
                                tee_off_time = newTeeFromText,
                                number_of_player = booking.NumberOfGolfers
                            }
                        },
                        priority: BackgroundJobPriority.Normal
                    );
                }
            }
            catch
            {
                // không throw để không ảnh hưởng update booking
            }

            try
            {
                var golfInfo = await GetGolfCourseInfoAsync(booking.GolfCourseId);
                var oldPlayersInline = BuildPlayersInlineText(oldPlayers, customer.FullName);
                var newPlayersInline = BuildPlayersInlineText(newPlayers, customer.FullName);
                var otherRequestsText = await BuildOtherRequestsTextAsync(booking.Utility);
                var invoiceInfoText = BuildInvoiceInfoText(booking);
                var updatedByText = BuildUpdatedByText(customer);

                var priceBreakdownItems = await BuildPriceBreakdownItemsAsync(
                    customer,
                    booking.CalendarSlotId,
                    booking.NumberHole,
                    booking.NumberOfGolfers,
                    booking.GolfCourseId
                );

                var emailTotalAmount = SumPriceBreakdownItems(priceBreakdownItems);
                if (emailTotalAmount <= 0m)
                {
                    emailTotalAmount = booking.TotalAmount;
                }

                var changeModel = new BookingChangeRequestEmailModelDto
                {
                    BookingCode = booking.BookingCode,
                    BookerName = string.IsNullOrWhiteSpace(customer.FullName) ? "N/A" : customer.FullName.Trim(),
                    BookerPhone = string.IsNullOrWhiteSpace(customer.PhoneNumber) ? "N/A" : customer.PhoneNumber.Trim(),

                    GolfCourseName = golfInfo.Name,
                    GolfCourseHotline = golfInfo.Phone,
                    GolfCourseAddress = golfInfo.Address,

                    OldStatusText = oldStatusText,
                    OldPaymentMethodText = oldPaymentMethodText,
                    OldNumberOfGolfers = oldNumberOfGolfers,
                    OldPlayDateText = oldPlayDateText,
                    OldTeeTimeFromText = oldTeeFromText,
                    OldTeeTimeToText = oldTeeToText,
                    OldCustomerTypeText = oldCustomerTypeText,
                    OldPromotionText = oldPromotionText,
                    OldPlayersText = oldPlayersInline,
                    OldUpdatedByText = updatedByText,

                    NewStatusText = newStatusText,
                    NewPaymentMethodText = newPaymentMethodText,
                    NewNumberOfGolfers = newNumberOfGolfers,
                    NewPlayDateText = newPlayDateText,
                    NewTeeTimeFromText = newTeeFromText,
                    NewTeeTimeToText = newTeeToText,
                    NewCustomerTypeText = newCustomerTypeText,
                    NewPromotionText = newPromotionText,
                    NewPlayersText = newPlayersInline,
                    NewUpdatedByText = updatedByText,

                    PricePerGolferText = MoneyText(booking.PricePerGolfer),

                    HasPriceBreakdownItems = priceBreakdownItems.Any(),
                    PriceBreakdownItems = priceBreakdownItems,

                    TotalAmountText = MoneyText(emailTotalAmount),

                    OtherRequestsText = otherRequestsText,
                    InvoiceInfoText = invoiceInfoText,

                    HasPlayerChanges = hasPlayerChanges,
                    HasHeaderChanges = hasHeaderChanges
                };

                var toEmails = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingChange_ToEmails);
                var ccEmails = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingChange_CcEmails);
                var bccEmails = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingChange_BccEmails);
                var subjectTpl = await _settingProvider.GetOrNullAsync(AppEmailSettingNames.BookingChange_SubjectTemplate);

                var finalToEmails = EmailHelper.NormalizeEmailList(toEmails);
                if (string.IsNullOrWhiteSpace(finalToEmails))
                {
                    finalToEmails = EmailHelper.NormalizeEmailList("tandv@baygolf.vn");
                }

                var subject = ApplyTemplate(
                    subjectTpl,
                    booking.BookingCode,
                    "[ZALO MINI APP] YÊU CẦU THAY ĐỔI ĐẶT CHỖ – {BookingCode}"
                );

                await _appEmailSenderService.EnqueueTemplateAsync(
                    templateName: AppEmailTemplateNames.BookingChangeRequest,
                    model: changeModel,
                    toEmails: finalToEmails,
                    subject: subject,
                    cc: NullIfEmpty(EmailHelper.NormalizeEmailList(ccEmails)),
                    bcc: NullIfEmpty(EmailHelper.NormalizeEmailList(bccEmails)),
                    bookingId: booking.Id,
                    bookingCode: booking.BookingCode
                );

                Logger.LogInformation(
                    "[BookingEmail] Enqueued booking change email successfully. BookingId={BookingId}, BookingCode={BookingCode}, TenantId={TenantId}, To={To}",
                    booking.Id,
                    booking.BookingCode,
                    CurrentTenant.Id,
                    finalToEmails
                );
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "[BookingEmail] Failed to enqueue booking change email. BookingId={BookingId}, BookingCode={BookingCode}, TenantId={TenantId}",
                    booking.Id,
                    booking.BookingCode,
                    CurrentTenant.Id
                );
            }

            return await GetMiniAppAsync(booking.Id, input.CustomerId);
        }
        catch (Exception e)
        {
            return new MiniAppBookingDetailDto
            {
                Error = (int)HttpStatusCode.BadRequest,
                Message = e.Message
            };
        }
    }

    [DisableValidation]
    public async Task<MiniAppBookingListDto> GetListMiniAppAsync(GetMiniAppBookingListInput input)
    {
        try
        {
            if (input.CustomerId == Guid.Empty)
                throw new MemberAccessException("Vui lòng đăng nhập trước khi truy cập");

            var query = await _bookingRepo.WithDetailsAsync(x => x.CalendarSlot);
            query = query.Where(x => x.CustomerId == input.CustomerId);

            if (input.PlayDateFrom.HasValue)
            {
                query = query.Where(x =>
                    (x.PlayDate > input.PlayDateFrom.Value.Date)
                    || (x.CalendarSlotId.HasValue && x.PlayDate.Date == input.PlayDateFrom.Value.Date && x.CalendarSlot.TimeFrom > input.PlayDateFrom.Value.TimeOfDay));
            }

            if (input.PlayDateTo.HasValue)
            {
                query = query.Where(x =>
                    (x.PlayDate < input.PlayDateTo.Value.Date)
                    || (x.CalendarSlotId.HasValue && x.PlayDate.Date == input.PlayDateTo.Value.Date && x.CalendarSlot.TimeFrom < input.PlayDateTo.Value.TimeOfDay));
            }

            if (input.Status.HasValue)
                query = query.Where(x => x.Status == input.Status.Value);

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(Booking.CreationTime) + " desc"
                : input.Sorting;

            query = query.OrderBy(sorting);

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            var dto = ObjectMapper.Map<List<Booking>, List<BookingListData>>(items);

            var calendarSlotIds = items
                .Where(x => x.CalendarSlotId.HasValue && x.CalendarSlotId.Value != Guid.Empty)
                .Select(x => x.CalendarSlotId!.Value)
                .Distinct()
                .ToList();

            var golfCourseIds = items
                .Select(x => x.GolfCourseId)
                .Distinct()
                .ToList();

            var calendars = await _calendarSlotRepo.GetListAsync(x => calendarSlotIds.Contains(x.Id));
            var calendarDict = calendars.ToDictionary(x => x.Id, x => x);

            var golfCourses = await _golfCourseRepo.GetListAsync(x => golfCourseIds.Contains(x.Id));
            var golfCourseDict = golfCourses.ToDictionary(x => x.Id, x => x);

            // Lookup AppPromotionPolicies cho các (GolfCourseId, PromotionTypeId) xuất hiện trên list
            var promotionTypeIdSet = calendars
                .Select(x => x.PromotionTypeId)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var policyList = (golfCourseIds.Count > 0 && promotionTypeIdSet.Count > 0)
                ? await _promotionPolicyRepo.GetListAsync(p =>
                    golfCourseIds.Contains(p.GolfCourseId) &&
                    promotionTypeIdSet.Contains(p.PromotionTypeId))
                : new List<PromotionPolicy>();

            var policyDict = policyList
                .GroupBy(p => (p.GolfCourseId, p.PromotionTypeId))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var item in dto)
            {
                item.VNDayOfWeek = FormatDateTimeHelper.GetVietnameseDayOfWeek(item.PlayDate);
                item.IsCancellationPolicy = false;

                CalendarSlot? calendar = null;
                if (item.CalendarSlotId.HasValue && item.CalendarSlotId.Value != Guid.Empty)
                {
                    calendarDict.TryGetValue(item.CalendarSlotId.Value, out calendar);
                    if (calendar != null)
                    {
                        item.FrameTimes = $"{calendar.TimeFrom} - {calendar.TimeTo}";
                    }
                }

                PromotionPolicy? policy = null;
                if (calendar != null && calendar.PromotionTypeId != Guid.Empty)
                {
                    policyDict.TryGetValue((item.GolfCourseId, calendar.PromotionTypeId), out policy);
                }

                var playDateTime = item.PlayDate.Date + (calendar?.TimeFrom ?? TimeSpan.Zero);
                item.IsCancellationPolicy = EvaluateCancellationPolicy(playDateTime, policy);
            }

            var result = new PagedResultDto<BookingListData>(total, dto);
            return new MiniAppBookingListDto { Data = result, Error = 0, Message = "Success" };
        }
        catch (Exception e)
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
            dto.MaxSlots = booking.NumberOfGolfers;

            var players = await _playerRepo.GetListAsync(x => x.BookingId == id);
            dto.Players = ObjectMapper.Map<List<BookingPlayer>, List<AppBookingPlayerDto>>(players);

            dto.Utilities = string.IsNullOrEmpty(booking.Utility)
                ? new List<int>()
                : booking.Utility.Split(",").Select(int.Parse).ToList();

            dto.NumberHoles = booking.NumberHole;

            // TotalAmount = tổng giá thực tế từng người chơi trong AppBookingPlayers
            dto.TotalAmount = players.Sum(p => p.PricePerPlayer ?? 0m);

            // ===== CustomerType + GolfCourse member config =====
            var customer = await _customerRepo.FindAsync(booking.CustomerId);
            var currentCt = (customer?.CustomerTypeId.HasValue == true)
                ? await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId.Value)
                : null;

            dto.CustomerTypeCode = currentCt?.Code;

            // OriginalTotalAmount = CustomerType.OriginalPrice * numberOfGolfers
            var originalUnitPrice = (currentCt?.OriginalPrice.HasValue == true && currentCt.OriginalPrice.Value > 0)
                ? currentCt.OriginalPrice.Value
                : 0m;
            dto.OriginalTotalAmount = originalUnitPrice * booking.NumberOfGolfers;

            var golfCourse = await _golfCourseRepo.FindAsync(booking.GolfCourseId);
            dto.IsMemberSupported = golfCourse?.IsMemberSupported ?? false;
            // Calculate actual MaxMemberGuest: how many guests get MemberGuest price in this booking
            if (golfCourse?.IsMemberSupported == true && currentCt?.Code == "MB")
            {
                dto.MaxMemberGuest = Math.Min(booking.NumberOfGolfers - 1, golfCourse.MaxMemberGuest ?? 0);
            }
            else
            {
                dto.MaxMemberGuest = null;
            }

            if (dto.CalendarSlotId.HasValue && dto.CalendarSlotId.Value != Guid.Empty)
            {
                var calendar = await _calendarSlotRepo.FirstOrDefaultAsync(x => x.Id == dto.CalendarSlotId.Value);
                if (calendar != null)
                {
                    dto.FrameTimes = $"{calendar.TimeFrom} - {calendar.TimeTo}";
                }

                // Load tất cả customer types cần dùng trong 1 lần
                var allCtCodes = new[] { "VIS", "MBG", "MB" };
                var relevantCts = await _customerType.GetListAsync(c => allCtCodes.Contains(c.Code));
                var visCtx  = relevantCts.FirstOrDefault(c => c.Code == "VIS");
                var mbgCtx  = relevantCts.FirstOrDefault(c => c.Code == "MBG");
                var mbCtx   = relevantCts.FirstOrDefault(c => c.Code == "MB");

                // Load tất cả slot prices của booking này trong 1 lần
                var slotPrices = await _calendarSlotPriceRepo.GetListAsync(
                    x => x.CalendarSlotId == booking.CalendarSlotId
                );

                // VisitorPrice = giá VIS từ AppCalendarSlotPrices theo số hố
                if (visCtx != null)
                {
                    var visRow = slotPrices.FirstOrDefault(x => x.CustomerTypeId == visCtx.Id);
                    dto.VisitorPrice = visRow != null
                        ? PriceByHoleHelper.GetPriceByNumberHoles(visRow, booking.NumberHole)
                        : 0m;
                }

                // MemberGuestPrice = giá MBG, chỉ khi sân hỗ trợ Member và KH là MB
                decimal mbSlotPrice = 0m;
                if (golfCourse?.IsMemberSupported == true && currentCt?.Code == "MB")
                {
                    // Giá MB từ slot
                    if (mbCtx != null)
                    {
                        var mbRow = slotPrices.FirstOrDefault(x => x.CustomerTypeId == mbCtx.Id);
                        mbSlotPrice = mbRow != null
                            ? PriceByHoleHelper.GetPriceByNumberHoles(mbRow, booking.NumberHole)
                            : 0m;
                    }

                    if (mbgCtx != null)
                    {
                        var mbgRow = slotPrices.FirstOrDefault(x => x.CustomerTypeId == mbgCtx.Id);
                        if (mbgRow != null)
                        {
                            var mbgPrice = PriceByHoleHelper.GetPriceByNumberHoles(mbgRow, booking.NumberHole);
                            dto.MemberGuestPrice = mbgPrice > 0 ? mbgPrice : null;
                        }
                    }
                }

                // ===== Tính customerBillTotalPrice / originalBillTotalPrice / discountTotalPrice =====
                int numGolfers = booking.NumberOfGolfers;
                int maxMbg     = golfCourse?.IsMemberSupported == true ? (golfCourse.MaxMemberGuest ?? 0) : 0;

                if (golfCourse?.IsMemberSupported == true && currentCt?.Code == "MB")
                {
                    decimal mbgSlotPrice = dto.MemberGuestPrice ?? 0m;
                    int visitorSlots     = Math.Max(0, numGolfers - maxMbg - 1);

                    dto.CustomerBillTotalPrice = mbSlotPrice
                        + (mbgSlotPrice * Math.Min(maxMbg, numGolfers - 1))
                        + (visitorSlots * dto.VisitorPrice);

                    decimal mbOriginal  = mbCtx?.OriginalPrice  ?? 0m;
                    decimal mbgOriginal = mbgCtx?.OriginalPrice ?? 0m;
                    decimal visOriginal = visCtx?.OriginalPrice ?? 0m;

                    dto.OriginalBillTotalPrice = mbOriginal
                        + (mbgOriginal * Math.Min(maxMbg, numGolfers - 1))
                        + (visitorSlots * visOriginal);
                }
                else
                {
                    dto.CustomerBillTotalPrice = dto.VisitorPrice * numGolfers;

                    decimal visOriginal         = visCtx?.OriginalPrice ?? 0m;
                    dto.OriginalBillTotalPrice  = visOriginal * numGolfers;
                }

                dto.DiscountTotalPrice = Math.Max(0m, dto.OriginalBillTotalPrice - dto.CustomerBillTotalPrice);
            }

            return new MiniAppBookingDetailDto { Data = dto, Error = 0, Message = "Success" };
        }
        catch (Exception e)
        {
            return new MiniAppBookingDetailDto { Error = (int)HttpStatusCode.BadRequest, Message = e.Message };
        }
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

    private async Task<string> BuildOtherRequestsTextAsync(string? utilityCsv)
    {
        if (string.IsNullOrWhiteSpace(utilityCsv))
            return string.Empty;

        var ids = utilityCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(x => int.TryParse(x, out var n) ? (int?)n : null)
                            .Where(x => x.HasValue)
                            .Select(x => x!.Value)
                            .Distinct()
                            .ToList();

        if (ids.Count == 0)
            return string.Empty;

        var query = await _optionExtendRepo.GetQueryableAsync();

        var utilities = query
            .Where(x => ids.Contains(x.OptionId))
            .Select(x => new { x.OptionId, x.OptionName })
            .ToList();

        if (utilities.Count == 0)
            return string.Empty;

        var dict = utilities.ToDictionary(x => x.OptionId, x => x.OptionName);

        var lines = ids
            .Where(id => dict.ContainsKey(id))
            .Select(id => $"• {dict[id]}");

        return string.Join(Environment.NewLine, lines);
    }

    private bool EvaluateCancellationPolicy(
        DateTime playDateTime,
        PromotionPolicy? policy)
    {
        // Không có policy cấu hình → cho phép hoãn hủy thoải mái
        if (policy == null) return false;

        // Chọn giờ theo ngày chơi: T7/CN dùng cấu hình cuối tuần, T2-T6 dùng cấu hình trong tuần
        var dow = playDateTime.DayOfWeek;
        var isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;

        var hours = isWeekend
            ? policy.CancellationPolicyHoursWeekend
            : policy.CancellationPolicyHours;

        // Hours null hoặc <= 0 → unlimited window → luôn được hoãn hủy
        if (!hours.HasValue || hours.Value <= 0) return false;

        // Hours > 0: so sánh khoảng cách từ NOW đến giờ chơi (PlayDate + slot.TimeFrom).
        //   - remaining >= hours  → còn đủ thời gian theo policy → cho phép hủy (false)
        //   - remaining <  hours  → không còn đủ → không được hủy (true)
        var remaining = playDateTime - Clock.Now;
        return remaining < TimeSpan.FromHours(hours.Value);
    }

    private bool EvaluateCancellationPolicy(
        DateTime creationTime,
        int? cancellationPolicyHours,
        string? promotionTypeIdsCsv,
        Guid? slotPromotionTypeId)
    {
        // Legacy fallback (giữ tạm để các caller cũ không break) — không còn dùng cho list mini app.
        var isExpiredByHours =
            cancellationPolicyHours.HasValue &&
            cancellationPolicyHours.Value > 0 &&
            creationTime.AddHours(cancellationPolicyHours.Value) <= Clock.Now;

        var blockedPromotionIds = ParseGuidCsv(promotionTypeIdsCsv);
        var isBlockedByPromotion =
            slotPromotionTypeId.HasValue &&
            blockedPromotionIds.Contains(slotPromotionTypeId.Value);

        return isExpiredByHours || isBlockedByPromotion;
    }

    private static HashSet<Guid> ParseGuidCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return new HashSet<Guid>();

        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .ToHashSet();
    }

    private async Task<string> GetPromotionNameAsync(Guid? calendarSlotId)
    {
        if (!calendarSlotId.HasValue || calendarSlotId.Value == Guid.Empty)
            return "";

        var slot = await _calendarSlotRepo.FindAsync(calendarSlotId.Value);
        if (slot == null || slot.PromotionTypeId == Guid.Empty)
            return "";

        var promo = await _promotionTypeRepository.FindAsync(slot.PromotionTypeId);
        return promo?.Name ?? "";
    }

    private async Task<(string Name, string Phone, string Address)> GetGolfCourseInfoAsync(Guid golfCourseId)
    {
        var golfCourse = await _golfCourseRepo.FindAsync(golfCourseId);
        return (
            Name: golfCourse?.Name ?? "",
            Phone: golfCourse?.Phone ?? "",
            Address: golfCourse?.Address ?? ""
        );
    }

    private string BuildPlayersInlineText(List<BookingPlayer> players, string? bookerName)
    {
        if (players == null || players.Count == 0) return "";

        var booker = (bookerName ?? "").Trim();

        var names = players
            .Select(x => (x.PlayerName ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => !string.Equals(x, booker, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return names.Count == 0 ? "" : string.Join(", ", names);
    }

    private async Task ReplacePlayersAsync(Guid bookingId, List<CreateUpdateBookingPlayerDto>? players, decimal? pricePerGolfer)
    {
        await _playerRepo.DeleteAsync(p => p.BookingId == bookingId);

        if (players == null || !players.Any())
            return;

        foreach (var p in players)
        {
            var player = new BookingPlayer(
                GuidGenerator.Create(),
                bookingId,
                p.CustomerId,
                p.PlayerName,
                pricePerGolfer,
                p.VgaCode,
                p.Notes
            );

            player.VgaCode = p.VgaCode;
            player.PricePerPlayer = pricePerGolfer;

            await _playerRepo.InsertAsync(player, autoSave: true);
        }
    }

    private static string ApplyTemplate(string? template, string bookingCode, string defaultTemplate)
    {
        template ??= defaultTemplate;
        return template.Replace("{BookingCode}", bookingCode ?? "");
    }

    private static string MoneyText(decimal? value)
    {
        return value.HasValue ? $"{value.Value:N0}" : "0";
    }

    private string BuildInvoiceInfoText(Booking booking)
    {
        if (!booking.IsExportInvoice)
            return "Không yêu cầu";

        var lines = new List<string>
    {
        $"Tên công ty: {booking.CompanyName ?? ""}",
        $"Mã số thuế: {booking.TaxCode ?? ""}",
        $"Địa chỉ: {booking.CompanyAddress ?? ""}",
        $"Email nhận hóa đơn: {booking.InvoiceEmail ?? ""}"
    };

        return string.Join(Environment.NewLine, lines);
    }

    private string BuildUpdatedByText(Customer customer)
    {
        var name = !string.IsNullOrWhiteSpace(customer.FullName)
            ? customer.FullName.Trim()
            : (!string.IsNullOrWhiteSpace(customer.PhoneNumber) ? customer.PhoneNumber.Trim() : "Khách hàng");

        return $"{name} (khách hàng)";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CancelFromMiniAppAsync
    // Chỉ chủ booking (CustomerId khớp) mới được huỷ.
    // Status → CancelledRefund. Gửi ZBS "BookingCancelled" + Email cancel.
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<MiniAppBookingDetailDto> CancelFromMiniAppAsync(Guid id, MiniAppCancelBookingDto input)
    {
        // 1. Load booking
        var booking = await _bookingRepo.FindAsync(id);
        if (booking == null)
            return new MiniAppBookingDetailDto
            {
                Error   = 404,
                Message = "Không tìm thấy booking"
            };

        // 2. Xác thực chủ booking
        if (booking.CustomerId != input.CustomerId)
            return new MiniAppBookingDetailDto
            {
                Error   = 403,
                Message = "Bạn không có quyền huỷ booking này"
            };

        // 3. Kiểm tra trạng thái — chỉ huỷ được khi chưa hoàn thành / chưa huỷ
        if (booking.Status == BookingStatus.CancelledRefund || booking.Status == BookingStatus.CancelledNoRefund)
            return new MiniAppBookingDetailDto
            {
                Error   = 400,
                Message = "Booking này đã được huỷ trước đó"
            };

        if (booking.Status == BookingStatus.Completed)
            return new MiniAppBookingDetailDto
            {
                Error   = 400,
                Message = "Booking đã hoàn thành, không thể huỷ"
            };

        // 4. Load customer để lấy thông tin gửi thông báo
        var customer = await _customerRepo.FindAsync(booking.CustomerId);

        // 5. Cập nhật status → CancelledRefund
        booking.Status = BookingStatus.CancelledRefund;
        await _bookingRepo.UpdateAsync(booking, autoSave: true);

        // ── Hoàn lại SlotAvailable khi booking bị hủy ──────────────────────
        if (booking.CalendarSlotId.HasValue)
        {
            var slotToRestore = await _calendarSlotRepo.FindAsync(booking.CalendarSlotId.Value);
            if (slotToRestore != null)
            {
                slotToRestore.SlotAvailable = Math.Min(
                    slotToRestore.MaxSlots,
                    slotToRestore.SlotAvailable + booking.NumberOfGolfers);
                await _calendarSlotRepo.UpdateAsync(slotToRestore, autoSave: true);
            }
        }

        // ── Lấy thông tin phụ để build thông báo ──────────────────────────
        var calendarSlot = await _calendarSlotRepo.FindAsync(booking.CalendarSlotId.Value);
        var golfInfo     = await GetGolfCourseInfoAsync(booking.GolfCourseId);
        var players      = await _playerRepo.GetListAsync(x => x.BookingId == booking.Id);
        var playersText  = BuildPlayersInlineText(players, customer?.FullName);

        static string ToHHmm(TimeSpan? ts)   => ts.HasValue ? ts.Value.ToString(@"hh\:mm") : "";
        static string ToDDMMYYYY(DateTime dt) => dt.ToString("dd/MM/yyyy");

        var playDateText = ToDDMMYYYY(booking.PlayDate);
        var teeFromText  = ToHHmm(calendarSlot?.TimeFrom);
        var teeToText    = ToHHmm(calendarSlot?.TimeTo);

        // 6. Gửi ZBS "BookingCancelled"
        try
        {
            if (customer != null && !string.IsNullOrWhiteSpace(customer.PhoneNumber))
            {
                await _jobManager.EnqueueAsync(
                    new ZbsSendJobArgs
                    {
                        TenantId    = CurrentTenant.Id,
                        TemplateKey = "BookingCancelled",
                        Phone       = customer.PhoneNumber,
                        TrackingId  = booking.Id.ToString(),
                        TemplateData = new
                        {
                            customer_name = customer.FullName,
                            booking_code  = booking.BookingCode,
                            tee_off_date  = playDateText,
                            tee_off_time  = teeFromText
                        }
                    },
                    priority: BackgroundJobPriority.Normal
                );
            }
        }
        catch
        {
            // không throw — ZBS lỗi không được block response
        }

        // 7. Gửi Email cancel
        try
        {
            var requesterName = !string.IsNullOrWhiteSpace(customer?.FullName)
                ? $"{customer.FullName.Trim()} (khách hàng)"
                : "Khách hàng";

            var cancelModel = new BookingCancelRequestEmailModelDto
            {
                BookingCode          = booking.BookingCode,
                BookerName           = customer?.FullName ?? "N/A",
                BookerPhone          = customer?.PhoneNumber ?? "N/A",
                CancelRequesterName  = requesterName,
                CancelRequesterPhone = customer?.PhoneNumber ?? "N/A",
                GolfCourseName       = golfInfo.Name,
                GolfCourseHotline    = golfInfo.Phone,
                GolfCourseAddress    = golfInfo.Address,
                PlayDate             = booking.PlayDate,
                PlayDateText         = playDateText,
                TeeTimeFromText      = teeFromText,
                TeeTimeToText        = teeToText,
                NumberOfGolfers      = booking.NumberOfGolfers,
                PlayersText          = playersText,
                CancelStatusText     = "Huỷ hoàn tiền"
            };

            var cfg = await GetEmailConfigAsync(
                AppEmailSettingNames.BookingCancel_ToEmails,
                AppEmailSettingNames.BookingCancel_CcEmails,
                AppEmailSettingNames.BookingCancel_BccEmails,
                AppEmailSettingNames.BookingCancel_SubjectTemplate,
                booking.BookingCode,
                fallbackTo: "tandv@baygolf.vn"
            );

            await _appEmailSenderService.EnqueueTemplateAsync(
                templateName: AppEmailTemplateNames.BookingCancelRequest,
                model:        cancelModel,
                toEmails:     cfg.To,
                subject:      cfg.Subject,
                cc:           cfg.Cc,
                bcc:          cfg.Bcc,
                bookingId:    booking.Id,
                bookingCode:  booking.BookingCode
            );
        }
        catch
        {
            // không throw — Email lỗi không được block response
        }

        // 8. Trả về detail booking đã huỷ
        return await GetMiniAppAsync(booking.Id, input.CustomerId);
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

        var toFinal = EmailHelper.NormalizeEmailList(to);
        if (string.IsNullOrWhiteSpace(toFinal)) toFinal = EmailHelper.NormalizeEmailList(fallbackTo);

        return (
            To: toFinal,
            Cc: NullIfEmpty(EmailHelper.NormalizeEmailList(cc)),
            Bcc: NullIfEmpty(EmailHelper.NormalizeEmailList(bcc)),
            Subject: ApplySubjectTemplate(subjectTpl, bookingCode)
        );
    }

    private async Task<List<BookingPriceBreakdownEmailItemDto>> BuildPriceBreakdownItemsAsync(
    Customer customer,
    Guid? calendarSlotId,
    short? numberHoles,
    int numberOfGolfers,
    Guid golfCourseId)
    {
        var result = new List<BookingPriceBreakdownEmailItemDto>();

        if (!calendarSlotId.HasValue || calendarSlotId.Value == Guid.Empty || numberOfGolfers <= 0)
        {
            return result;
        }

        var customerType = customer.CustomerTypeId.HasValue
            ? await _customerType.FindAsync(x => x.Id == customer.CustomerTypeId.Value)
            : null;

        var customerTypeCode = (customerType?.Code ?? "").Trim().ToUpperInvariant();

        var customerTypes = await _customerType.GetListAsync(x =>
            x.Code == "MB" || x.Code == "MBG" || x.Code == "VIS");

        var mbType = customerTypes.FirstOrDefault(x => x.Code == "MB");
        var mbgType = customerTypes.FirstOrDefault(x => x.Code == "MBG");
        var visType = customerTypes.FirstOrDefault(x => x.Code == "VIS");

        var slotPrices = await _calendarSlotPriceRepo.GetListAsync(x =>
            x.CalendarSlotId == calendarSlotId.Value);

        decimal GetPrice(CustomerType? type)
        {
            if (type == null)
            {
                return 0m;
            }

            var row = slotPrices.FirstOrDefault(x => x.CustomerTypeId == type.Id);
            return row == null
                ? 0m
                : PriceByHoleHelper.GetPriceByNumberHoles(row, numberHoles);
        }

        void AddItem(CustomerType? type, int count)
        {
            if (type == null || count <= 0)
            {
                return;
            }

            var price = GetPrice(type);

            result.Add(new BookingPriceBreakdownEmailItemDto
            {
                CustomerTypeCode = type.Code ?? "",
                CustomerTypeName = type.Code ?? type.Name ?? "",
                Price = price,
                PriceText = $"{price:N0}",
                Count = count
            });
        }

        var golfCourse = await _golfCourseRepo.FindAsync(golfCourseId);
        var isMemberSupported = golfCourse?.IsMemberSupported == true;

        // Theo nghiệp vụ hiện tại: Member được 1 suất MB, tối đa 3 suất MBG, còn lại VIS.
        // Nếu GolfCourse.MaxMemberGuest có cấu hình thì dùng cấu hình, nếu null thì mặc định 3.
        var maxMemberGuest = isMemberSupported
            ? Math.Max(0, golfCourse?.MaxMemberGuest ?? 3)
            : 0;

        if (customerTypeCode == "MB" && isMemberSupported)
        {
            AddItem(mbType, 1);

            var remaining = Math.Max(0, numberOfGolfers - 1);
            var mbgCount = Math.Min(maxMemberGuest, remaining);
            var visCount = Math.Max(0, remaining - mbgCount);

            AddItem(mbgType, mbgCount);
            AddItem(visType, visCount);

            return result;
        }

        // VIS hoặc các loại khách khác: tất cả người chơi cùng dùng giá của loại khách booking.
        if (customerTypeCode == "VIS")
        {
            AddItem(visType, numberOfGolfers);
            return result;
        }

        AddItem(customerType ?? visType, numberOfGolfers);
        return result;
    }

    private static decimal SumPriceBreakdownItems(List<BookingPriceBreakdownEmailItemDto> items)
    {
        return items == null || items.Count == 0
            ? 0m
            : items.Sum(x => x.Price * x.Count);
    }
}