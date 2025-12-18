using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        public MiniAppCalendarSlotService(IRepository<CalendarSlot, Guid> calendarSlotRepository, IRepository<CalendarSlotPrice, Guid> priceRepository, IRepository<GolfCourse, Guid> golfCourseRepository, IRepository<CustomerType, Guid> customerTypeRepository)
        {
            _calendarSlotRepository = calendarSlotRepository;
            _priceRepository = priceRepository;
            _golfCourseRepository = golfCourseRepository;
            _customerTypeRepository = customerTypeRepository;
        }

        public async Task<PagedResultDto<AppCalendarSlotDto>> GetListMiniAppAsync(GetCalendarSlotListInput input)
        {
            var query = await _calendarSlotRepository.GetQueryableAsync();

            if (input.GolfCourseId.HasValue)
            {
                query = query.Where(x => x.GolfCourseId == input.GolfCourseId.Value);
            }

            if (input.ApplyDateFrom.HasValue)
            {
                query = query.Where(x => x.ApplyDate >= input.ApplyDateFrom.Value);
            }

            if (input.ApplyDateTo.HasValue)
            {
                query = query.Where(x => x.ApplyDate <= input.ApplyDateTo.Value);
            }

            if (input.PromotionType.HasValue)
            {
                query = query.Where(x => x.PromotionType == input.PromotionType.Value);
            }

            if (input.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == input.IsActive.Value);
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
            .Select(slot => new AppCalendarSlotDto
            {
                Id = slot.Id,
                TenantId = slot.TenantId,
                GolfCourseId = slot.GolfCourseId,
                ApplyDate = slot.ApplyDate,
                TimeFrom = slot.TimeFrom,
                TimeTo = slot.TimeTo,
                PromotionType = slot.PromotionType,
                MaxSlots = slot.MaxSlots,
                InternalNote = slot.InternalNote,
                IsActive = slot.IsActive,
                CreationTime = slot.CreationTime,
                CreatorId = slot.CreatorId,
                LastModificationTime = slot.LastModificationTime,
                LastModifierId = slot.LastModifierId
            })
            .ToList();

            return new PagedResultDto<AppCalendarSlotDto>(totalCount, dtoList);
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

            // Map thủ công sang DTO đầy đủ
            var dto = new AppCalendarSlotDto
            {
                Id = slot.Id,
                TenantId = slot.TenantId,
                GolfCourseId = slot.GolfCourseId,
                GolfCourseName = golf?.Name ?? string.Empty,
                ApplyDate = slot.ApplyDate,
                TimeFrom = slot.TimeFrom,
                TimeTo = slot.TimeTo,
                PromotionType = slot.PromotionType,
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
                    Price = p.Price
                });
            }

            return dto;
        }
    }
}
