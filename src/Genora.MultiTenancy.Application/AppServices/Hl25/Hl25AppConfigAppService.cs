using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Hl25;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService cấu hình chung Mini App "Dược Phẩm Hoa Linh 25 Năm" (singleton theo tenant).
/// Tự check Feature (Hl25.Management cho tenant) + dual permission (Tenant/Host).
/// </summary>
[Authorize]
public class Hl25AppConfigAppService : ApplicationService, IHl25AppConfigAppService
{
    private readonly IRepository<Hl25AppConfig, Guid> _repository;
    private readonly IManageImageService _manageImageService;
    private readonly IFeatureChecker _featureChecker;

    public Hl25AppConfigAppService(
        IRepository<Hl25AppConfig, Guid> repository,
        IManageImageService manageImageService,
        IFeatureChecker featureChecker)
    {
        _repository = repository;
        _manageImageService = manageImageService;
        _featureChecker = featureChecker;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<Hl25AppConfigDto> GetAsync()
    {
        await CheckSettingsViewPolicyAsync();

        var entity = await GetOrCreateAsync();
        return ObjectMapper.Map<Hl25AppConfig, Hl25AppConfigDto>(entity);
    }

    public async Task<Hl25AppConfigDto> UpdateAsync(CreateUpdateHl25AppConfigDto input)
    {
        await CheckSettingsEditPolicyAsync();

        var entity = await GetOrCreateAsync();
        ObjectMapper.Map(input, entity);
        entity = await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<Hl25AppConfig, Hl25AppConfigDto>(entity);
    }

    public async Task<string> UploadAssetAsync(IRemoteStreamContent file, string assetType)
    {
        await CheckSettingsEditPolicyAsync();

        if (file == null)
            throw new BusinessException("Hl25:AssetFileRequired").WithData("AssetType", assetType);

        // Tự validate 5MB (ManageImageService KHÔNG chặn size).
        var length = file.ContentLength ?? file.GetStream().Length;
        if (length > Hl25Consts.MaxCardImageSizeBytes)
        {
            throw new BusinessException("Hl25:AssetTooLarge")
                .WithData("AssetType", assetType)
                .WithData("MaxBytes", Hl25Consts.MaxCardImageSizeBytes);
        }

        var url = await _manageImageService.UploadImageAsync(
            file,
            CurrentTenant.Id?.ToString() ?? "host",
            Hl25Consts.DefaultImageSubFolder);

        return url;
    }

    private async Task<Hl25AppConfig> GetOrCreateAsync()
    {
        var queryable = await _repository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable);

        if (entity == null)
        {
            entity = new Hl25AppConfig(GuidGenerator.Create(), CurrentTenant.Id)
            {
                IsActive = true
            };
            entity = await _repository.InsertAsync(entity, autoSave: true);
        }

        return entity;
    }

    // ===== Feature + dual permission helpers =====

    private async Task EnsureFeatureAsync()
    {
        if (!CurrentTenant.IsAvailable) return;
        if (!await _featureChecker.IsEnabledAsync(AppHl25Features.Management))
            throw new AbpAuthorizationException($"Feature '{AppHl25Features.Management}' is disabled for this tenant.");
    }

    private async Task CheckSettingsViewPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Settings.Default
            : MultiTenancyPermissions.HostAppHl25Settings.Default;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }

    private async Task CheckSettingsEditPolicyAsync()
    {
        var policy = CurrentTenant.IsAvailable
            ? MultiTenancyPermissions.AppHl25Settings.Edit
            : MultiTenancyPermissions.HostAppHl25Settings.Edit;
        await AuthorizationService.CheckAsync(policy);
        await EnsureFeatureAsync();
    }
}
