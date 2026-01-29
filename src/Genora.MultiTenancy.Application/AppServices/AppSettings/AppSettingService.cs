using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppSettings;

[Authorize]
public class AppSettingService
    : FeatureProtectedCrudAppService<AppSetting, AppSettingDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateAppSettingDto>,
      IAppSettingService
{
    private readonly IEntityCache<AppSettingDto, Guid> _appSettingCache;
    private readonly IManageImageService _manageImageService;

    protected override string FeatureName => AppSettingFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppSettings.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppSettings.Default;

    public AppSettingService(
        IRepository<AppSetting, Guid> repository,
        IEntityCache<AppSettingDto, Guid> appSettingCache,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IManageImageService manageImageService)
        : base(repository, currentTenant, featureChecker)
    {
        _appSettingCache = appSettingCache;
        _manageImageService = manageImageService;

        GetPolicyName = MultiTenancyPermissions.AppSettings.Default;
        GetListPolicyName = MultiTenancyPermissions.AppSettings.Default;
        CreatePolicyName = MultiTenancyPermissions.AppSettings.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppSettings.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppSettings.Delete;
    }

    public override async Task<AppSettingDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return await _appSettingCache.GetAsync(id);
    }

    public async Task<PagedResultDto<AppSettingDto>> GetListWithFilterAsync(GetMiniAppSettingListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var f = input.Filter.Trim();
            queryable = queryable.Where(x =>
                x.SettingKey.Contains(f) ||
                x.SettingValue.Contains(f) ||
                (x.Description != null && x.Description.Contains(f))
            );
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(AppSetting.SettingKey)
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<AppSettingDto>(
            totalCount,
            ObjectMapper.Map<List<AppSetting>, List<AppSettingDto>>(items)
        );
    }

    private static BusinessException SettingError(string code, string field, object? value = null)
    {
        var ex = new BusinessException(code)
            .WithData("Field", field);

        if (value != null)
            ex.WithData("Value", value);

        return ex;
    }

    private static void ValidateCommon(CreateUpdateAppSettingDto input)
    {
        if (string.IsNullOrWhiteSpace(input.SettingKey))
            throw SettingError(AppSettingErrorCodes.SettingKeyRequired, "SettingKey");

        if (string.IsNullOrWhiteSpace(input.Description))
            throw SettingError(AppSettingErrorCodes.DescriptionRequired, "Description");
    }

    private static void ValidateValueOrImageOnCreate(CreateUpdateAppSettingDto input)
    {
        var hasImages = input.Images != null && input.Images.Count > 0;

        if (input.IsImageInput)
        {
            if (!hasImages)
            {
                throw SettingError(AppSettingErrorCodes.ImageRequired, "Images")
                    .WithData("IsImageInput", input.IsImageInput)
                    .WithData("SettingKey", input.SettingKey);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(input.SettingValue))
            {
                throw SettingError(AppSettingErrorCodes.SettingValueRequired, "SettingValue")
                    .WithData("IsImageInput", input.IsImageInput)
                    .WithData("SettingKey", input.SettingKey);
            }
        }
    }

    private static void ValidateValueOrImageOnUpdate(CreateUpdateAppSettingDto input)
    {
        var hasImages = input.Images != null && input.Images.Count > 0;

        if (input.IsImageInput)
        {
            if (!hasImages && string.IsNullOrWhiteSpace(input.SettingValue))
            {
                throw SettingError(AppSettingErrorCodes.ImageOrKeepValueRequired, "Images")
                    .WithData("IsImageInput", input.IsImageInput)
                    .WithData("SettingKey", input.SettingKey);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(input.SettingValue))
            {
                throw SettingError(AppSettingErrorCodes.SettingValueRequired, "SettingValue")
                    .WithData("IsImageInput", input.IsImageInput)
                    .WithData("SettingKey", input.SettingKey);
            }
        }
    }

    public override async Task<AppSettingDto> CreateAsync(CreateUpdateAppSettingDto input)
    {
        await CheckCreatePolicyAsync();

        ValidateCommon(input);
        ValidateValueOrImageOnCreate(input);

        if (input.Images != null && input.Images.Count > 0)
        {
            List<CreateUpdateAppSettingDto> inputs = new();

            foreach (var image in input.Images)
            {
                var upload = await _manageImageService.UploadImageAsync(image, CurrentTenant.Id.ToString());
                if (upload != null)
                {
                    var dto = new CreateUpdateAppSettingDto
                    {
                        SettingKey = input.SettingKey,
                        SettingType = input.SettingType,
                        SettingValue = upload,
                        Description = input.Description,
                        IsActive = input.IsActive,
                        IsImageInput = input.IsImageInput
                    };
                    inputs.Add(dto);
                }
            }

            var entities = ObjectMapper.Map<List<CreateUpdateAppSettingDto>, List<AppSetting>>(inputs);
            await Repository.InsertManyAsync(entities, autoSave: true);

            return ObjectMapper.Map<AppSetting, AppSettingDto>(entities.FirstOrDefault());
        }
        else
        {
            var entity = ObjectMapper.Map<CreateUpdateAppSettingDto, AppSetting>(input);
            entity = await Repository.InsertAsync(entity, autoSave: true);
            return ObjectMapper.Map<AppSetting, AppSettingDto>(entity);
        }
    }

    public override async Task<AppSettingDto> UpdateAsync(Guid id, CreateUpdateAppSettingDto input)
    {
        await CheckUpdatePolicyAsync();

        ValidateCommon(input);
        ValidateValueOrImageOnUpdate(input);

        if (input.Images != null && input.Images.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(input.SettingValue))
            {
                await _manageImageService.DeleteFileAsync(input.SettingValue);
            }

            var upload = await _manageImageService.UploadImageAsync(input.Images.FirstOrDefault(), CurrentTenant.Id.ToString());
            input.SettingValue = upload;
        }

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);
        entity = await Repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<AppSetting, AppSettingDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        await Repository.DeleteAsync(id);
    }
}
