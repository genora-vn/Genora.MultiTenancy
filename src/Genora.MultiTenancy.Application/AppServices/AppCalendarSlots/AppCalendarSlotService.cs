using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using Genora.MultiTenancy.Features.AppCalendarSlots;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Data;
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
    private readonly IRepository<PromotionType, Guid> _promotionType;
    private readonly IRepository<SpecialDate, Guid> _specialDateRepository;
    private readonly AppCalendarExcelTemplateGenerator _generator;
    private readonly IDataFilter _dataFilter;

    public AppCalendarSlotService(
        IRepository<CalendarSlot, Guid> repository,
        IRepository<CalendarSlotPrice, Guid> priceRepository,
        IRepository<GolfCourse, Guid> golfCourseRepository,
        IRepository<CustomerType, Guid> customerTypeRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        AppCalendarExcelTemplateGenerator generator,
        IRepository<PromotionType, Guid> promotionType,
        IRepository<SpecialDate, Guid> specialDateRepository,
        IDataFilter dataFilter)
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

        _generator = generator;
        _promotionType = promotionType;
        _specialDateRepository = specialDateRepository;
        _dataFilter = dataFilter;
    }

    /// <summary>
    /// Get dữ liệu từ bảng AppSpecialDates thông qua golfCourseId
    /// </summary>
    /// <param name="golfCourseId">Mã sân</param>
    /// <returns></returns>
    private async Task<List<SpecialDate>> GetSpecialDatesForCourseAsync(Guid golfCourseId)
    {
        // lấy global + per-course
        var list = await _specialDateRepository.GetListAsync(x =>
            x.IsActive &&
            (x.GolfCourseId == null || x.GolfCourseId == golfCourseId));

        return list ?? new List<SpecialDate>();
    }

    /// <summary>
    /// Lấy danh sách loại ngày ra, mặc định cấu hình: Ngày trong tuần, ngày cuối tuần, ngày lễ,.. hoặc ngày đặc biệt khác để đổ vào dropdown import khung giờ chơi
    /// </summary>
    /// <param name="specialDates">List specialDates</param>
    /// <returns></returns>
    private static List<string> GetDayTypesFromSpecialDates(List<SpecialDate> specialDates)
    {
        var names = (specialDates ?? new List<SpecialDate>())
            .Where(x => x.IsActive)
            .Select(x => (x.Name ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        if (names.Count == 0)
            names = new List<string> { "Ngày trong tuần", "Ngày cuối tuần", "Ngày lễ" };

        return names;
    }

    /// <summary>
    /// Validate dữ liệu loại ngày
    /// </summary>
    /// <param name="inputDayType"></param>
    /// <param name="allowedDayTypes"></param>
    /// <param name="rowNumber"></param>
    /// <exception cref="UserFriendlyException"></exception>
    private static void ValidateDayTypeAllowed(string? inputDayType, List<string> allowedDayTypes, int rowNumber)
    {
        var s = (inputDayType ?? "").Trim();
        if (string.IsNullOrWhiteSpace(s))
            throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: DayType (Loại ngày) là bắt buộc");

        // match ignore-case
        var ok = allowedDayTypes.Any(x => string.Equals(x, s, StringComparison.OrdinalIgnoreCase));
        if (!ok)
        {
            throw new UserFriendlyException(
                "Import Excel lỗi",
                $"Dòng {rowNumber}: DayType '{s}' không hợp lệ. Cho phép: {string.Join(", ", allowedDayTypes)}"
            );
        }
    }

    /// <summary>
    ///  Record có DatesJson (danh sách ngày) => dùng để match "Ngày lễ"
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private static bool LooksLikeHolidayConfig(SpecialDate x)
    {
        return x != null && !string.IsNullOrWhiteSpace(x.DatesJson);
    }

    /// <summary>
    /// Lấy ra danh sách cấu hình ngày lễ
    /// </summary>
    /// <param name="specialDates"></param>
    /// <returns></returns>
    private static HashSet<DateTime> ParseHolidaySet(List<SpecialDate> specialDates)
    {
        var set = new HashSet<DateTime>();

        var holidayConfigs = (specialDates ?? new List<SpecialDate>())
            .Where(LooksLikeHolidayConfig)
            .ToList();

        foreach (var cfg in holidayConfigs)
        {
            foreach (var d in ParseDatesJson(cfg.DatesJson!))
            {
                set.Add(d.Date);
            }
        }

        return set;
    }

    /// <summary>
    /// Format cấu hình ngày lễ
    /// </summary>
    /// <param name="specialDates"></param>
    /// <returns></returns>
    private static string ResolveHolidayName(List<SpecialDate> specialDates)
    {
        var x = (specialDates ?? new List<SpecialDate>())
            .FirstOrDefault(LooksLikeHolidayConfig);

        var name = (x?.Name ?? "").Trim();
        return string.IsNullOrWhiteSpace(name) ? "Ngày lễ" : name;
    }

    /// <summary>
    /// Xử lý lấy ra đúng cấu hình cuối tuần
    /// </summary>
    /// <param name="dayTypes"></param>
    /// <returns></returns>
    private static string GuessWeekendName(List<string> dayTypes)
    {
        var wk = dayTypes.FirstOrDefault(x =>
            x.Contains("cuối tuần", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("weekend", StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrWhiteSpace(wk) ? "Cuối tuần" : wk;
    }

    /// <summary>
    /// Xử lý lấy ra đúng cấu hình trong tuần
    /// </summary>
    /// <param name="dayTypes"></param>
    /// <returns></returns>
    private static string GuessWeekdayName(List<string> dayTypes)
    {
        var wd = dayTypes.FirstOrDefault(x =>
            x.Contains("trong tuần", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("weekday", StringComparison.OrdinalIgnoreCase));

        // nếu không có thì lấy "dayTypes" đầu tiên khác weekend/holiday
        if (string.IsNullOrWhiteSpace(wd))
        {
            wd = dayTypes.FirstOrDefault(x =>
                !x.Contains("cuối tuần", StringComparison.OrdinalIgnoreCase) &&
                !x.Contains("weekend", StringComparison.OrdinalIgnoreCase) &&
                !x.Contains("lễ", StringComparison.OrdinalIgnoreCase) &&
                !x.Contains("holiday", StringComparison.OrdinalIgnoreCase));
        }

        return string.IsNullOrWhiteSpace(wd) ? "Trong tuần" : wd;
    }

    /// <summary>
    /// Check các cấu hình trong tuần, cuối tuần và ngày lễ và lấy ra tên cấu hình
    /// </summary>
    /// <param name="applyDate"></param>
    /// <param name="holidaySet"></param>
    /// <param name="allowedDayTypes"></param>
    /// <param name="holidayName"></param>
    /// <returns></returns>
    private static string ResolveDayTypeNameForExport(
        DateTime applyDate,
        HashSet<DateTime> holidaySet,
        List<string> allowedDayTypes,
        string holidayName)
    {
        var isHoliday = holidaySet.Contains(applyDate.Date);
        var isWeekend = applyDate.DayOfWeek == DayOfWeek.Saturday || applyDate.DayOfWeek == DayOfWeek.Sunday;

        if (isHoliday)
            return holidayName;

        if (isWeekend)
            return GuessWeekendName(allowedDayTypes);

        return GuessWeekdayName(allowedDayTypes);
    }

    /// <summary>
    /// Định dạng cấu hình ngày lễ kiểu date linh hoạt - "yyyy-MM-dd" hoặc ISO -> parse
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private static List<DateTime> ParseDatesJson(string json)
    {
        try
        {
            var arr = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            var list = new List<DateTime>();

            foreach (var s in arr)
            {
                if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                    list.Add(d.Date);
                else if (DateTime.TryParse(s, out d))
                    list.Add(d.Date);
            }

            return list.Distinct().OrderBy(x => x).ToList();
        }
        catch
        {
            return new List<DateTime>();
        }
    }

    /// <summary>
    /// Lấy danh sách khung giờ chơi
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisableValidation]
    public override async Task<PagedResultDto<AppCalendarSlotDto>> GetListAsync(GetCalendarSlotListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var promotion = await _promotionType.GetListAsync();
        var query = queryable;

        if (input.GolfCourseId.HasValue)
            query = query.Where(x => x.GolfCourseId == input.GolfCourseId.Value);

        if (input.ApplyDateFrom.HasValue)
            query = query.Where(x => x.ApplyDate >= input.ApplyDateFrom.Value);

        if (input.ApplyDateTo.HasValue)
            query = query.Where(x => x.ApplyDate <= input.ApplyDateTo.Value);

        if (input.PromotionType.HasValue)
            query = query.Where(x => x.PromotionTypeId == input.PromotionType.Value);

        if (input.IsActive.HasValue)
            query = query.Where(x => x.IsActive == input.IsActive.Value);

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
                PromotionTypeId = slot.PromotionTypeId,
                PromotionType = promotion.FirstOrDefault(p => p.Id == slot.PromotionTypeId)?.Name,
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

    /// <summary>
    /// Lấy danh sách khung giờ chơi theo bộ lọc thời gian
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<AppCalendarSlotDto>> GetByDateAsync(GetCalendarSlotByDateInput input)
    {
        await CheckGetListPolicyAsync();

        var slotQuery = await Repository.GetQueryableAsync();
        var promotions = await _promotionType.GetListAsync();

        if (input.GolfCourseId == Guid.Empty)
        {
            throw new BusinessException("CalendarSlot:MissingGolfCourse")
                .WithData("Message", "GolfCourseId is empty when getting slots by date.");
        }

        slotQuery = slotQuery.Where(x => x.GolfCourseId == input.GolfCourseId);

        if (input.ApplyDateFrom.HasValue)
            slotQuery = slotQuery.Where(x => x.ApplyDate.Date >= input.ApplyDateFrom.Value.Date);

        if (input.ApplyDateTo.HasValue)
            slotQuery = slotQuery.Where(x => x.ApplyDate.Date <= input.ApplyDateTo.Value.Date);

        var slots = await AsyncExecuter.ToListAsync(slotQuery.OrderBy(x => x.TimeFrom));

        if (!slots.Any())
            return new List<AppCalendarSlotDto>();

        var slotIds = slots.Select(s => s.Id).ToList();
        var golfCourseIds = slots.Select(s => s.GolfCourseId).Distinct().ToList();

        var golfCourses = await _golfCourseRepository.GetListAsync(x => golfCourseIds.Contains(x.Id));
        var golfDict = golfCourses.ToDictionary(x => x.Id, x => x.Name);

        var prices = await _priceRepository.GetListAsync(x => slotIds.Contains(x.CalendarSlotId));

        var customerTypeIds = prices.Select(p => p.CustomerTypeId).Distinct().ToList();
        var customerTypes = await _customerTypeRepository.GetListAsync(x => customerTypeIds.Contains(x.Id));
        var ctDict = customerTypes.ToDictionary(x => x.Id, x => x);

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
                PromotionTypeId = slot.PromotionTypeId,
                PromotionType = promotions.FirstOrDefault(p => p.Id == slot.PromotionTypeId)?.Name,
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
                    CustomerTypeCode = ct.Code,
                    CustomerTypeName = ct.Name,
                    Price9 = p.Price9,
                    Price18 = p.Price18,
                    Price27 = p.Price27,
                    Price36 = p.Price36
                });
            }

            result.Add(dto);
        }

        return result;
    }

    /// <summary>
    /// Tạo khung giờ chơi
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public override async Task<AppCalendarSlotDto> CreateAsync(CreateUpdateAppCalendarSlotDto input)
    {
        await CheckCreatePolicyAsync();

        if (input.GolfCourseId == Guid.Empty)
        {
            throw new BusinessException("CalendarSlot:MissingGolfCourse")
                .WithData("Message", "GolfCourseId is empty when creating slot.");
        }

        await EnsureNoOverlapAsync(input);

        var entity = new CalendarSlot(
            GuidGenerator.Create(),
            input.GolfCourseId,
            input.ApplyDate,
            input.TimeFrom,
            input.TimeTo
        )
        {
            PromotionTypeId = input.PromotionTypeId,
            MaxSlots = input.MaxSlots,
            InternalNote = input.InternalNote,
            IsActive = input.IsActive
        };

        entity = await Repository.InsertAsync(entity, autoSave: true);

        await SavePricesAsync(entity.Id, input.Prices);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(entity.Id);
    }

    /// <summary>
    /// Cập nhật thông tin khung giờ chơi
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public override async Task<AppCalendarSlotDto> UpdateAsync(Guid id, CreateUpdateAppCalendarSlotDto input)
    {
        await CheckUpdatePolicyAsync();

        await EnsureNoOverlapAsync(input, id);

        var entity = await Repository.GetAsync(id);

        entity.GolfCourseId = input.GolfCourseId;
        entity.ApplyDate = input.ApplyDate;
        entity.TimeFrom = input.TimeFrom;
        entity.TimeTo = input.TimeTo;
        entity.PromotionTypeId = input.PromotionTypeId;
        entity.MaxSlots = input.MaxSlots;
        entity.InternalNote = input.InternalNote;
        entity.IsActive = input.IsActive;

        entity = await Repository.UpdateAsync(entity, autoSave: true);

        await SavePricesAsync(entity.Id, input.Prices);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(entity.Id);
    }

    public async Task<int> UpdateStatusBulkAsync(UpdateCalendarSlotStatusBulkInput input)
    {
        await CheckUpdatePolicyAsync();

        var ids = (input.Ids ?? new List<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            throw new UserFriendlyException("Cập nhật trạng thái lỗi", "Vui lòng chọn ít nhất 1 dòng để cập nhật.");

        var slots = await Repository.GetListAsync(x => ids.Contains(x.Id));

        if (slots.Count == 0)
            return 0;

        foreach (var s in slots)
        {
            s.IsActive = input.IsActive;
        }

        await Repository.UpdateManyAsync(slots, autoSave: false);
        await CurrentUnitOfWork.SaveChangesAsync();

        return slots.Count;
    }

    /// <summary>
    /// Lấy chi tiết khung giờ chơi theo Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException"></exception>
    public override async Task<AppCalendarSlotDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var slot = await Repository.FindAsync(id);
        if (slot == null)
            throw new EntityNotFoundException(typeof(CalendarSlot), id);

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
            InternalNote = slot.InternalNote,
            IsActive = slot.IsActive,
            CreationTime = slot.CreationTime,
            CreatorId = slot.CreatorId,
            LastModificationTime = slot.LastModificationTime,
            LastModifierId = slot.LastModifierId,
            Prices = new List<AppCalendarSlotPriceDto>()
        };

        var promotion = await _promotionType.FirstOrDefaultAsync(p => p.Id == slot.PromotionTypeId);
        if (promotion != null)
            dto.PromotionType = promotion.Name;

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
                Price9 = p.Price9,
                Price18 = p.Price18,
                Price27 = p.Price27,
                Price36 = p.Price36
            });
        }

        return dto;
    }

    /// <summary>
    /// Xóa khung giờ chơi cấu hình theo Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        await _priceRepository.DeleteAsync(x => x.CalendarSlotId == id);
        await Repository.DeleteAsync(id);
    }

    /// <summary>
    /// Xử lý kiểm tra dữ liệu và lọc khung giờ chơi
    /// </summary>
    /// <param name="input"></param>
    /// <param name="currentId"></param>
    /// <returns></returns>
    private async Task EnsureNoOverlapAsync(CreateUpdateAppCalendarSlotDto input, Guid? currentId = null)
    {
        var queryable = await Repository.GetQueryableAsync();
        var query = queryable.Where(x =>
            x.GolfCourseId == input.GolfCourseId &&
            x.ApplyDate.Date == input.ApplyDate.Date &&
            (!currentId.HasValue || x.Id != currentId.Value));

        query = query.Where(x => (input.TimeFrom < x.TimeTo) && (input.TimeTo > x.TimeFrom));

        var exists = await AsyncExecuter.AnyAsync(query);
        if (exists)
        {
            throw new BusinessException("CalendarSlot:Overlap")
                .WithData("ApplyDate", input.ApplyDate.ToString("yyyy-MM-dd"))
                .WithData("TimeFrom", input.TimeFrom)
                .WithData("TimeTo", input.TimeTo);
        }
    }

    /// <summary>
    /// Lưu dữ liệu cấu hình giá
    /// </summary>
    /// <param name="calendarSlotId"></param>
    /// <param name="inputPrices"></param>
    /// <returns></returns>
    private async Task SavePricesAsync(Guid calendarSlotId, List<CreateUpdateCalendarSlotPriceDto> inputPrices)
    {
        var normalized = (inputPrices ?? new List<CreateUpdateCalendarSlotPriceDto>())
            .Where(x => x.CustomerTypeId != Guid.Empty)
            .GroupBy(x => x.CustomerTypeId)
            .Select(g => g.Last())
            .ToList();

        var existing = await _priceRepository.GetListAsync(x => x.CalendarSlotId == calendarSlotId);

        var inputCtIds = normalized.Select(x => x.CustomerTypeId).ToHashSet();
        var toDelete = existing.Where(x => !inputCtIds.Contains(x.CustomerTypeId)).ToList();
        if (toDelete.Count > 0)
        {
            await _priceRepository.DeleteManyAsync(toDelete, autoSave: true);
        }

        foreach (var p in normalized)
        {
            var row = existing.FirstOrDefault(x => x.CustomerTypeId == p.CustomerTypeId);

            if (row == null)
            {
                var newRow = new CalendarSlotPrice(
                    GuidGenerator.Create(),
                    calendarSlotId,
                    p.CustomerTypeId,
                    p.Price9,
                    p.Price18,
                    p.Price27,
                    p.Price36
                );
                newRow.TenantId = CurrentTenant.Id;
                await _priceRepository.InsertAsync(newRow, autoSave: true);
            }
            else
            {
                row.Price9 = p.Price9;
                row.Price18 = p.Price18;
                row.Price27 = p.Price27;
                row.Price36 = p.Price36;
                row.TenantId = CurrentTenant.Id;
                await _priceRepository.UpdateAsync(row, autoSave: true);
            }
        }
    }

    /// <summary>
    /// Tải file mẫu excel import khung giờ chơi (trước đây mặc định khi không truyền golfCourseId vào)
    /// </summary>
    /// <returns></returns>
    public Task<IRemoteStreamContent> DownloadTemplateAsync()
    {
        // Template export (file trống) -> dùng format import hướng 2
        var rows = new List<AppCalendarSlotExcelRowDto>();
        var exporter = new AppCalendarExcelExporter();
        var customerTypes = new List<CustomerType>();
        return Task.FromResult(exporter.Export(rows, customerTypes));
    }

    /// <summary>
    /// Xuất excel khung giờ chơi (đang ẩn đi trên front end)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<IRemoteStreamContent> ExportExcelAsync(GetCalendarSlotListInput input)
    {
        await CheckGetListPolicyAsync();

        var exporter = new AppCalendarExcelExporter();
        var query = await Repository.GetQueryableAsync();
        var promotions = await _promotionType.GetListAsync();

        if (input.GolfCourseId != Guid.Empty)
            query = query.Where(x => x.GolfCourseId == input.GolfCourseId);

        if (input.ApplyDateFrom.HasValue)
            query = query.Where(x => x.ApplyDate >= input.ApplyDateFrom.Value);

        if (input.ApplyDateTo.HasValue)
            query = query.Where(x => x.ApplyDate <= input.ApplyDateTo.Value);

        var list = await AsyncExecuter.ToListAsync(query);

        var golfCourseIds = list.Select(g => g.GolfCourseId).Distinct().ToList();
        var golfCourses = await _golfCourseRepository.GetListAsync(gc => golfCourseIds.Contains(gc.Id));

        var customerTypes = await _customerTypeRepository.GetListAsync();
        customerTypes = customerTypes.OrderBy(t => t.CreationTime).ToList();

        // Prices
        var slotIds = list.Select(x => x.Id).ToList();
        var prices = await _priceRepository.GetListAsync(p => slotIds.Contains(p.CalendarSlotId));
        var ctNameById = customerTypes.ToDictionary(x => x.Id, x => x.Name);

        var priceBySlot = prices
            .GroupBy(p => p.CalendarSlotId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var items = new List<CustomerTypeExcelRowDto>();
                    foreach (var p in g)
                    {
                        if (!ctNameById.TryGetValue(p.CustomerTypeId, out var ctName)) continue;

                        items.Add(new CustomerTypeExcelRowDto
                        {
                            CustomerType = ctName,
                            Price9 = p.Price9,
                            Price18 = p.Price18,
                            Price27 = p.Price27,
                            Price36 = p.Price36
                        });
                    }
                    return items;
                });

        // SpecialDates cache per golfCourseId => dayTypes + holidaySet + holidayName
        var specCache = new Dictionary<Guid, (List<string> DayTypes, HashSet<DateTime> HolidaySet, string HolidayName)>();

        async Task<(List<string> DayTypes, HashSet<DateTime> HolidaySet, string HolidayName)> GetSpecAsync(Guid golfCourseId)
        {
            if (specCache.TryGetValue(golfCourseId, out var v)) return v;

            var spec = await GetSpecialDatesForCourseAsync(golfCourseId);
            var dayTypes = GetDayTypesFromSpecialDates(spec);
            var holidaySet = ParseHolidaySet(spec);
            var holidayName = ResolveHolidayName(spec);

            v = (dayTypes, holidaySet, holidayName);
            specCache[golfCourseId] = v;
            return v;
        }

        var rows = new List<AppCalendarSlotExcelRowDto>();

        foreach (var b in list)
        {
            priceBySlot.TryGetValue(b.Id, out var ctPrices);

            var gc = golfCourses.FirstOrDefault(g => g.Id == b.GolfCourseId);
            var (dayTypes, holidaySet, holidayName) = await GetSpecAsync(b.GolfCourseId);

            rows.Add(new AppCalendarSlotExcelRowDto
            {
                GolfCourseCode = gc?.Code,
                GolfCourseName = gc?.Name,

                DayType = ResolveDayTypeNameForExport(b.ApplyDate.Date, holidaySet, dayTypes, holidayName),

                // Export format: FromDate/ToDate là date range
                FromDate = b.ApplyDate,
                ToDate = b.ApplyDate,

                StartTime = b.TimeFrom,
                EndTime = b.TimeTo,
                MaxSlots = b.MaxSlots,
                PromotionType = promotions.FirstOrDefault(p => p.Id == b.PromotionTypeId)?.Name ?? "",
                InternalNote = b.InternalNote ?? "",
                Gap = 0,
                CustomerTypePrice = ctPrices ?? new List<CustomerTypeExcelRowDto>()
            });
        }

        // Nếu export theo 1 sân => lấy đúng dayTypes sân đó
        List<string> dayTypesHint;

        if (input.GolfCourseId.HasValue && input.GolfCourseId.Value != Guid.Empty)
        {
            var spec = await GetSpecAsync(input.GolfCourseId.Value);
            dayTypesHint = spec.DayTypes;
        }
        else
        {
            var globalSpec = await _specialDateRepository.GetListAsync(x => x.IsActive && x.GolfCourseId == null);
            dayTypesHint = GetDayTypesFromSpecialDates(globalSpec);
        }

        return exporter.Export(rows, customerTypes, dayTypesHint);
    }

    /// <summary>
    /// Thay bằng hàm download file mẫu khi truyền GolfCOurseId vào
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync(Guid? golfCourseId)
    {
        var customerTypes = await _customerTypeRepository.GetListAsync();
        var promotions = await _promotionType.GetListAsync();

        List<SpecialDate> specialDates;

        // Nếu có chọn sân => lấy config theo sân
        if (golfCourseId.HasValue == true && golfCourseId.Value != Guid.Empty)
        {
            specialDates = await GetSpecialDatesForCourseAsync(golfCourseId.Value);
        }
        else
        {
            specialDates = await _specialDateRepository.GetListAsync(x => x.IsActive && x.GolfCourseId == null);
        }

        var template = _generator.GenerateTemplate(
            customerTypes.OrderBy(t => t.CreationTime).ToList(),
            promotions,
            specialDates
        );

        return template;
    }


    // =========================
    // IMPORT: validate DayType by SpecialDates + apply rule weekday/weekend/holiday
    // =========================

    /// <summary>
    /// Import excel cấu hình khung giờ chơi
    /// Validate DayType theo SpecialDates
    /// Áp dụng rule weekday/weekend/holiday
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    public async Task<int> ImportExcelAsync(ImportCalendarExcelInput input)
    {
        await CheckUpdatePolicyAsync();

        var importer = new AppCalendarExcelImporter();
        var customerTypes = await _customerTypeRepository.GetListAsync();
        var golfCourses = await _golfCourseRepository.GetListAsync();
        var promotions = await _promotionType.GetListAsync();

        using var stream = input.File.GetStream();
        var orderedCustomerTypes = customerTypes.OrderBy(t => t.CreationTime).ToList();
        var rows = importer.Read(stream, orderedCustomerTypes);

        var golfByCode = golfCourses
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .ToDictionary(x => x.Code.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

        var promoByName = promotions
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(x => x.Name.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

        var ctByName = orderedCustomerTypes
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(x => x.Name.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

        var specCache = new Dictionary<Guid, (List<string> DayTypes, HashSet<DateTime> HolidaySet, string HolidayName)>();

        async Task<(List<string> DayTypes, HashSet<DateTime> HolidaySet, string HolidayName)> GetSpecAsync(Guid golfCourseId)
        {
            if (specCache.TryGetValue(golfCourseId, out var v)) return v;

            var spec = await GetSpecialDatesForCourseAsync(golfCourseId);
            var dayTypes = GetDayTypesFromSpecialDates(spec);
            var holidaySet = ParseHolidaySet(spec);
            var holidayName = ResolveHolidayName(spec);

            v = (dayTypes, holidaySet, holidayName);
            specCache[golfCourseId] = v;
            return v;
        }

        // ====== CHECK VÀ XỬ LÝ TRÙNG CẤU HÌNH KHUNG GIỜ CHƠI ======
        // key: CourseId|ApplyDate|TimeFrom|TimeTo
        var slotKeySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pendingInsertByKey = new Dictionary<string, CalendarSlot>(StringComparer.OrdinalIgnoreCase);

        // ====== CHECK VÀ XỬ LÝ TRÙNG CẤU HÌNH GIÁ TRÙNG LẶP======
        // key: SlotId|CustomerTypeId
        var pendingPriceByKey = new Dictionary<string, CalendarSlotPrice>(StringComparer.OrdinalIgnoreCase);

        // Bộ nhớ đệm về giá để tránh việc đọc cơ sở dữ liệu lặp đi lặp lại
        var priceCacheBySlotId = new Dictionary<Guid, List<CalendarSlotPrice>>();

        async Task<List<CalendarSlotPrice>> GetPricesOfSlotAsync(Guid slotId)
        {
            if (priceCacheBySlotId.TryGetValue(slotId, out var cached)) return cached;
            var list = await _priceRepository.GetListAsync(x => x.CalendarSlotId == slotId);
            priceCacheBySlotId[slotId] = list;
            return list;
        }

        static string MakeSlotKey(Guid courseId, DateTime applyDate, TimeSpan timeFrom, TimeSpan timeTo)
            => $"{courseId:N}|{applyDate:yyyyMMdd}|{timeFrom:c}|{timeTo:c}";

        static string MakePriceKey(Guid slotId, Guid customerTypeId)
            => $"{slotId:N}|{customerTypeId:N}";

        // ====== VÒNG LẶP XỬ LÝ TỪNG BẢN GHI ======
        foreach (var item in rows)
        {
            var rowNumber = item.Row;
            var r = item.Data;

            // Validate các dòng dữ liệu
            if (string.IsNullOrWhiteSpace(r.GolfCourseCode))
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: GolfCourseCode là bắt buộc");

            if (r.FromDate == DateTime.MinValue)
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: FromDate là bắt buộc");

            if (r.ToDate == DateTime.MinValue)
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: ToDate là bắt buộc");

            if (r.StartTime == TimeSpan.Zero)
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: StartTime là bắt buộc");

            if (r.EndTime == TimeSpan.Zero)
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: EndTime là bắt buộc");

            if (r.MaxSlots <= 0)
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: MaxSlots phải > 0");

            if (r.Gap <= 0)
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: Gap phải > 0");

            if (!promoByName.TryGetValue((r.PromotionType ?? "").Trim(), out var promotion))
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: PromotionType không hợp lệ");

            if (!golfByCode.TryGetValue(r.GolfCourseCode.Trim(), out var golfCourse))
                throw new UserFriendlyException("Import Excel lỗi", $"Dòng {rowNumber}: Mã sân {r.GolfCourseCode} không tìm thấy");

            // Lấy ra cấu hình loại ngày
            var (allowedDayTypes, holidaySet, holidayName) = await GetSpecAsync(golfCourse.Id);

            ValidateDayTypeAllowed(r.DayType, allowedDayTypes, rowNumber);

            var selectedDayType = allowedDayTypes.First(x =>
                string.Equals(x, r.DayType?.Trim(), StringComparison.OrdinalIgnoreCase));

            var weekendName = GuessWeekendName(allowedDayTypes);
            var weekdayName = GuessWeekdayName(allowedDayTypes);

            var totalDays = (r.ToDate.Date - r.FromDate.Date).TotalDays;
            var totalSlots = (int)((r.EndTime - r.StartTime).TotalMinutes / r.Gap);
            if (totalSlots <= 0) continue;

            // Lấy dữ liệu cấu hình khung giờ đã tồn tại
            var calendars = await Repository.GetListAsync(
                x => x.GolfCourseId == golfCourse.Id &&
                     x.ApplyDate >= r.FromDate.Date &&
                     x.ApplyDate <= r.ToDate.Date
            );

            for (int i = 0; i <= totalDays; i++)
            {
                var applyDate = r.FromDate.Date.AddDays(i);

                var isHoliday = holidaySet.Contains(applyDate);
                var isWeekend = applyDate.DayOfWeek == DayOfWeek.Saturday || applyDate.DayOfWeek == DayOfWeek.Sunday;
                var isWeekday = !isWeekend;

                // Ghi đè các ngày lễ
                bool pass = selectedDayType.Equals(holidayName, StringComparison.OrdinalIgnoreCase)
                    ? isHoliday
                    : selectedDayType.Equals(weekendName, StringComparison.OrdinalIgnoreCase)
                        ? (!isHoliday && isWeekend)
                        : (!isHoliday && isWeekday);

                if (!pass) continue;

                for (int j = 0; j < totalSlots; j++)
                {
                    var timeFrom = r.StartTime.Add(TimeSpan.FromMinutes(r.Gap * j));
                    var timeTo = timeFrom.Add(TimeSpan.FromMinutes(r.Gap));

                    var slotKey = MakeSlotKey(golfCourse.Id, applyDate, timeFrom, timeTo);

                    // xử lý trùng lặp dữ liệu
                    if (pendingInsertByKey.TryGetValue(slotKey, out var pending))
                    {
                        // Cập nhật dữ liệu
                        pending.PromotionTypeId = promotion.Id;
                        pending.MaxSlots = r.MaxSlots;
                        pending.InternalNote = r.InternalNote;
                        pending.IsActive = true;

                        // Cập nhật giá cho vị trí đang chờ xử lý trong bộ nhớ
                        UpsertPendingPrices(pending.Id, r.CustomerTypePrice);

                        continue;
                    }

                    // Kiểm tra tồn tại trong DB
                    var existingCalendar = calendars.FirstOrDefault(c =>
                        c.ApplyDate == applyDate &&
                        c.TimeFrom == timeFrom &&
                        c.TimeTo == timeTo);

                    if (existingCalendar != null)
                    {
                        // Nếu chúng ta đã xử lý cùng một slotKey cơ sở dữ liệu này trước đó trong lô thì bỏ qua.
                        if (!slotKeySet.Add(slotKey))
                        {
                            continue;
                        }

                        existingCalendar.PromotionTypeId = promotion.Id;
                        existingCalendar.MaxSlots = r.MaxSlots;
                        existingCalendar.InternalNote = r.InternalNote;
                        existingCalendar.IsActive = true;

                        await Repository.UpdateAsync(existingCalendar, autoSave: false);

                        // Cập nhật cấu hình giá, không xóa bản ghi cũ vì sẽ lỗi
                        await UpsertPricesAsync(existingCalendar.Id, r.CustomerTypePrice);

                        continue;
                    }

                    // Không tồn tại trong cơ sở dữ liệu => tạo slot MỚI, đồng thời loại bỏ trùng lặp theo slotKeySet
                    if (!slotKeySet.Add(slotKey))
                    {
                        continue;
                    }

                    var calendarId = GuidGenerator.Create();

                    var newCalendar = new CalendarSlot(
                        calendarId,
                        golfCourse.Id,
                        applyDate,
                        timeFrom,
                        timeTo
                    )
                    {
                        PromotionTypeId = promotion.Id,
                        MaxSlots = r.MaxSlots,
                        InternalNote = r.InternalNote,
                        IsActive = true,
                        TenantId = CurrentTenant.Id
                    };

                    pendingInsertByKey[slotKey] = newCalendar;

                    UpsertPendingPrices(calendarId, r.CustomerTypePrice);
                }
            }
        }

        // Lưu vị trí + giá mới 
        if (pendingInsertByKey.Count > 0)
            await Repository.InsertManyAsync(pendingInsertByKey.Values.ToList(), autoSave: false);

        if (pendingPriceByKey.Count > 0)
            await _priceRepository.InsertManyAsync(pendingPriceByKey.Values.ToList(), autoSave: false);

        await CurrentUnitOfWork.SaveChangesAsync();
        return 1;

        // Cập nhật dữ liệu cáu hình giá đang chờ xử lý
        void UpsertPendingPrices(Guid calendarSlotId, List<CustomerTypeExcelRowDto>? inputPrices)
        {
            if (inputPrices == null || inputPrices.Count == 0) return;

            // Loại bỏ dữ liệu trùng lặp theo tên loại khách hàng (lấy tên cuối cùng)
            var normalized = inputPrices
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.CustomerType) && x.Price18 > 0)
                .GroupBy(x => x.CustomerType.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .ToList();

            foreach (var item in normalized)
            {
                var ctName = item.CustomerType.Trim();
                if (!ctByName.TryGetValue(ctName, out var ct)) continue;

                var key = MakePriceKey(calendarSlotId, ct.Id);

                var price9 = item.Price9 > 0 ? item.Price9 : (decimal?)null;
                var price18 = item.Price18;
                var price27 = item.Price27 > 0 ? item.Price27 : (decimal?)null;
                var price36 = item.Price36 > 0 ? item.Price36 : (decimal?)null;

                if (pendingPriceByKey.TryGetValue(key, out var existing))
                {
                    // cập nhật trong bộ nhớ (loại bỏ trùng lặp)
                    existing.Price9 = price9;
                    existing.Price18 = price18;
                    existing.Price27 = price27;
                    existing.Price36 = price36;
                }
                else
                {
                    pendingPriceByKey[key] = new CalendarSlotPrice(
                        GuidGenerator.Create(),
                        calendarSlotId,
                        ct.Id,
                        price9,
                        price18,
                        price27,
                        price36
                    )
                    {
                        TenantId = CurrentTenant.Id
                    };
                }
            }
        }

        async Task UpsertPricesAsync(Guid calendarSlotId, List<CustomerTypeExcelRowDto>? inputPrices)
        {
            if (inputPrices == null || inputPrices.Count == 0) return;

            var normalized = inputPrices
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.CustomerType) && x.Price18 > 0)
                .GroupBy(x => x.CustomerType.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .ToList();

            if (normalized.Count == 0) return;

            var existingPrices = await GetPricesOfSlotAsync(calendarSlotId);
            var existingByCtId = existingPrices
                .GroupBy(x => x.CustomerTypeId)
                .ToDictionary(g => g.Key, g => g.First());

            var toInsert = new List<CalendarSlotPrice>();

            foreach (var item in normalized)
            {
                var ctName = item.CustomerType.Trim();
                if (!ctByName.TryGetValue(ctName, out var ct)) continue;

                var price9 = item.Price9 > 0 ? item.Price9 : (decimal?)null;
                var price18 = item.Price18;
                var price27 = item.Price27 > 0 ? item.Price27 : (decimal?)null;
                var price36 = item.Price36 > 0 ? item.Price36 : (decimal?)null;

                if (existingByCtId.TryGetValue(ct.Id, out var row))
                {
                    // Cập nhật nếu tồn tại
                    row.Price9 = price9;
                    row.Price18 = price18;
                    row.Price27 = price27;
                    row.Price36 = price36;
                    row.TenantId = CurrentTenant.Id;

                    await _priceRepository.UpdateAsync(row, autoSave: false);
                }
                else
                {
                    // Thêm mới cho mỗi khóa
                    var newRow = new CalendarSlotPrice(
                        GuidGenerator.Create(),
                        calendarSlotId,
                        ct.Id,
                        price9,
                        price18,
                        price27,
                        price36
                    )
                    {
                        TenantId = CurrentTenant.Id
                    };

                    toInsert.Add(newRow);

                    // Đồng thời cập nhật bộ nhớ cache để tránh chèn trùng lặp nếu cùng một vị trí xuất hiện lại trong lô
                    existingPrices.Add(newRow);
                    existingByCtId[ct.Id] = newRow;
                }
            }

            if (toInsert.Count > 0)
                await _priceRepository.InsertManyAsync(toInsert, autoSave: false);
        }
    }
}