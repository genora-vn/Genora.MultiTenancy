using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyTimeSlotAppService : ApplicationService, ISalonBeautyTimeSlotAppService
{
    private readonly IRepository<SalonBeautyTimeSlot, Guid> _slotRepo;
    private readonly IRepository<SalonBeautyStylist, Guid> _stylistRepo;
    private readonly IRepository<SalonBeautyLocation, Guid> _locationRepo;
    private readonly IStringLocalizer<MultiTenancyResource> _l;
    private readonly ICurrentTenant _currentTenant;

    public SalonBeautyTimeSlotAppService(
        IRepository<SalonBeautyTimeSlot, Guid> slotRepo,
        IRepository<SalonBeautyStylist, Guid> stylistRepo,
        IRepository<SalonBeautyLocation, Guid> locationRepo,
        IStringLocalizer<MultiTenancyResource> l,
        ICurrentTenant currentTenant)
    {
        _slotRepo = slotRepo;
        _stylistRepo = stylistRepo;
        _locationRepo = locationRepo;
        _l = l;
        _currentTenant = currentTenant;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<PagedResultDto<SalonBeautyTimeSlotGroupedDto>> GetListAsync(GetSalonBeautyTimeSlotListInput input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var slotQuery = await _slotRepo.GetQueryableAsync();
        var stylistQuery = await _stylistRepo.GetQueryableAsync();
        var locationQuery = await _locationRepo.GetQueryableAsync();

        if (input.LocationId.HasValue)
            slotQuery = slotQuery.Where(x => x.LocationId == input.LocationId.Value);

        if (input.StylistId.HasValue)
            slotQuery = slotQuery.Where(x => x.StylistId == input.StylistId.Value);

        if (input.FromDate.HasValue)
        {
            var from = input.FromDate.Value.Date;
            slotQuery = slotQuery.Where(x => x.WorkDate >= from);
        }

        if (input.ToDate.HasValue)
        {
            var to = input.ToDate.Value.Date;
            slotQuery = slotQuery.Where(x => x.WorkDate <= to);
        }

        if (input.Status.HasValue)
            slotQuery = slotQuery.Where(x => (byte)x.Status == input.Status.Value);

        if (input.IsShowOnApp.HasValue)
            slotQuery = slotQuery.Where(x => x.IsShowOnApp == input.IsShowOnApp.Value);

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText.Trim();
            var stylistIds = stylistQuery
                .Where(s => s.DisplayName.Contains(keyword))
                .Select(s => s.Id);
            slotQuery = slotQuery.Where(x => stylistIds.Contains(x.StylistId));
        }

        // Group by stylist, aggregate
        var grouped = await AsyncExecuter.ToListAsync(
            slotQuery
                .GroupBy(x => x.StylistId)
                .Select(g => new
                {
                    StylistId = g.Key,
                    LocationId = (Guid?)g.Min(x => x.LocationId),
                    FromDate = (DateTime?)g.Min(x => x.WorkDate),
                    ToDate = (DateTime?)g.Max(x => x.WorkDate),
                    FromTime = (TimeSpan?)g.Min(x => x.StartTime),
                    ToTime = (TimeSpan?)g.Max(x => x.EndTime),
                    SlotCount = g.Count(),
                    HasOn = g.Any(x => x.Status == SalonBeautyTimeSlotStatus.On),
                    HasShowOnApp = g.Any(x => x.IsShowOnApp)
                }));

        var stylistIdList = grouped.Select(x => x.StylistId).Distinct().ToList();
        var stylists = await AsyncExecuter.ToListAsync(stylistQuery.Where(x => stylistIdList.Contains(x.Id)));
        var stylistMap = stylists.ToDictionary(x => x.Id, x => x);

        var locationIdList = grouped.Where(x => x.LocationId.HasValue).Select(x => x.LocationId!.Value).Distinct().ToList();
        var locations = await AsyncExecuter.ToListAsync(locationQuery.Where(x => locationIdList.Contains(x.Id)));
        var locationMap = locations.ToDictionary(x => x.Id, x => x);

        var allItems = grouped
            .Select(g =>
            {
                stylistMap.TryGetValue(g.StylistId, out var stylist);
                SalonBeautyLocation? location = null;
                if (g.LocationId.HasValue)
                    locationMap.TryGetValue(g.LocationId.Value, out location);

                return new SalonBeautyTimeSlotGroupedDto
                {
                    StylistId = g.StylistId,
                    StylistName = stylist?.DisplayName ?? string.Empty,
                    StylistAvatar = stylist?.Avatar,
                    LocationId = g.LocationId,
                    LocationName = location?.Name,
                    FromDate = g.FromDate,
                    ToDate = g.ToDate,
                    FromTime = g.FromTime,
                    ToTime = g.ToTime,
                    IsActive = g.HasOn,
                    IsShowOnApp = g.HasShowOnApp,
                    SlotCount = g.SlotCount
                };
            })
            .OrderBy(x => x.StylistName)
            .ToList();

        var totalCount = allItems.Count;
        var pageItems = allItems
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<SalonBeautyTimeSlotGroupedDto>(totalCount, pageItems);
    }

    public async Task<SalonBeautyTimeSlotEditDto> GetByStylistAsync(Guid stylistId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        var stylist = await _stylistRepo.GetAsync(stylistId);
        var slotQuery = (await _slotRepo.GetQueryableAsync())
            .Where(x => x.StylistId == stylistId);

        if (fromDate.HasValue)
            slotQuery = slotQuery.Where(x => x.WorkDate >= fromDate.Value.Date);
        if (toDate.HasValue)
            slotQuery = slotQuery.Where(x => x.WorkDate <= toDate.Value.Date);

        var slots = await AsyncExecuter.ToListAsync(slotQuery.OrderBy(x => x.WorkDate).ThenBy(x => x.StartTime));

        if (slots.Count == 0)
        {
            return new SalonBeautyTimeSlotEditDto
            {
                StylistId = stylistId,
                StylistName = stylist.DisplayName,
                FromDate = fromDate?.Date,
                ToDate = toDate?.Date
            };
        }

        var locationId = slots.First().LocationId;
        SalonBeautyLocation? location = null;
        try { location = await _locationRepo.GetAsync(locationId); } catch { }

        var ranges = slots
            .GroupBy(x => new { x.StartTime, x.EndTime })
            .Select(g => new TimeRangeDto
            {
                StartTime = g.Key.StartTime,
                EndTime = g.Key.EndTime,
                Capacity = g.Max(x => x.Capacity),
                IsPeakHour = g.Any(x => x.Status == SalonBeautyTimeSlotStatus.PeakHour)
            })
            .OrderBy(x => x.StartTime)
            .ToList();

        var weekdayMask = 0;
        foreach (var slot in slots)
        {
            weekdayMask |= 1 << (int)slot.WorkDate.DayOfWeek;
        }

        var first = slots.First();

        return new SalonBeautyTimeSlotEditDto
        {
            StylistId = stylistId,
            StylistName = stylist.DisplayName,
            LocationId = locationId,
            LocationName = location?.Name,
            FromDate = slots.Min(x => x.WorkDate),
            ToDate = slots.Max(x => x.WorkDate),
            Ranges = ranges,
            WeekdayMask = weekdayMask == 0 ? 127 : weekdayMask,
            IsShowOnApp = slots.Any(x => x.IsShowOnApp),
            Status = (byte)first.Status,
            Note = first.Note
        };
    }

    public async Task<List<SalonBeautyTimeSlotDto>> GetCalendarEventsAsync(GetSalonBeautyTimeSlotCalendarInput input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        var slotQuery = await _slotRepo.GetQueryableAsync();
        slotQuery = slotQuery.Where(x => x.WorkDate >= input.FromDate.Date && x.WorkDate <= input.ToDate.Date);

        if (input.LocationId.HasValue)
            slotQuery = slotQuery.Where(x => x.LocationId == input.LocationId.Value);
        if (input.StylistId.HasValue)
            slotQuery = slotQuery.Where(x => x.StylistId == input.StylistId.Value);
        if (input.Status.HasValue)
            slotQuery = slotQuery.Where(x => (byte)x.Status == input.Status.Value);

        var slots = await AsyncExecuter.ToListAsync(slotQuery.OrderBy(x => x.WorkDate).ThenBy(x => x.StartTime));

        var stylistIds = slots.Select(x => x.StylistId).Distinct().ToList();
        var locationIds = slots.Select(x => x.LocationId).Distinct().ToList();

        var stylists = await AsyncExecuter.ToListAsync((await _stylistRepo.GetQueryableAsync()).Where(x => stylistIds.Contains(x.Id)));
        var locations = await AsyncExecuter.ToListAsync((await _locationRepo.GetQueryableAsync()).Where(x => locationIds.Contains(x.Id)));

        var stylistMap = stylists.ToDictionary(x => x.Id, x => x);
        var locationMap = locations.ToDictionary(x => x.Id, x => x);

        return slots.Select(s => MapToDto(s, stylistMap, locationMap)).ToList();
    }

    public async Task<List<TimeRangeDto>> GenerateRangesByLocationAsync(Guid locationId)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        var location = await _locationRepo.GetAsync(locationId);
        return GenerateRanges(location);
    }

    public async Task<List<SalonBeautyStylistLookupDto>> GetStylistLookupAsync(Guid? locationId = null)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        var query = await _stylistRepo.GetQueryableAsync();
        query = query.Where(x => x.Status == 1);
        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        var stylists = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SortOrder).ThenBy(x => x.DisplayName));

        return stylists.Select(x => new SalonBeautyStylistLookupDto
        {
            Id = x.Id,
            LocationId = x.LocationId,
            DisplayName = x.DisplayName,
            Avatar = x.Avatar,
            Role = x.Role,
            RoleText = x.Role.HasValue ? $"Enum:SalonBeautyStylistRole.{(SalonBeautyStylistRole)x.Role.Value}" : null
        }).ToList();
    }

    public async Task<List<DateTime>> GetAvailableDatesAsync(Guid stylistId, DateTime fromDate, DateTime toDate, Guid? locationId = null)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        var from = fromDate.Date;
        var to = toDate.Date;
        if (to < from) (from, to) = (to, from);

        var query = await _slotRepo.GetQueryableAsync();
        query = query.Where(x => x.StylistId == stylistId
                                 && x.WorkDate >= from
                                 && x.WorkDate <= to
                                 && x.Status != SalonBeautyTimeSlotStatus.Off
                                 && x.IsShowOnApp);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        var dates = await AsyncExecuter.ToListAsync(
            query.Select(x => x.WorkDate).Distinct());

        return dates.OrderBy(x => x).ToList();
    }

    public async Task<List<SalonBeautyTimeSlotDto>> GetAvailableSlotsAsync(Guid stylistId, DateTime workDate, Guid? locationId = null)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        var date = workDate.Date;

        var query = await _slotRepo.GetQueryableAsync();
        query = query.Where(x => x.StylistId == stylistId
                                 && x.WorkDate == date
                                 && x.Status != SalonBeautyTimeSlotStatus.Off
                                 && x.Status != SalonBeautyTimeSlotStatus.Full
                                 && x.IsShowOnApp
                                 && x.BookedCount < x.Capacity);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        var slots = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.StartTime));
        if (slots.Count == 0) return new List<SalonBeautyTimeSlotDto>();

        var stylistIds = slots.Select(x => x.StylistId).Distinct().ToList();
        var locationIds = slots.Select(x => x.LocationId).Distinct().ToList();
        var stylists = await AsyncExecuter.ToListAsync((await _stylistRepo.GetQueryableAsync()).Where(x => stylistIds.Contains(x.Id)));
        var locations = await AsyncExecuter.ToListAsync((await _locationRepo.GetQueryableAsync()).Where(x => locationIds.Contains(x.Id)));
        var stylistMap = stylists.ToDictionary(x => x.Id, x => x);
        var locationMap = locations.ToDictionary(x => x.Id, x => x);

        return slots.Select(s => MapToDto(s, stylistMap, locationMap)).ToList();
    }

    public async Task<SalonBeautyTimeSlotDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default);

        var slot = await _slotRepo.GetAsync(id);

        var stylistMap = new Dictionary<Guid, SalonBeautyStylist>();
        var locationMap = new Dictionary<Guid, SalonBeautyLocation>();

        if (slot.StylistId != Guid.Empty)
        {
            var stylist = await _stylistRepo.FindAsync(slot.StylistId);
            if (stylist != null) stylistMap[stylist.Id] = stylist;
        }
        if (slot.LocationId != Guid.Empty)
        {
            var location = await _locationRepo.FindAsync(slot.LocationId);
            if (location != null) locationMap[location.Id] = location;
        }

        return MapToDto(slot, stylistMap, locationMap);
    }

    public async Task<List<SalonBeautyTimeSlotDto>> CreateAsync(CreateSalonBeautyTimeSlotDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Create, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Create);

        ValidateCreateUpdate(input.FromDate, input.ToDate, input.Ranges);

        await _stylistRepo.GetAsync(input.StylistId);
        var location = await _locationRepo.GetAsync(input.LocationId);

        ValidateRangesAgainstLocation(input.Ranges, location);

        var existing = (await _slotRepo.GetQueryableAsync())
            .Where(x => x.StylistId == input.StylistId
                        && x.WorkDate >= input.FromDate.Date
                        && x.WorkDate <= input.ToDate.Date);
        if (await AsyncExecuter.AnyAsync(existing))
            throw new UserFriendlyException(L("SalonBeautyTimeSlots:OverlappingSchedule"));

        var slots = BuildSlots(input.LocationId, input.StylistId, input.FromDate, input.ToDate, input.Ranges, input.WeekdayMask, input.IsShowOnApp, input.Status, input.Note, location.MaxCapacityPerSlot);

        var created = new List<SalonBeautyTimeSlot>();
        foreach (var s in slots)
        {
            var inserted = await _slotRepo.InsertAsync(s, autoSave: true);
            created.Add(inserted);
        }

        return await BuildDtoListAsync(created);
    }

    public async Task<List<SalonBeautyTimeSlotDto>> UpdateByStylistAsync(Guid stylistId, UpdateSalonBeautyTimeSlotDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Edit, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Edit);

        ValidateCreateUpdate(input.FromDate, input.ToDate, input.Ranges);

        await _stylistRepo.GetAsync(stylistId);
        var location = await _locationRepo.GetAsync(input.LocationId);

        ValidateRangesAgainstLocation(input.Ranges, location);

        // Replace toàn bộ slot của stylist
        var existing = (await _slotRepo.GetQueryableAsync())
            .Where(x => x.StylistId == stylistId);
        var existingList = await AsyncExecuter.ToListAsync(existing);
        foreach (var s in existingList)
        {
            await _slotRepo.DeleteAsync(s.Id, autoSave: true);
        }

        var slots = BuildSlots(input.LocationId, stylistId, input.FromDate, input.ToDate, input.Ranges, input.WeekdayMask, input.IsShowOnApp, input.Status, input.Note, location.MaxCapacityPerSlot);

        var created = new List<SalonBeautyTimeSlot>();
        foreach (var s in slots)
        {
            var inserted = await _slotRepo.InsertAsync(s, autoSave: true);
            created.Add(inserted);
        }

        return await BuildDtoListAsync(created);
    }

    public async Task<SalonBeautyTimeSlotDto> UpdateStatusAsync(Guid id, UpdateSalonBeautyTimeSlotStatusDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Edit, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Edit);

        if (!Enum.IsDefined(typeof(SalonBeautyTimeSlotStatus), input.Status))
            throw new UserFriendlyException(L("SalonBeautyTimeSlots:StatusInvalid"));

        var entity = await _slotRepo.GetAsync(id);
        var newStatus = (SalonBeautyTimeSlotStatus)input.Status;

        // Admin tự can thiệp → bật manual override để recalculate không đè
        entity.IsManualOverride = true;
        entity.Status = newStatus;
        await _slotRepo.UpdateAsync(entity, autoSave: true);

        var stylist = await _stylistRepo.GetAsync(entity.StylistId);
        var location = await _locationRepo.GetAsync(entity.LocationId);

        var stylistMap = new Dictionary<Guid, SalonBeautyStylist> { { stylist.Id, stylist } };
        var locationMap = new Dictionary<Guid, SalonBeautyLocation> { { location.Id, location } };

        return MapToDto(entity, stylistMap, locationMap);
    }

    public async Task DeleteByStylistAsync(Guid stylistId)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Delete, MultiTenancyPermissions.HostSalonBeautyTimeSlots.Delete);

        var existing = await AsyncExecuter.ToListAsync(
            (await _slotRepo.GetQueryableAsync()).Where(x => x.StylistId == stylistId));

        foreach (var s in existing)
        {
            await _slotRepo.DeleteAsync(s.Id, autoSave: true);
        }
    }

    private static List<SalonBeautyTimeSlot> BuildSlots(
        Guid locationId,
        Guid stylistId,
        DateTime fromDate,
        DateTime toDate,
        List<TimeRangeDto> ranges,
        int weekdayMask,
        bool isShowOnApp,
        byte status,
        string? note,
        int locationMaxCapacity)
    {
        var defaultStatus = Enum.IsDefined(typeof(SalonBeautyTimeSlotStatus), status)
            ? (SalonBeautyTimeSlotStatus)status
            : SalonBeautyTimeSlotStatus.On;

        // Off (admin tắt cả lịch) thì giữ nguyên Off cho mọi slot — không cho peak override.
        var mask = weekdayMask == 0 ? 127 : weekdayMask;
        var result = new List<SalonBeautyTimeSlot>();

        for (var day = fromDate.Date; day <= toDate.Date; day = day.AddDays(1))
        {
            var dayBit = 1 << (int)day.DayOfWeek;
            if ((mask & dayBit) == 0) continue;

            foreach (var r in ranges)
            {
                var capacity = r.Capacity is > 0 ? r.Capacity!.Value : locationMaxCapacity;
                if (capacity > locationMaxCapacity) capacity = locationMaxCapacity;
                if (capacity < 1) capacity = 1;

                var rangeStatus = defaultStatus;
                if (defaultStatus != SalonBeautyTimeSlotStatus.Off && r.IsPeakHour)
                {
                    rangeStatus = SalonBeautyTimeSlotStatus.PeakHour;
                }

                result.Add(new SalonBeautyTimeSlot
                {
                    LocationId = locationId,
                    StylistId = stylistId,
                    WorkDate = day,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Capacity = capacity,
                    BookedCount = 0,
                    IsManualOverride = false,
                    Status = rangeStatus,
                    IsShowOnApp = isShowOnApp,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Validate capacity per range không vượt Location.MaxCapacityPerSlot (BR-Slot).
    /// </summary>
    private void ValidateRangesAgainstLocation(List<TimeRangeDto> ranges, SalonBeautyLocation location)
    {
        foreach (var r in ranges)
        {
            // Slot không được vượt qua close_time (BR-06)
            if (r.EndTime > location.CloseTime)
                throw new UserFriendlyException(L("SalonBeautyTimeSlots:RangeOutsideLocation"));

            if (r.StartTime < location.OpenTime)
                throw new UserFriendlyException(L("SalonBeautyTimeSlots:RangeOutsideLocation"));

            if (r.Capacity.HasValue && r.Capacity.Value > location.MaxCapacityPerSlot)
                throw new UserFriendlyException(L("SalonBeautyTimeSlots:CapacityExceedsLocation"));
        }
    }

    /// <summary>
    /// Auto-generate khung giờ trong 1 ngày dựa vào (open_time, close_time, slot_duration, buffer_time).
    /// VD: open=09:00, close=18:00, slot=60, buffer=10 → [09:00-10:00, 10:10-11:10, ..., 17:50-18:00].
    /// </summary>
    private static List<TimeRangeDto> GenerateRanges(SalonBeautyLocation location)
    {
        var result = new List<TimeRangeDto>();
        if (location.SlotDuration <= 0) return result;

        var current = location.OpenTime;
        var slotDuration = TimeSpan.FromMinutes(location.SlotDuration);
        var buffer = TimeSpan.FromMinutes(Math.Max(0, location.BufferTime));

        while (current < location.CloseTime)
        {
            var end = current + slotDuration;
            if (end > location.CloseTime) end = location.CloseTime;

            // Slot quá ngắn (< 5 phút) thì bỏ qua
            if ((end - current).TotalMinutes < 5) break;

            result.Add(new TimeRangeDto
            {
                StartTime = current,
                EndTime = end,
                Capacity = location.MaxCapacityPerSlot
            });

            current = end + buffer;
        }

        return result;
    }

    private void ValidateCreateUpdate(DateTime fromDate, DateTime toDate, List<TimeRangeDto> ranges)
    {
        if (fromDate.Date > toDate.Date)
            throw new UserFriendlyException(L("SalonBeautyTimeSlots:DateRangeInvalid"));

        if (ranges == null || ranges.Count == 0)
            throw new UserFriendlyException(L("SalonBeautyTimeSlots:TimeRangeRequired"));

        foreach (var r in ranges)
        {
            if (r.StartTime >= r.EndTime)
                throw new UserFriendlyException(L("SalonBeautyTimeSlots:TimeRangeInvalid"));
        }

        // Detect overlap
        var sorted = ranges.OrderBy(x => x.StartTime).ToList();
        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i].StartTime < sorted[i - 1].EndTime)
                throw new UserFriendlyException(L("SalonBeautyTimeSlots:TimeRangeOverlap"));
        }
    }

    private async Task<List<SalonBeautyTimeSlotDto>> BuildDtoListAsync(List<SalonBeautyTimeSlot> slots)
    {
        if (slots.Count == 0) return new List<SalonBeautyTimeSlotDto>();

        var stylistIds = slots.Select(x => x.StylistId).Distinct().ToList();
        var locationIds = slots.Select(x => x.LocationId).Distinct().ToList();

        var stylists = await AsyncExecuter.ToListAsync((await _stylistRepo.GetQueryableAsync()).Where(x => stylistIds.Contains(x.Id)));
        var locations = await AsyncExecuter.ToListAsync((await _locationRepo.GetQueryableAsync()).Where(x => locationIds.Contains(x.Id)));

        var stylistMap = stylists.ToDictionary(x => x.Id, x => x);
        var locationMap = locations.ToDictionary(x => x.Id, x => x);

        return slots.Select(s => MapToDto(s, stylistMap, locationMap)).ToList();
    }

    private SalonBeautyTimeSlotDto MapToDto(
        SalonBeautyTimeSlot entity,
        Dictionary<Guid, SalonBeautyStylist> stylistMap,
        Dictionary<Guid, SalonBeautyLocation> locationMap)
    {
        stylistMap.TryGetValue(entity.StylistId, out var stylist);
        locationMap.TryGetValue(entity.LocationId, out var location);

        return new SalonBeautyTimeSlotDto
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            LocationName = location?.Name,
            StylistId = entity.StylistId,
            StylistName = stylist?.DisplayName,
            StylistAvatar = stylist?.Avatar,
            WorkDate = entity.WorkDate,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Capacity = entity.Capacity,
            BookedCount = entity.BookedCount,
            CapacityText = $"{entity.BookedCount}/{entity.Capacity}",
            IsManualOverride = entity.IsManualOverride,
            Status = (byte)entity.Status,
            StatusText = LocalizeStatus(entity.Status),
            IsPeakHour = entity.Status == SalonBeautyTimeSlotStatus.PeakHour,
            IsShowOnApp = entity.IsShowOnApp,
            Note = entity.Note
        };
    }

    private string LocalizeStatus(SalonBeautyTimeSlotStatus status)
    {
        var key = $"Enum:SalonBeautyTimeSlotStatus.{status}";
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? status.ToString() : text;
    }

    private string L(string key)
    {
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? key : text;
    }

    private async Task CheckPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = _currentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty time slot permission.");
        await AuthorizationService.CheckAsync(permission);
    }
}
