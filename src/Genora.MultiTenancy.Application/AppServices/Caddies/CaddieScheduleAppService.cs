using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.Caddie;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.Caddies;

[Authorize]
public class CaddieScheduleAppService : ApplicationService
{
    private readonly IRepository<AppCaddieSchedule, Guid> _scheduleRepo;
    private readonly IRepository<AppCaddie, Guid> _caddieRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly IGuidGenerator _guidGenerator;

    public CaddieScheduleAppService(
        IRepository<AppCaddieSchedule, Guid> scheduleRepo,
        IRepository<AppCaddie, Guid> caddieRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IGuidGenerator guidGenerator)
    {
        _scheduleRepo = scheduleRepo;
        _caddieRepo = caddieRepo;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        _guidGenerator = guidGenerator;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.IsAvailable ? tenantPerm : hostPerm;

    private async Task EnsureFeatureAsync()
    {
        if (!_currentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(CaddieFeatures.Management))
            throw new AbpAuthorizationException($"Feature '{CaddieFeatures.Management}' is disabled for this tenant.");
    }

    public async Task<PagedResultDto<CaddieScheduleDto>> GetListAsync(GetCaddieScheduleListInput input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSchedules.Default, MultiTenancyPermissions.HostAppCaddieSchedules.Default));

        var query = await _scheduleRepo.GetQueryableAsync();

        if (input.CaddieId.HasValue)
            query = query.Where(x => x.CaddieId == input.CaddieId.Value);

        if (input.SlotStatus.HasValue)
            query = query.Where(x => x.SlotStatus == input.SlotStatus.Value);

        if (input.FromDate.HasValue)
            query = query.Where(x => x.WorkDate >= input.FromDate.Value);

