using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Hl25;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService cấu hình vòng quay may mắn (singleton theo tenant).
/// Get/Update cấu hình + slots (thay thế toàn bộ), validate tổng WinRate = 100.
/// </summary>
[Authorize]
public class Hl25WheelConfigAppService : ApplicationService, IHl25WheelConfigAppService
{
    private readonly IRepository<Hl25WheelConfig, Guid> _configRepository;
    private readonly IRepository<Hl25WheelSlot, Guid> _slotRepository;
    private readonly IManageImageService _manageImageService;
    private readonly IFeatureChecker _featureChecker;

    public Hl25WheelConfigAppService(
        IRepository<Hl25WheelConfig, Guid> configRepository,
        IRepository<Hl25WheelSlot, Guid> slotRepository,
        IManageImageService manageImageService,
        IFeatureChecker featureChecker)
    {
        _configRepository = configRepository;
        _slotRepository = slotRepository;
        _manageImageService = manageImageService;
        _featureChecker = featureChecker;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<Hl25WheelConfigDto> GetAsync()
    {
        await CheckViewPolicyAsync();

        var config = await GetOrCreateConfigAsync();
        var slots = await GetSlotsAsync(config.Id);

        return MapToDto(config, slots);
    }

    public async Task<Hl25WheelConfigDto> UpdateAsync(CreateUpdateHl25WheelConfigDto input)
    {
        await CheckEditPolicyAsync();

        ValidateWinRate(input.Slots);

        var config = await GetOrCreateConfigAsync();

        // Cập nhật cấu hình
        config.Title = input.Title;
        config.SubTitle = input.SubTitle;
        config.PrimaryColor = input.PrimaryColor;
        config.SecondaryColor = input.SecondaryColor;
        config.BackgroundImageUrl = input.BackgroundImageUrl;
        config.PointerImageUrl = input.PointerImageUrl;
        config.IsActive = input.IsActive;
        config.SlotCount = input.Slots?.Count ?? 0;
        await _configRepository.UpdateAsync(config, autoSave: true);

        // Thay thế toàn bộ slots (xóa cũ, chèn mới) — pattern MARS/autoSave: xử lý child qua repo.
        var existingSlots = await GetSlotsAsync(config.Id);
        if (existingSlots.Count > 0)
        {
            await _slotRepository.DeleteManyAsync(existingSlots, autoSave: true);
        }

        if (input.Slots != null && input.Slots.Count > 0)
        {
            var newSlots = new List<Hl25WheelSlot>();
            foreach (var s in input.Slots)
            {
                var slot = new Hl25WheelSlot(GuidGenerator.Create(), config.Id, CurrentTenant.Id)
                {
                    GiftId = s.GiftId,
                    Label = s.Label,
                    SlotImageUrl = s.SlotImageUrl,
                    WinRate = s.WinRate,
                    DisplayOrder = s.DisplayOrder,
                    ColorHex = s.ColorHex
                };
                newSlots.Add(slot);
            }
            await _slotRepository.InsertManyAsync(newSlots, autoSave: true);
        }

        var slotsAfter = await GetSlotsAsync(config.Id);
        return MapToDto(config, slotsAfter);
    }

    public async Task<string> UploadWheelImageAsync(IRemoteStreamContent file)
    {
        await CheckEditPolicyAsync();

        if (file == null)
            throw new BusinessException("Hl25:WheelImageRequired");

        var length = file.ContentLength ?? file.GetStream().Length;
        if (length > Hl25Consts.MaxCardImageSizeBytes)
            throw new BusinessException("Hl25:AssetTooLarge").WithData("MaxBytes", Hl25Consts.MaxCardImageSizeBytes);

        return await _manageImageService.UploadImageAsync(
            file, CurrentTenant.Id?.ToString() ?? "host", Hl25Consts.DefaultImageSubFolder);
    }

    // ===== Helpers =====

    private async Task<Hl25WheelConfig> GetOrCreateConfigAsync()
    {
        var queryable = await _configRepository.GetQueryableAsync();
        var config = await AsyncExecuter.FirstOrDefaultAsync(queryable);

        if (config == null)
        {
            config = new Hl25WheelConfig(GuidGenerator.Create(), CurrentTenant.Id)
            {
                IsActive = true,
                SlotCount = 0
            };
            config = await _configRepository.InsertAsync(config, autoSave: true);
        }

        return config;
    }

    private async Task<List<Hl25WheelSlot>> GetSlotsAsync(Guid configId)
    {
        var queryable = await _slotRepository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.WheelConfigId == configId).OrderBy(x => x.DisplayOrder));
    }

    /// <summary>Validate tổng WinRate của các ô phải = 100 (nếu có ô).</summary>
    private void ValidateWinRate(List<CreateUpdateHl25WheelSlotDto>? slots)
    {
        if (slots == null || slots.Count == 0)
            return;

        var total = slots.Sum(x => x.WinRate);
        if (Math.Abs(total - 100m) > 0.01m)
        {
            throw new BusinessException("Hl25:WheelWinRateInvalid")
                .WithData("Total", total);
        }
    }

    private Hl25WheelConfigDto MapToDto(Hl25WheelConfig config, List<Hl25WheelSlot> slots)
    {
        var dto = ObjectMapper.Map<Hl25WheelConfig, Hl25WheelConfigDto>(config);
        dto.Slots = ObjectMapper.Map<List<Hl25WheelSlot>, List<Hl25WheelSlotDto>>(slots);
        return dto;
    }

    private async Task EnsureFeatureAsync()
    {
        if (!CurrentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(AppHl25Features.Management))
            throw new AbpAuthorizationException($"Feature '{AppHl25Features.Management}' is disabled for this tenant.");
    }

    private async Task CheckViewPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Wheel.Default
            : MultiTenancyPermissions.HostAppHl25Wheel.Default;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }

    private async Task CheckEditPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Wheel.Edit
            : MultiTenancyPermissions.HostAppHl25Wheel.Edit;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }
}
