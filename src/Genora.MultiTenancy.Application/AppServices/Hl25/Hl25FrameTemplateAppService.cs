using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Hl25;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService quản lý mẫu frame (Hl25FrameTemplate) — CRUD + upload ảnh mẫu frame.
/// </summary>
[Authorize]
public class Hl25FrameTemplateAppService :
    FeatureProtectedCrudAppService<Hl25FrameTemplate, Hl25FrameTemplateDto, Guid, GetHl25FrameTemplateListInput, CreateUpdateHl25FrameTemplateDto>,
    IHl25FrameTemplateAppService
{
    protected override string FeatureName => AppHl25Features.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHl25Frames.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHl25Frames.Default;

    private readonly IManageImageService _manageImageService;

    private readonly Hl25MiniAppCacheInvalidator _cacheInvalidator;

    public Hl25FrameTemplateAppService(
        IRepository<Hl25FrameTemplate, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IManageImageService manageImageService,
        Hl25MiniAppCacheInvalidator cacheInvalidator)
        : base(repository, currentTenant, featureChecker)
    {
        _cacheInvalidator = cacheInvalidator;
        GetPolicyName = MultiTenancyPermissions.AppHl25Frames.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHl25Frames.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHl25Frames.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHl25Frames.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHl25Frames.Delete;

        _manageImageService = manageImageService;
    }

    public override async Task DeleteAsync(Guid id)
    {
        await base.DeleteAsync(id);
        await _cacheInvalidator.AfterCommitAsync(CurrentTenant.Id, Hl25CacheArea.FrameCampaigns, Hl25CacheArea.FrameTemplates);
    }

    [DisableValidation]
    public override async Task<PagedResultDto<Hl25FrameTemplateDto>> GetListAsync(GetHl25FrameTemplateListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (input.CampaignId.HasValue)
        {
            query = query.Where(x => x.CampaignId == input.CampaignId.Value);
        }

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(filter));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(Hl25FrameTemplate.DisplayOrder)
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<Hl25FrameTemplateDto>(
            totalCount,
            ObjectMapper.Map<List<Hl25FrameTemplate>, List<Hl25FrameTemplateDto>>(items));
    }

    public override async Task<Hl25FrameTemplateDto> CreateAsync(CreateUpdateHl25FrameTemplateDto input)
    {
        await CheckCreatePolicyAsync();

        var entity = new Hl25FrameTemplate(GuidGenerator.Create(), input.CampaignId, input.Name, input.ImageUrl, CurrentTenant.Id)
        {
            ThumbnailUrl = input.ThumbnailUrl,
            DisplayOrder = input.DisplayOrder,
            IsActive = input.IsActive
        };

        entity = await Repository.InsertAsync(entity, autoSave: true);
        await _cacheInvalidator.AfterCommitAsync(CurrentTenant.Id, Hl25CacheArea.FrameCampaigns, Hl25CacheArea.FrameTemplates);
        return ObjectMapper.Map<Hl25FrameTemplate, Hl25FrameTemplateDto>(entity);
    }

    public override async Task<Hl25FrameTemplateDto> UpdateAsync(Guid id, CreateUpdateHl25FrameTemplateDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);
        entity.CampaignId = input.CampaignId;
        entity.Name = input.Name;
        entity.ImageUrl = input.ImageUrl;
        entity.ThumbnailUrl = input.ThumbnailUrl;
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        await _cacheInvalidator.AfterCommitAsync(CurrentTenant.Id, Hl25CacheArea.FrameCampaigns, Hl25CacheArea.FrameTemplates);
        return ObjectMapper.Map<Hl25FrameTemplate, Hl25FrameTemplateDto>(entity);
    }

    public async Task<string> UploadTemplateImageAsync(IRemoteStreamContent file)
    {
        await CheckUpdatePolicyAsync();

        if (file == null)
            throw new BusinessException("Hl25:TemplateImageRequired");

        var length = file.ContentLength ?? file.GetStream().Length;
        if (length > Hl25Consts.MaxCardImageSizeBytes)
            throw new BusinessException("Hl25:AssetTooLarge").WithData("MaxBytes", Hl25Consts.MaxCardImageSizeBytes);

        return await _manageImageService.UploadImageAsync(
            file, CurrentTenant.Id?.ToString() ?? "host", Hl25Consts.DefaultImageSubFolder);
    }
}
