using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
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
        public MiniAppCalendarSlotService(IRepository<CalendarSlot, Guid> calendarSlotRepository, IRepository<CalendarSlotPrice, Guid> priceRepository, IRepository<GolfCourse, Guid> golfCourseRepository, IRepository<CustomerType, Guid> customerTypeRepository, IRepository<Customer, Guid> customerRepo, IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> promotionTypeRepository)
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
            var result = new MiniAppCalendarSlotDto();
            result.FrameTimeOfDays = SessionOfDayEnum.List().Select(x => new FrameTimeOfDay { Id = x.Value, Name = x.Name }).ToList();
            var query = await _calendarSlotRepository.GetQueryableAsync();
            var promotion = await _promotionTypeRepository.GetListAsync();
            if (string.IsNullOrEmpty(input.GolfCourseCode)) return new MiniAppCalendarSlotDto { Error = (int)HttpStatusCode.BadRequest, Message = "Vui lòng nhập mã sân để lấy giờ chơi"};
            GolfCourse golfCourse = await _golfCourseRepository.FirstOrDefaultAsync(x => x.Code == input.GolfCourseCode);
            if (!string.IsNullOrEmpty(input.GolfCourseCode))
            {
                query = query.Where(x => x.GolfCourseId == golfCourse.Id);
            }
            //if (golfCourse == null) return;
            if (input.Date.HasValue && input.Date.Value != DateTime.Now.Date)
            {
                query = query.Where(x => x.ApplyDate == input.Date.Value.Date);
            }
            else
            {
                query = query.Where(x => (x.ApplyDate.Date > DateTime.Now.Date) || (x.ApplyDate.Date == DateTime.Now.Date && x.TimeTo >= DateTime.Now.TimeOfDay));
            }

            if (!string.IsNullOrEmpty(input.PromotionType))
            {
                var promotionType = promotion.FirstOrDefault(p => p.Code.Contains(input.PromotionType));
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
                //query = query.Where(x => x.IsActive == input.IsActive.Value);
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
            .Select(slot => new CalendarSlotData
            {
                Id = slot.Id,
                //TenantId = slot.TenantId,
                GolfCourseId = slot.GolfCourseId,
                GolfCourseCode = golfCourse.Code,
                PlayDate = slot.ApplyDate,
                TimeFrom = slot.TimeFrom,
                TimeTo = slot.TimeTo,
                PromotionId = slot.PromotionTypeId,
                PromotionName = promotion.FirstOrDefault(p => p.Id == slot.PromotionTypeId)?.Name,
                MaxSlots = slot.MaxSlots,
            })
            .ToList();
            var calendarIds = dtoList.Select(c => c.Id).ToList();
            var prices = await _priceRepository.GetListAsync(p => calendarIds.Contains(p.CalendarSlotId));
            var customerTypes = await _customerTypeRepository.GetListAsync();
            var user = (input.CustomerId.HasValue && input.CustomerId != Guid.Empty) ? await _customerRepo.FirstOrDefaultAsync(c => c.Id == input.CustomerId) : null;
            var holes = ParseHoles(input.NumberHoles); // FE truyền
            var visCustomerTypeId = customerTypes.FirstOrDefault(c => c.Code == "VIS")?.Id ?? Guid.Empty;
            foreach (var item in dtoList)
            {
                item.FrameTime = $"{item.TimeFrom} - {item.TimeTo}";
                item.IsBestDeal = item.PromotionName == "Best Deal";
                item.FrameTimeOfDayId = FormatSessionOfDayHelper.DateTimeToSessionOfDay(item.TimeFrom.Value).Value;
                item.FrameTimeOfDayName = FormatSessionOfDayHelper.DateTimeToSessionOfDay(item.TimeFrom.Value).Name;

                var slotPrices = prices.Where(p => p.CalendarSlotId == item.Id).ToList();

                // VIS price
                var visRow = slotPrices.FirstOrDefault(p => p.CustomerTypeId == visCustomerTypeId);
                var visPrice = visRow != null
                    ? PriceByHoleHelper.GetPriceByNumberHoles(visRow, input.NumberHoles)
                    : 0m;

                if (visPrice <= 0 && slotPrices.Count > 0)
                {
                    visPrice = slotPrices
                        .Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, input.NumberHoles))
                        .DefaultIfEmpty(0m)
                        .Max();
                }

                item.VisitorPrice = visPrice;

                // Price theo loại khách hiện tại
                decimal myPrice;
                if (user != null)
                {
                    var myRow = slotPrices.FirstOrDefault(p => p.CustomerTypeId == user.CustomerTypeId);
                    myPrice = myRow != null
                        ? PriceByHoleHelper.GetPriceByNumberHoles(myRow, input.NumberHoles)
                        : slotPrices.Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, input.NumberHoles)).DefaultIfEmpty(0m).Min();
                }
                else
                {
                    myPrice = slotPrices.Select(p => PriceByHoleHelper.GetPriceByNumberHoles(p, input.NumberHoles)).DefaultIfEmpty(0m).Min();
                }

                item.CustomerTypePrice = myPrice;

                item.DiscountPercent = (item.VisitorPrice - item.CustomerTypePrice) > 0 && item.VisitorPrice > 0
                    ? Math.Round(100 - (item.CustomerTypePrice / item.VisitorPrice) * 100, MidpointRounding.AwayFromZero)
                    : 0;
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

            // Lấy sân golf
            var golf = await _golfCourseRepository.FindAsync(slot.GolfCourseId);

            // Lấy giá
            var prices = await _priceRepository.GetListAsync(p => p.CalendarSlotId == id);

            // Lấy loại khách cho các giá
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