        if (input.ToDate.HasValue)
            query = query.Where(x => x.WorkDate <= input.ToDate.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "WorkDate ASC, StartTime ASC" : input.Sorting;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Load caddie names
        var caddieIds = items.Select(x => x.CaddieId).Distinct().ToList();
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CaddieName, x.CaddieCode });
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        var dtos = items.Select(x =>
        {
            var caddie = caddies.FirstOrDefault(c => c.Id == x.CaddieId);
            return MapToDto(x, caddie?.CaddieName, caddie?.CaddieCode);
        }).ToList();

        return new PagedResultDto<CaddieScheduleDto>(totalCount, dtos);
    }

    public async Task<List<CaddieScheduleDto>> GetWeekScheduleAsync(DateTime weekStart)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSchedules.Default, MultiTenancyPermissions.HostAppCaddieSchedules.Default));

        var weekEnd = weekStart.AddDays(7);

        var query = (await _scheduleRepo.GetQueryableAsync())
            .Where(x => x.WorkDate >= weekStart && x.WorkDate < weekEnd)
            .OrderBy(x => x.WorkDate).ThenBy(x => x.StartTime);

        var items = await AsyncExecuter.ToListAsync(query);

        // Load caddie names
        var caddieIds = items.Select(x => x.CaddieId).Distinct().ToList();
        var caddieQuery = (await _caddieRepo.GetQueryableAsync())
            .Where(x => caddieIds.Contains(x.Id))
            .Select(x => new { x.Id, x.CaddieName, x.CaddieCode });
        var caddies = await AsyncExecuter.ToListAsync(caddieQuery);

        return items.Select(x =>
        {
            var caddie = caddies.FirstOrDefault(c => c.Id == x.CaddieId);
            return MapToDto(x, caddie?.CaddieName, caddie?.CaddieCode);
        }).ToList();
    }

    public async Task<CaddieScheduleDto> CreateAsync(CreateUpdateCaddieScheduleDto input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSchedules.Create, MultiTenancyPermissions.HostAppCaddieSchedules.Create));

        // Check if schedule already exists for same caddie + date + shift + startTime (unique time slot)
        var existingQuery = (await _scheduleRepo.GetQueryableAsync())
            .Where(x => x.CaddieId == input.CaddieId
                && x.WorkDate == input.WorkDate
                && x.ShiftCode == input.ShiftCode
                && x.StartTime == input.StartTime);
        var existing = await AsyncExecuter.FirstOrDefaultAsync(existingQuery);

        if (existing != null)
        {
            // If existing has booking, skip (don't overwrite)
            if (existing.SlotStatus == (byte)CaddieSlotStatus.Booked && existing.BookingId.HasValue)
                throw new UserFriendlyException("Khung giờ này đã có booking, không thể ghi đè.");

            // Overwrite existing
            existing.EndTime = input.EndTime;
            existing.SlotStatus = input.SlotStatus;
            existing.IsNightShift = input.IsNightShift;
            existing.Note = input.Note;
            await _scheduleRepo.UpdateAsync(existing, autoSave: true);

            var caddieUpdate = await _caddieRepo.GetAsync(input.CaddieId);
            return MapToDto(existing, caddieUpdate.CaddieName, caddieUpdate.CaddieCode);
        }

        var entity = new AppCaddieSchedule
        {
            CaddieId = input.CaddieId,
            WorkDate = input.WorkDate,
            ShiftCode = input.ShiftCode,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            SlotStatus = input.SlotStatus,
            IsNightShift = input.IsNightShift,
            Note = input.Note
        };

        await _scheduleRepo.InsertAsync(entity, autoSave: true);

        var caddie = await _caddieRepo.GetAsync(input.CaddieId);
        return MapToDto(entity, caddie.CaddieName, caddie.CaddieCode);
    }

    public async Task<CaddieScheduleDto> UpdateAsync(Guid id, CreateUpdateCaddieScheduleDto input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSchedules.Edit, MultiTenancyPermissions.HostAppCaddieSchedules.Edit));

        var entity = await _scheduleRepo.GetAsync(id);

        entity.CaddieId = input.CaddieId;
        entity.WorkDate = input.WorkDate;
        entity.ShiftCode = input.ShiftCode;
        entity.StartTime = input.StartTime;
        entity.EndTime = input.EndTime;
        entity.SlotStatus = input.SlotStatus;
        entity.IsNightShift = input.IsNightShift;
        entity.Note = input.Note;

        await _scheduleRepo.UpdateAsync(entity, autoSave: true);

        var caddie = await _caddieRepo.GetAsync(entity.CaddieId);
        return MapToDto(entity, caddie.CaddieName, caddie.CaddieCode);
    }

    public async Task UpdateSlotStatusAsync(Guid id, byte slotStatus, string? note)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSchedules.Edit, MultiTenancyPermissions.HostAppCaddieSchedules.Edit));

        var entity = await _scheduleRepo.GetAsync(id);
        entity.SlotStatus = slotStatus;
        if (!string.IsNullOrWhiteSpace(note))
            entity.Note = note;

        await _scheduleRepo.UpdateAsync(entity, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSchedules.Delete, MultiTenancyPermissions.HostAppCaddieSchedules.Delete));

        var entity = await _scheduleRepo.GetAsync(id);

        // Don't allow delete if booked (has booking)
        if (entity.SlotStatus == (byte)CaddieSlotStatus.Booked && entity.BookingId.HasValue)
            throw new UserFriendlyException("Không thể xóa ca làm việc đang có booking.");

        await _scheduleRepo.DeleteAsync(id);
    }

    /// <summary>
    /// Xóa lịch làm việc theo khoảng ngày/giờ. Bỏ qua những ca đã có booking.
    /// </summary>
    public async Task<DeleteCaddieScheduleRangeResultDto> DeleteRangeAsync(DeleteCaddieScheduleRangeInput input)
    {
        await EnsureFeatureAsync();
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSchedules.Delete, MultiTenancyPermissions.HostAppCaddieSchedules.Delete));

        var query = (await _scheduleRepo.GetQueryableAsync())
            .Where(x => x.WorkDate >= input.FromDate && x.WorkDate <= input.ToDate);

        if (input.FromTime.HasValue)
            query = query.Where(x => x.StartTime >= input.FromTime.Value);

        if (input.ToTime.HasValue)
            query = query.Where(x => x.EndTime <= input.ToTime.Value);

        if (input.CaddieId.HasValue)
            query = query.Where(x => x.CaddieId == input.CaddieId.Value);

        var items = await AsyncExecuter.ToListAsync(query);

        // Separate: can delete vs cannot delete (has booking)
        var canDelete = items.Where(x => x.SlotStatus != (byte)CaddieSlotStatus.Booked || !x.BookingId.HasValue).ToList();
        var skipped = items.Count - canDelete.Count;

        if (canDelete.Any())
            await _scheduleRepo.DeleteManyAsync(canDelete, autoSave: true);

        return new DeleteCaddieScheduleRangeResultDto
        {
            TotalFound = items.Count,
            DeletedCount = canDelete.Count,
            SkippedCount = skipped
        };
    }

    private static CaddieScheduleDto MapToDto(AppCaddieSchedule entity, string? caddieName, string? caddieCode)
    {
        return new CaddieScheduleDto
        {
            Id = entity.Id,
            CaddieId = entity.CaddieId,
            CaddieName = caddieName,
            CaddieCode = caddieCode,
            WorkDate = entity.WorkDate,
            ShiftCode = entity.ShiftCode,
            ShiftCodeText = entity.ShiftCode switch
            {
                (byte)CaddieShiftCode.Morning => "Sáng",
                (byte)CaddieShiftCode.Afternoon => "Chiều",
                (byte)CaddieShiftCode.Night => "Tối",
                _ => "Khác"
            },
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            SlotStatus = entity.SlotStatus,
            SlotStatusText = entity.SlotStatus switch
            {
                (byte)CaddieSlotStatus.Available => "Trống lịch",
                (byte)CaddieSlotStatus.Booked => "Đang phục vụ",
                (byte)CaddieSlotStatus.Off => "Nghỉ",
                _ => "Khác"
            },
            BookingId = entity.BookingId,
            IsNightShift = entity.IsNightShift,
            Note = entity.Note
        };
    }
}
