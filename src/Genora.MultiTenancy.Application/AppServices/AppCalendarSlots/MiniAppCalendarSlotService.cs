using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Enums;
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

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots
{
    public class MiniAppCalendarSlotService : ApplicationService, IMiniAppCalendarSlotService
    {
        private readonly IRepository<CalendarSlot, Guid> _calendarSlotRepository;
        private readonly IRepository<CalendarSlotPrice, Guid> _priceRepository;
        private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
        private readonly IRepository<CustomerType, Guid> _customerTypeRepository;
        private readonly IRepository<Customer, Guid> _customerRepo;
        private readonly IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> _promotionTypeRepository;

        public MiniAppCalendarSlotService(
            IRepository<CalendarSlot, Guid> calendarSlotRepository,
            IRepository<CalendarSlotPrice, Guid> priceRepository,
            IRepository<GolfCourse, Guid> golfCourseRepository,
            IRepository<CustomerType, Guid> customerTypeRepository,
            IRepository<Customer, Guid> customerRepo,
            IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> promotionTypeRepository)
        {
            _customerRepo = customerRepo;
            _calendarSlotRepository = calendarSlotRepository;
            _priceRepository = priceRepository;
            _golfCourseRepository = golfCourseRepository;
            _customerTypeRepository = customerTypeRepository;
            _promotionTypeRepository = promotionTypeRepository;
        }

        public async Task<MiniAppCalendarSlotDto> GetListMiniAppAsync(GetMiniAppCalendarListInput input)
        {
            var result = new MiniAppCalendarSlotDto
            {
                FrameTimeOfDays = SessionOfDayEnum.List()
                    .Select(x => new FrameTimeOfDay { Id = x.Value, Name = x.Name })
                    .ToList()
            };

            if (string.IsNullOrEmpty(input.GolfCourseCode))
            {
                return new MiniAppCalendarSlotDto
                {
                    Error = (int)HttpStatusCode.BadRequest,
                    Message = "Vui lòng nhập mã sân để lấy giờ chơi"
                };
            }

            var query = await _calendarSlotRepository.GetQueryableAsync();
            var promotions = await _promotionTypeRepository.GetListAsync();

            var promotionDict = promotions.ToDictionary(x => x.Id, x => x);

            GolfCourse golfCourse = await _golfCourseRepository.FirstOrDefaultAsync(x => x.Code == input.GolfCourseCode);
            if (golfCourse == null)
            {
                return new MiniAppCalendarSlotDto
                {
                    Error = (int)HttpStatusCode.BadRequest,
                    Message = "Không tìm thấy sân golf"
                };
            }

            query = query.Where(x => x.GolfCourseId == golfCourse.Id && x.IsActive);

            if (input.Date.HasValue && input.Date.Value != DateTime.Now.Date)
            {
                query = query.Where(x => x.ApplyDate == input.Date.Value.Date);
            }
            else
            {
                query = query.Where(x =>
                    //(x.ApplyDate.Date > DateTime.Now.Date) &&
                    (x.ApplyDate.Date == DateTime.Now.Date && x.TimeTo >= DateTime.Now.TimeOfDay));
            }

            if (!string.IsNullOrEmpty(input.PromotionType))
            {
                var promotionType = promotions.FirstOrDefault(p => p.Code.Contains(input.PromotionType));
                if (promotionType != null)
                {
                    query = query.Where(x => x.PromotionTypeId == promotionType.Id);
                }
                else
                {
                    return result;
                }
            }

            if (input.FrameTime.HasValue)
            {
                if (input.FrameTime == SessionOfDayEnum.Morning.Value)
                {
                    var to = new TimeSpan(11, 0, 0);
                    query = query.Where(x => x.TimeTo <= to);
                }

                if (input.FrameTime == SessionOfDayEnum.Noon.Value)
                {
                    var from = new TimeSpan(11, 0, 0);
                    var to = new TimeSpan(13, 0, 0);
                    query = query.Where(x => x.TimeFrom >= from && x.TimeTo <= to);
                }

                if (input.FrameTime == SessionOfDayEnum.Afternoon.Value)
                {
                    var from = new TimeSpan(13, 0, 0);
                    var to = new TimeSpan(17, 30, 0);
                    query = query.Where(x => x.TimeFrom >= from && x.TimeTo <= to);
                }

                if (input.FrameTime == SessionOfDayEnum.Evening.Value)
                {
                    var from = new TimeSpan(17, 30, 0);
                    query = query.Where(x => x.TimeFrom >= from);
                }
            }

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(CalendarSlot.ApplyDate) + " asc, " + nameof(CalendarSlot.TimeFrom) + " asc"
                : input.Sorting;

            query = query.OrderBy(sorting);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var slots = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            var dtoList = slots
                .Select(slot =>
                {
                    var promotionId = slot.PromotionTypeId != Guid.Empty ? slot.PromotionTypeId : Guid.Empty;
                    promotionDict.TryGetValue(promotionId, out var promotion);

                    return new CalendarSlotData
                    {
                        Id = slot.Id,
                        GolfCourseId = slot.GolfCourseId,
                        GolfCourseCode = golfCourse.Code,
                        PlayDate = slot.ApplyDate,
                        TimeFrom = slot.TimeFrom,
                        TimeTo = slot.TimeTo,
                        PromotionId = slot.PromotionTypeId,
                        PromotionCode = promotion?.Code,
                        PromotionName = promotion?.Name,
                        PromotionIconUrl = promotion?.IconUrl,
                        PromotionColorCode = promotion?.ColorCode,
                        MaxSlots = slot.MaxSlots,
                        SlotAvailable = slot.SlotAvailable,
                    };
                })
                .ToList();

            var calendarIds = dtoList.Select(c => c.Id).ToList();
            var prices = await _priceRepository.GetListAsync(p => calendarIds.Contains(p.CalendarSlotId));
            var customerTypes = await _customerTypeRepository.GetListAsync();

            var customerTypeDict = customerTypes.ToDictionary(x => x.Id, x => x);

            var user = (input.CustomerId.HasValue && input.CustomerId != Guid.Empty)
                ? await _customerRepo.FirstOrDefaultAsync(c => c.Id == input.CustomerId)
                : null;

            var currentCustomerType = (user != null && user.CustomerTypeId.HasValue && customerTypeDict.ContainsKey(user.CustomerTypeId.Value))
                ? customerTypeDict[user.CustomerTypeId.Value]
                : null;

            var visCustomerType = customerTypes.FirstOrDefault(c => c.Code == "VIS");
            var visCustomerTypeId = visCustomerType?.Id ?? Guid.Empty;

            var mbgCustomerType = customerTypes.FirstOrDefault(c => c.Code == "MBG");
            var mbCustomerType  = customerTypes.FirstOrDefault(c => c.Code == "MB");
            var isCurrentMember = golfCourse.IsMemberSupported
                && currentCustomerType?.Code == "MB";

            foreach (var item in dtoList)
            {
                item.FrameTime = $"{item.TimeFrom} - {item.TimeTo}";
                item.IsBestDeal = item.PromotionName == "Best Deal";
                item.FrameTimeOfDayId = FormatSessionOfDayHelper.DateTimeToSessionOfDay(item.TimeFrom.Value).Value;
                item.FrameTimeOfDayName = FormatSessionOfDayHelper.DateTimeToSessionOfDay(item.TimeFrom.Value).Name;

                item.CustomerTypeCode = currentCustomerType?.Code ?? visCustomerType?.Code;

                var slotPrices = prices.Where(p => p.CalendarSlotId == item.Id).ToList();

                // ===== Giá gốc theo loại khách hàng đã cấu hình trong AppCustomerTypes =====
                decimal visitorPrice = 0m;
                string originalPriceSource = "None";

                if (currentCustomerType != null && currentCustomerType.OriginalPrice.HasValue && currentCustomerType.OriginalPrice.Value > 0)
                {
                    visitorPrice = currentCustomerType.OriginalPrice.Value;
                    originalPriceSource = $"CustomerType:{currentCustomerType.Code}";
                }
                else if (user == null && visCustomerType != null && visCustomerType.OriginalPrice.HasValue && visCustomerType.OriginalPrice.Value > 0)
                {
                    visitorPrice = visCustomerType.OriginalPrice.Value;
                    originalPriceSource = $"CustomerType:{visCustomerType.Code}";
                }

                // Fallback về logic cũ nếu chưa cấu hình OriginalPrice
                if (visitorPrice <= 0)
                {
                    var visRow = slotPrices.FirstOrDefault(p => p.CustomerTypeId == visCustomerTypeId);
                    visitorPrice = visRow != null
                        ? PriceByHoleHelper.GetPriceByNumberHoles(visRow, input.NumberHoles)
                        : 0m;

                    if (visitorPrice > 0)
                    {
                        originalPriceSource = "CalendarSlotPrice:VIS";
                    }

                    if (visitorPrice <= 0 && slotPrices.Count > 0)
                    {
                        visitorPrice = slotPrices
                            .Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, input.NumberHoles))
                            .DefaultIfEmpty(0m)
                            .Max();

                        if (visitorPrice > 0)
                        {
                            originalPriceSource = "CalendarSlotPrice:MAX";
                        }
                    }
                }

                item.OriginalPrice = visitorPrice;
                item.OriginalPriceSource = originalPriceSource;

                // ===== Giá theo loại khách hiện tại =====
                decimal myPrice;
                if (user != null && user.CustomerTypeId.HasValue)
                {
                    var myRow = slotPrices.FirstOrDefault(p => p.CustomerTypeId == user.CustomerTypeId.Value);
                    myPrice = myRow != null
                        ? PriceByHoleHelper.GetPriceByNumberHoles(myRow, input.NumberHoles)
                        : slotPrices.Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, input.NumberHoles))
                                   .DefaultIfEmpty(0m)
                                   .Min();
                }
                else
                {
                    myPrice = slotPrices.Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, input.NumberHoles))
                                        .DefaultIfEmpty(0m)
                                        .Max();
                }

                item.CustomerTypePrice = myPrice;

                // ===== VisitorPrice = giá VIS từ AppCalendarSlotPrices theo NumberHoles =====
                var visSlotRow = slotPrices.FirstOrDefault(p => p.CustomerTypeId == visCustomerTypeId);
                item.VisitorPrice = visSlotRow != null
                    ? PriceByHoleHelper.GetPriceByNumberHoles(visSlotRow, input.NumberHoles)
                    : 0m;

                item.DiscountPercent = (item.VisitorPrice - item.CustomerTypePrice) > 0 && item.VisitorPrice > 0
                    ? Math.Round(100 - (item.CustomerTypePrice / item.VisitorPrice) * 100, MidpointRounding.AwayFromZero)
                    : 0;

                // ===== Member config =====
                item.IsMemberSupported = golfCourse.IsMemberSupported;
                item.MaxMemberGuest = golfCourse.IsMemberSupported ? golfCourse.MaxMemberGuest : null;

                if (isCurrentMember && mbgCustomerType != null)
                {
                    var mbgRow = slotPrices.FirstOrDefault(p => p.CustomerTypeId == mbgCustomerType.Id);
                    if (mbgRow != null)
                    {
                        var mbgPrice = PriceByHoleHelper.GetPriceByNumberHoles(mbgRow, input.NumberHoles);
                        item.MemberGuestPrice = mbgPrice > 0 ? mbgPrice : null;
                    }
                }

                // ===== Tính customerBillTotalPrice / originalBillTotalPrice / discountTotalPrice =====
                int slotCount    = item.SlotAvailable;
                int maxMbg       = golfCourse.IsMemberSupported ? (golfCourse.MaxMemberGuest ?? 0) : 0;

                if (isCurrentMember)
                {
                    // MB + MBG guests + Visitor phần còn lại
                    decimal mbSlotPrice  = item.CustomerTypePrice;
                    decimal mbgSlotPrice = item.MemberGuestPrice ?? 0m;
                    int remaining        = Math.Max(0, slotCount - maxMbg - 1);

                    item.CustomerBillTotalPrice = mbSlotPrice
                        + (mbgSlotPrice * maxMbg)
                        + (remaining * item.VisitorPrice);

                    // Giá gốc theo AppCustomerTypes.OriginalPrice
                    decimal mbOriginal  = mbCustomerType?.OriginalPrice  ?? 0m;
                    decimal mbgOriginal = mbgCustomerType?.OriginalPrice ?? 0m;
                    decimal visOriginal = visCustomerType?.OriginalPrice ?? 0m;

                    item.OriginalBillTotalPrice = mbOriginal
                        + (mbgOriginal * maxMbg)
                        + (remaining * visOriginal);
                }
                else
                {
                    // Visitor hoặc sân không hỗ trợ Member - dùng giá CustomerTypePrice (của loại khách hàng hiện tại)
                    item.CustomerBillTotalPrice = item.CustomerTypePrice * slotCount;

                    // Tính OriginalBillTotalPrice dựa vào OriginalPrice của loại khách hàng hiện tại
                    decimal currentOriginalPrice = (currentCustomerType?.OriginalPrice ?? 0m) > 0
                        ? currentCustomerType.OriginalPrice.Value
                        : (visCustomerType?.OriginalPrice ?? 0m);
                    item.OriginalBillTotalPrice = currentOriginalPrice * slotCount;
                }

                item.DiscountTotalPrice = Math.Max(0m, item.OriginalBillTotalPrice - item.CustomerBillTotalPrice);
            }

            result.Data = new PagedResultDto<CalendarSlotData>(totalCount, dtoList);
            return result;
        }

        public async Task<AppCalendarSlotDto> GetMiniAppAsync(Guid id)
        {
            var slot = await _calendarSlotRepository.FindAsync(id);
            if (slot == null)
            {
                throw new EntityNotFoundException(typeof(CalendarSlot), id);
            }

            var golf = await _golfCourseRepository.FindAsync(slot.GolfCourseId);
            var prices = await _priceRepository.GetListAsync(p => p.CalendarSlotId == id);

            var customerTypeIds = prices.Select(p => p.CustomerTypeId).Distinct().ToList();
            var customerTypes = await _customerTypeRepository.GetListAsync(ct => customerTypeIds.Contains(ct.Id));
            var ctDict = customerTypes.ToDictionary(ct => ct.Id, ct => ct);

            var dto = new AppCalendarSlotDto
            {
                Id = slot.Id,
                TenantId = slot.TenantId,
                GolfCourseId = slot.GolfCourseId,
                GolfCourseName = golf?.Name ?? string.Empty,
                ApplyDate = slot.ApplyDate,
                TimeFrom = slot.TimeFrom,
                TimeTo = slot.TimeTo,
                PromotionTypeId = slot.PromotionTypeId,
                MaxSlots = slot.MaxSlots,
                SlotAvailable = slot.SlotAvailable,
                InternalNote = slot.InternalNote,
                IsActive = slot.IsActive,
                CreationTime = slot.CreationTime,
                CreatorId = slot.CreatorId,
                LastModificationTime = slot.LastModificationTime,
                LastModifierId = slot.LastModifierId,
                Prices = new List<AppCalendarSlotPriceDto>()
            };

            foreach (var p in prices)
            {
                ctDict.TryGetValue(p.CustomerTypeId, out var ct);

                dto.Prices.Add(new AppCalendarSlotPriceDto
                {
                    Id = p.Id,
                    CalendarSlotId = p.CalendarSlotId,
                    CustomerTypeId = p.CustomerTypeId,
                    CustomerTypeCode = ct?.Code,
                    CustomerTypeName = ct?.Name,
                    Price9 = p.Price9 ?? 0m,
                    Price18 = p.Price18,
                    Price27 = p.Price27 ?? 0m,
                    Price36 = p.Price36 ?? 0m
                });
            }

            return dto;
        }

        public async Task<AppCalendarSlotDto> GetMiniAppAsync(GetMiniAppCalendarSlotDetailInput input)
        {
            var slot = await _calendarSlotRepository.FindAsync(input.Id);
            if (slot == null)
            {
                throw new EntityNotFoundException(typeof(CalendarSlot), input.Id);
            }

            var golf = await _golfCourseRepository.FindAsync(slot.GolfCourseId);
            var prices = await _priceRepository.GetListAsync(p => p.CalendarSlotId == input.Id);

            var customerTypeIds = prices.Select(p => p.CustomerTypeId).Distinct().ToList();
            var customerTypes = await _customerTypeRepository.GetListAsync(ct => customerTypeIds.Contains(ct.Id));
            var ctDict = customerTypes.ToDictionary(ct => ct.Id, ct => ct);

            var dto = new AppCalendarSlotDto
            {
                Id = slot.Id,
                TenantId = slot.TenantId,
                GolfCourseId = slot.GolfCourseId,
                GolfCourseName = golf?.Name ?? string.Empty,
                ApplyDate = slot.ApplyDate,
                TimeFrom = slot.TimeFrom,
                TimeTo = slot.TimeTo,
                PromotionTypeId = slot.PromotionTypeId,
                MaxSlots = slot.MaxSlots,
                SlotAvailable = slot.SlotAvailable,
                InternalNote = slot.InternalNote,
                IsActive = slot.IsActive,
                CreationTime = slot.CreationTime,
                CreatorId = slot.CreatorId,
                LastModificationTime = slot.LastModificationTime,
                LastModifierId = slot.LastModifierId,
                Prices = new List<AppCalendarSlotPriceDto>()
            };

            foreach (var p in prices)
            {
                ctDict.TryGetValue(p.CustomerTypeId, out var ct);

                dto.Prices.Add(new AppCalendarSlotPriceDto
                {
                    Id = p.Id,
                    CalendarSlotId = p.CalendarSlotId,
                    CustomerTypeId = p.CustomerTypeId,
                    CustomerTypeCode = ct?.Code,
                    CustomerTypeName = ct?.Name,
                    Price9 = p.Price9 ?? 0m,
                    Price18 = p.Price18,
                    Price27 = p.Price27 ?? 0m,
                    Price36 = p.Price36 ?? 0m
                });
            }

            // Tính toán giá dựa trên số người chơi
            var numberHoles = input.NumberHoles ?? 18;
            var playerNumber = input.PlayerNumber > 0 ? input.PlayerNumber : 1;

            var visCustomerType = customerTypes.FirstOrDefault(c => c.Code == "VIS");
            var visCustomerTypeId = visCustomerType?.Id ?? Guid.Empty;
            var mbgCustomerType = customerTypes.FirstOrDefault(c => c.Code == "MBG");
            var mbCustomerType = customerTypes.FirstOrDefault(c => c.Code == "MB");

            var user = (input.CustomerId.HasValue && input.CustomerId != Guid.Empty)
                ? await _customerRepo.FirstOrDefaultAsync(c => c.Id == input.CustomerId)
                : null;

            var currentCustomerType = (user != null && user.CustomerTypeId.HasValue && ctDict.ContainsKey(user.CustomerTypeId.Value))
                ? ctDict[user.CustomerTypeId.Value]
                : null;

            var isCurrentMember = golf?.IsMemberSupported == true && currentCustomerType?.Code == "MB";

            // Tính giá gốc theo loại khách hàng
            decimal visitorPrice = 0m;
            string originalPriceSource = "None";

            if (currentCustomerType != null && currentCustomerType.OriginalPrice.HasValue && currentCustomerType.OriginalPrice.Value > 0)
            {
                visitorPrice = currentCustomerType.OriginalPrice.Value;
                originalPriceSource = $"CustomerType:{currentCustomerType.Code}";
            }
            else if (user == null && visCustomerType != null && visCustomerType.OriginalPrice.HasValue && visCustomerType.OriginalPrice.Value > 0)
            {
                visitorPrice = visCustomerType.OriginalPrice.Value;
                originalPriceSource = $"CustomerType:{visCustomerType.Code}";
            }

            // Fallback về logic cũ nếu chưa cấu hình OriginalPrice
            if (visitorPrice <= 0)
            {
                var visRow = prices.FirstOrDefault(p => p.CustomerTypeId == visCustomerTypeId);
                visitorPrice = visRow != null
                    ? PriceByHoleHelper.GetPriceByNumberHoles(visRow, numberHoles)
                    : 0m;

                if (visitorPrice > 0)
                {
                    originalPriceSource = "CalendarSlotPrice:VIS";
                }

                if (visitorPrice <= 0 && prices.Count > 0)
                {
                    visitorPrice = prices
                        .Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, numberHoles))
                        .DefaultIfEmpty(0m)
                        .Max();

                    if (visitorPrice > 0)
                    {
                        originalPriceSource = "CalendarSlotPrice:MAX";
                    }
                }
            }

            dto.OriginalPrice = visitorPrice;
            dto.OriginalPriceSource = originalPriceSource;

            // Giá theo loại khách hiện tại
            decimal myPrice;
            if (user != null && user.CustomerTypeId.HasValue)
            {
                var myRow = prices.FirstOrDefault(p => p.CustomerTypeId == user.CustomerTypeId.Value);
                myPrice = myRow != null
                    ? PriceByHoleHelper.GetPriceByNumberHoles(myRow, numberHoles)
                    : prices.Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, numberHoles))
                           .DefaultIfEmpty(0m)
                           .Min();
            }
            else
            {
                myPrice = prices.Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, numberHoles))
                                .DefaultIfEmpty(0m)
                                .Max();
            }

            dto.CustomerTypePrice = myPrice;

            // VisitorPrice từ AppCalendarSlotPrices
            var visSlotRow = prices.FirstOrDefault(p => p.CustomerTypeId == visCustomerTypeId);
            dto.VisitorPrice = visSlotRow != null
                ? PriceByHoleHelper.GetPriceByNumberHoles(visSlotRow, numberHoles)
                : 0m;

            dto.DiscountPercent = (dto.VisitorPrice - dto.CustomerTypePrice) > 0 && dto.VisitorPrice > 0
                ? Math.Round(100 - (dto.CustomerTypePrice / dto.VisitorPrice) * 100, MidpointRounding.AwayFromZero)
                : 0;

            // Set customer type code
            dto.CustomerTypeCode = currentCustomerType?.Code ?? visCustomerType?.Code;

            // Member config
            dto.IsMemberSupported = golf?.IsMemberSupported ?? false;
            dto.MaxMemberGuest = golf?.IsMemberSupported == true ? golf.MaxMemberGuest : null;

            if (isCurrentMember && mbgCustomerType != null)
            {
                var mbgRow = prices.FirstOrDefault(p => p.CustomerTypeId == mbgCustomerType.Id);
                if (mbgRow != null)
                {
                    var mbgPrice = PriceByHoleHelper.GetPriceByNumberHoles(mbgRow, numberHoles);
                    dto.MemberGuestPrice = mbgPrice > 0 ? mbgPrice : null;
                }
            }

            // Tính toán CustomerBillTotalPrice, OriginalBillTotalPrice, DiscountTotalPrice dựa trên playerNumber
            int maxMbg = golf?.IsMemberSupported == true ? (golf.MaxMemberGuest ?? 0) : 0;

            if (isCurrentMember)
            {
                // MB + MBG guests + Visitor phần còn lại
                decimal mbSlotPrice = dto.CustomerTypePrice;
                decimal mbgSlotPrice = dto.MemberGuestPrice ?? 0m;
                int visitorSlots = Math.Max(0, playerNumber - maxMbg - 1);

                dto.CustomerBillTotalPrice = mbSlotPrice
                    + (mbgSlotPrice * Math.Min(maxMbg, playerNumber - 1))
                    + (visitorSlots * dto.VisitorPrice);

                // Giá gốc theo AppCustomerTypes.OriginalPrice
                decimal mbOriginal = mbCustomerType?.OriginalPrice ?? 0m;
                decimal mbgOriginal = mbgCustomerType?.OriginalPrice ?? 0m;
                decimal visOriginal = visCustomerType?.OriginalPrice ?? 0m;

                dto.OriginalBillTotalPrice = mbOriginal
                    + (mbgOriginal * Math.Min(maxMbg, playerNumber - 1))
                    + (visitorSlots * visOriginal);
            }
            else
            {
                // Visitor hoặc sân không hỗ trợ Member - dùng giá CustomerTypePrice (của loại khách hàng hiện tại)
                dto.CustomerBillTotalPrice = dto.CustomerTypePrice * playerNumber;

                // Tính OriginalBillTotalPrice dựa vào OriginalPrice của loại khách hàng hiện tại
                decimal currentOriginalPrice = (currentCustomerType?.OriginalPrice ?? 0m) > 0
                    ? currentCustomerType.OriginalPrice.Value
                    : (visCustomerType?.OriginalPrice ?? 0m);
                dto.OriginalBillTotalPrice = currentOriginalPrice * playerNumber;
            }

            dto.DiscountTotalPrice = Math.Max(0m, dto.OriginalBillTotalPrice - dto.CustomerBillTotalPrice);

            return dto;
        }

        private static int ParseHoles(object? numberHoles)
        {
            if (numberHoles == null) return 18;
            var s = numberHoles.ToString()?.Trim();
            if (int.TryParse(s, out var n) && (n == 9 || n == 18 || n == 27 || n == 36))
                return n;
            return 18;
        }

        private static decimal PickPriceByHoles(CalendarSlotPrice row, int holes)
        {
            return holes switch
            {
                9 => row.Price9 ?? row.Price18,
                18 => row.Price18,
                27 => row.Price27 ?? row.Price18,
                36 => row.Price36 ?? row.Price18,
                _ => row.Price18
            };
        }
    }
}