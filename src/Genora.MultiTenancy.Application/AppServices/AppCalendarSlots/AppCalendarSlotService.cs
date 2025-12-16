using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Features.AppCalendarSlots;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots;

[Authorize]
public class AppCalendarSlotService :
        FeatureProtectedCrudAppService<
            CalendarSlot,
            AppCalendarSlotDto,
            Guid,
            GetCalendarSlotListInput,
            CreateUpdateAppCalendarSlotDto>,
        IAppCalendarSlotService
{
    protected override string FeatureName => AppCalendarSlotFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppCalendarSlots.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppCalendarSlots.Default;

    private readonly IRepository<CalendarSlotPrice, Guid> _priceRepository;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;
    private readonly IRepository<CustomerType, Guid> _customerTypeRepository;

    public AppCalendarSlotService(
        IRepository<CalendarSlot, Guid> repository,
        IRepository<CalendarSlotPrice, Guid> priceRepository,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        IRepository<CustomerType, Guid> customerTypeRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        _priceRepository = priceRepository;
        _golfCourseRepository = golfCourseRepository;
        _customerTypeRepository = customerTypeRepository;

        GetPolicyName = MultiTenancyPermissions.AppCalendarSlots.Default;
        GetListPolicyName = MultiTenancyPermissions.AppCalendarSlots.Default;
        CreatePolicyName = MultiTenancyPermissions.AppCalendarSlots.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppCalendarSlots.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppCalendarSlots.Delete;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<AppCalendarSlotDto>> GetListAsync(GetCalendarSlotListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        var query = queryable;

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

        // Nếu màn list chỉ cần dữ liệu cơ bản (không cần Prices / GolfCourseName)
        // ta map đơn giản:
        var dtoList = slots
            .Select(slot => new AppCalendarSlotDto
            {
                Id = slot.Id,
                TenantId = slot.TenantId,
                GolfCourseId = slot.GolfCourseId,
                // GolfCourseName có thể để trống hoặc lấy thêm sau nếu cần
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

    public async Task<List<AppCalendarSlotDto>> GetByDateAsync(GetCalendarSlotByDateInput input)
    {
        await CheckGetListPolicyAsync();

        // 1) Lấy list slot theo ngày + sân
        var slotQuery = await Repository.GetQueryableAsync();

        var slots = await AsyncExecuter.ToListAsync(
            slotQuery
                .Where(x =>
                    x.GolfCourseId == input.GolfCourseId &&
                    x.ApplyDate.Date == input.ApplyDate.Date)
                .OrderBy(x => x.TimeFrom)
        );

        if (!slots.Any())
        {
            return new List<AppCalendarSlotDto>();
        }

        var slotIds = slots.Select(s => s.Id).ToList();
        var golfCourseIds = slots.Select(s => s.GolfCourseId).Distinct().ToList();

        // 2) Lấy sân
        var golfCourses = await _golfCourseRepository.GetListAsync(
            x => golfCourseIds.Contains(x.Id)
        );
        var golfDict = golfCourses.ToDictionary(x => x.Id, x => x.Name);

        // 3) Lấy giá theo slot
        var prices = await _priceRepository.GetListAsync(
            x => slotIds.Contains(x.CalendarSlotId)
        );

        var customerTypeIds = prices.Select(p => p.CustomerTypeId).Distinct().ToList();
        var customerTypes = await _customerTypeRepository.GetListAsync(
            x => customerTypeIds.Contains(x.Id)
        );
        var ctDict = customerTypes.ToDictionary(x => x.Id, x => x);

        // 4) Map thủ công sang DTO (không cần Include)
        var result = new List<AppCalendarSlotDto>();

        foreach (var slot in slots)
        {
            var dto = new AppCalendarSlotDto
            {
                Id = slot.Id,
                TenantId = slot.TenantId,
                GolfCourseId = slot.GolfCourseId,
                GolfCourseName = golfDict.TryGetValue(slot.GolfCourseId, out var gcName) ? gcName : string.Empty,
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

            var slotPrices = prices.Where(p => p.CalendarSlotId == slot.Id);

            foreach (var p in slotPrices)
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

            result.Add(dto);
        }

        return result;
    }

    public override async Task<AppCalendarSlotDto> CreateAsync(CreateUpdateAppCalendarSlotDto input)
    {
        await CheckCreatePolicyAsync();

        if (input.GolfCourseId == Guid.Empty)
        {
            throw new BusinessException("CalendarSlot:MissingGolfCourse")
                .WithData("Message", "GolfCourseId is empty when creating slot.");
        }

        // validate trùng khung giờ trong cùng ngày cùng sân
        await EnsureNoOverlapAsync(input);

        var entity = new CalendarSlot(
            GuidGenerator.Create(),
            input.GolfCourseId,
            input.ApplyDate,
            input.TimeFrom,
            input.TimeTo
        )
        {
            PromotionType = input.PromotionType,
            MaxSlots = input.MaxSlots,
            InternalNote = input.InternalNote,
            IsActive = input.IsActive
        };

        entity = await Repository.InsertAsync(entity, autoSave: true);

        // Prices
        await SavePricesAsync(entity.Id, input.Prices);

        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = await GetAsync(entity.Id);
        return dto;
    }

    public override async Task<AppCalendarSlotDto> UpdateAsync(Guid id, CreateUpdateAppCalendarSlotDto input)
    {
        await CheckUpdatePolicyAsync();

        await EnsureNoOverlapAsync(input, id);

        var entity = await Repository.GetAsync(id);

        // Map tay các field cho rõ ràng
        entity.GolfCourseId = input.GolfCourseId;
        entity.ApplyDate = input.ApplyDate;
        entity.TimeFrom = input.TimeFrom;
        entity.TimeTo = input.TimeTo;
        entity.PromotionType = input.PromotionType;
        entity.MaxSlots = input.MaxSlots;
        entity.InternalNote = input.InternalNote;
        entity.IsActive = input.IsActive;

        entity = await Repository.UpdateAsync(entity, autoSave: true);

        await SavePricesAsync(entity.Id, input.Prices);
        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = await GetAsync(entity.Id);
        return dto;
    }

    public override async Task<AppCalendarSlotDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var slot = await Repository.FindAsync(id);
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

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        // xóa prices trước
        await _priceRepository.DeleteAsync(x => x.CalendarSlotId == id);
        await Repository.DeleteAsync(id);
    }

    private async Task EnsureNoOverlapAsync(CreateUpdateAppCalendarSlotDto input, Guid? currentId = null)
    {
        var queryable = await Repository.GetQueryableAsync();
        var query = queryable.Where(x =>
            x.GolfCourseId == input.GolfCourseId &&
            x.ApplyDate.Date == input.ApplyDate.Date &&
            (!currentId.HasValue || x.Id != currentId.Value));

        // overlap: [from, to] giao nhau
        query = query.Where(x =>
            (input.TimeFrom < x.TimeTo) && (input.TimeTo > x.TimeFrom));

        var exists = await AsyncExecuter.AnyAsync(query);
        if (exists)
        {
            throw new BusinessException("CalendarSlot:Overlap")
                .WithData("ApplyDate", input.ApplyDate.ToString("yyyy-MM-dd"))
                .WithData("TimeFrom", input.TimeFrom)
                .WithData("TimeTo", input.TimeTo);
        }
    }

    private async Task SavePricesAsync(Guid calendarSlotId, List<CreateUpdateCalendarSlotPriceDto> inputPrices)
    {
        // normalize input + remove duplicates CustomerTypeId
        var normalized = (inputPrices ?? new List<CreateUpdateCalendarSlotPriceDto>())
            .Where(x => x.CustomerTypeId != Guid.Empty)
            .GroupBy(x => x.CustomerTypeId)
            .Select(g => g.Last())
            .ToList();

        // NOTE:
        // - Host: CurrentTenant.Id == null
        // - Tenant: CurrentTenant.Id != null
        // Ta phải lấy đúng existing prices theo slotId (kể cả TenantId null)
        //using (CurrentUnitOfWork?.DisableFilter(Volo.Abp.Data.AbpDataFilters.MayHaveTenant))
        //using (CurrentUnitOfWork?.DisableFilter(Volo.Abp.Data.AbpDataFilters.MustHaveTenant))
        //{
            var existing = await _priceRepository.GetListAsync(x => x.CalendarSlotId == calendarSlotId);

            // 1) Delete các dòng DB không còn trong input
            var inputCtIds = normalized.Select(x => x.CustomerTypeId).ToHashSet();
            var toDelete = existing.Where(x => !inputCtIds.Contains(x.CustomerTypeId)).ToList();
            if (toDelete.Count > 0)
            {
                await _priceRepository.DeleteManyAsync(toDelete, autoSave: true);
            }

            // 2) Upsert từng dòng
            foreach (var p in normalized)
            {
                var row = existing.FirstOrDefault(x => x.CustomerTypeId == p.CustomerTypeId);

                if (row == null)
                {
                    var newRow = new CalendarSlotPrice(
                        GuidGenerator.Create(),
                        calendarSlotId,
                        p.CustomerTypeId,
                        p.Price
                    );

                    // đồng bộ tenant id:
                    // - Tenant: set tenant id
                    // - Host: để null
                    newRow.TenantId = CurrentTenant.Id;

                    await _priceRepository.InsertAsync(newRow, autoSave: true);
                }
                else
                {
                    row.Price = p.Price;

                    // đảm bảo tenant id đúng side
                    row.TenantId = CurrentTenant.Id;

                    await _priceRepository.UpdateAsync(row, autoSave: true);
                }
            }
        //}
    }

}