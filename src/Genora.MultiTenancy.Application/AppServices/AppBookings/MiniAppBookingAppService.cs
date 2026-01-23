using DocumentFormat.OpenXml.Vml.Office;
using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
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
    private readonly IRepository<GolfCourse, Guid> _golfcourseRepo;
    private readonly IRepository<CalendarSlot, Guid> _calendarSlotRepo;
    private readonly IRepository<CalendarSlotPrice, Guid> _calendarSlotPriceRepo;
    private readonly IRepository<CustomerType, Guid> _customerType;
    private readonly IAppEmailSenderService _appEmailSenderService;
    private readonly IRepository<OptionExtend, Guid> _optionExtendRepo;
    private readonly ISettingProvider _settingProvider;
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
        ISettingProvider settingProvider)
    {
        _bookingRepo = bookingRepo;
        _playerRepo = playerRepo;
        _customerRepo = customerRepo;
        _golfcourseRepo = golfcourseRepo;
        _calendarSlotRepo = calendarSlotRepo;
        _calendarSlotPriceRepo = calendarSlotPriceRepo;
        _customerType = customerType;
        _appEmailSenderService = appEmailSenderService;
        _optionExtendRepo = optionExtendRepo;
        _settingProvider = settingProvider;
    }

    public async Task<MiniAppBookingDetailDto> CreateFromMiniAppAsync(MiniAppCreateBookingDto input)
    {

        var customer = await _customerRepo.GetAsync(input.CustomerId);
        if (customer == null) return new MiniAppBookingDetailDto { Error = (int)HttpStatusCode.Unauthorized, Message = "Quý khách chưa đăng nhập dịch vụ" };

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

        // BookingCode: CustomerCode + ddMMyy + serial/day (reset theo ngày)
        var slotWithPrices = await _calendarSlotRepo.WithDetailsAsync(c => c.Prices);
        var calendarSlot = slotWithPrices.FirstOrDefault(c => c.Id == input.CalendarSlotId);
        if (calendarSlot == null)
            return new MiniAppBookingDetailDto { Error = (int)HttpStatusCode.NotFound, Message = "Không tìm thấy giờ chơi" };

        var datePart = calendarSlot?.ApplyDate.ToString("ddMMyy");

        var countInDay = await _bookingRepo.CountAsync(x => x.PlayDate.Date == input.PlayDate.Date);
        var serial = (countInDay + 1).ToString("D3");

        var bookingCode = $"{customer.CustomerCode}-{datePart}-{serial}";
        if (await _bookingRepo.AnyAsync(b => b.BookingCode == bookingCode))
        {
            // trường hợp hiếm: đã có booking cùng mã
            bookingCode = $"{customer.CustomerCode}-{datePart}-{serial + 1}";
        }
        // Lấy giá theo loại khách + số hố
        var myPriceRow = calendarSlot.Prices.FirstOrDefault(x => x.CustomerTypeId == customer.CustomerTypeId);

        // nếu không có giá theo loại khách, fallback lấy VIS (nếu có), nếu không thì lấy dòng bất kỳ
        if (myPriceRow == null)
        {
            var visType = await _customerType.FirstOrDefaultAsync(c => c.Code == "VIS");
            if (visType != null)
                myPriceRow = calendarSlot.Prices.FirstOrDefault(x => x.CustomerTypeId == visType.Id);

            myPriceRow ??= calendarSlot.Prices.FirstOrDefault();
        }

        // Giá / golfer theo số hố
        input.PricePerGolfer = myPriceRow != null
            ? PriceByHoleHelper.GetPriceByNumberHoles(myPriceRow, input.NumberHoles)
            : 0m;
        // Tổng tiền
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

        if (input.Players != null && input.Players.Any())
        {
            foreach (var p in input.Players)
            {
                var player = new BookingPlayer(
                    GuidGenerator.Create(),
                    booking.Id,
                    p.CustomerId,
                    p.PlayerName,
                    p.PricePerPlayer,
                    p.Notes
                );
                player.VgaCode = p.VgaCode;
                player.PricePerPlayer = booking.PricePerGolfer;

                await _playerRepo.InsertAsync(player, autoSave: true);
            }
        }

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

            var otherRequestsText = await BuildOtherRequestsTextAsync(booking.Utility);
            var model = new BookingNewRequestEmailModelDto
            {
                BookingCode = booking.BookingCode,
                BookerName = customer.FullName ?? "N/A",
                BookerPhone = customer.PhoneNumber ?? "N/A",

                // giữ DateTime gốc để audit
                PlayDate = booking.PlayDate,
                PlayDateText = ToDDMMYYYY(booking.PlayDate),

                // giờ chơi (TimeSpan -> HH:mm)
                TeeTimeFromText = ToHHmm(calendarSlot?.TimeFrom),
                TeeTimeToText = ToHHmm(calendarSlot?.TimeTo),

                // nếu vẫn muốn TeeTime gộp
                TeeTime = $"{ToHHmm(calendarSlot?.TimeFrom)} - {ToHHmm(calendarSlot?.TimeTo)}",

                NumberOfGolfers = booking.NumberOfGolfers,
                CustomerTypeSummary = customerTypeSummary,

                TotalAmount = booking.TotalAmount,
                TotalAmountText = $"{booking.TotalAmount:N0} đ",

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

            var subject = ApplyTemplate(subjectTpl, booking.BookingCode);

            await _appEmailSenderService.EnqueueTemplateAsync(
                templateName: AppEmailTemplateNames.BookingNewRequest,
                model: model,
                toEmails: toEmails ?? "tandv@baygolf.vn", // fallback nếu chưa set
                subject: subject,
                cc: NullIfEmpty(ccEmails),
                bcc: NullIfEmpty(bccEmails),
                bookingId: booking.Id,
                bookingCode: booking.BookingCode
            );
        }
        catch
        {
            // ✅ không throw để tránh ảnh hưởng kết quả booking của mini app
        }

        // trả về dto đầy đủ (kèm players)
        var dto = ObjectMapper.Map<Booking, BookingDetailData>(booking);
        dto.NumberHoles = booking.NumberHole;
        dto.Utilities = input.Utilities;
        dto.FrameTimes = $"{calendarSlot.TimeFrom} - {calendarSlot.TimeTo}";
        var players = await _playerRepo.GetListAsync(x => x.BookingId == booking.Id);
        dto.Players = ObjectMapper.Map<System.Collections.Generic.List<BookingPlayer>, System.Collections.Generic.List<AppBookingPlayerDto>>(players);

        return new MiniAppBookingDetailDto { Error = 0, Message = "Success", Data = dto };
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
                    var calendar = calendars.FirstOrDefault(x => x.Id == item.CalendarSlotId.Value);
                    if (calendar != null)
                    {
                        item.FrameTimes = $"{calendar.TimeFrom} - {calendar.TimeTo}";
                    }
                }
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
            var players = await _playerRepo.GetListAsync(x => x.BookingId == id);
            dto.Players = ObjectMapper.Map<System.Collections.Generic.List<BookingPlayer>, System.Collections.Generic.List<AppBookingPlayerDto>>(players);
            dto.Utilities = string.IsNullOrEmpty(booking.Utility) ? new List<int>() : booking.Utility.Split(",").Select(int.Parse).ToList();
            dto.NumberHoles = booking.NumberHole;
            var visCustomerType = await _customerType.FirstOrDefaultAsync(c => c.Code == "VIS");
            var visCustomerTypeId = visCustomerType?.Id;

            if (dto.CalendarSlotId.HasValue && dto.CalendarSlotId.Value != Guid.Empty)
            {
                var getCalendarPrice = await _calendarSlotPriceRepo.FirstOrDefaultAsync(
    x => x.CalendarSlotId == booking.CalendarSlotId
      && x.CustomerTypeId == visCustomerTypeId
);

                var basePrice = getCalendarPrice != null
                    ? PriceByHoleHelper.GetPriceByNumberHoles(getCalendarPrice, booking.NumberHole)
                    : 0m;

                dto.OriginalTotalAmount = basePrice * (booking?.NumberOfGolfers ?? 0);

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

        // parse "1,4" => [1,4]
        var ids = utilityCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(x => int.TryParse(x, out var n) ? (int?)n : null)
                            .Where(x => x.HasValue)
                            .Select(x => x!.Value)
                            .Distinct()
                            .ToList();

        if (ids.Count == 0)
            return string.Empty;

        var query = await _optionExtendRepo.GetQueryableAsync();

        // ví dụ Type == OptionExtendTypeEnum.GolfCourseUlitity.Value
        var utilities = query
            .Where(x => ids.Contains(x.OptionId))
            .Select(x => new { x.OptionId, x.OptionName })
            .ToList();

        if (utilities.Count == 0)
            return string.Empty;

        // giữ thứ tự theo input ids
        var dict = utilities.ToDictionary(x => x.OptionId, x => x.OptionName);

        var lines = ids
            .Where(id => dict.ContainsKey(id))
            .Select(id => $"• {dict[id]}");

        // không dư dòng, join đúng newline
        return string.Join(Environment.NewLine, lines);
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