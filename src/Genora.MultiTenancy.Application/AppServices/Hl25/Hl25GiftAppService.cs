using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppHl25Features;
using Genora.MultiTenancy.Hl25;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
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
/// AppService quản lý kho quà tặng (Hl25Gift) — CRUD + upload ảnh + tự set OutOfStock khi hết hàng.
/// </summary>
[Authorize]
public class Hl25GiftAppService :
    FeatureProtectedCrudAppService<Hl25Gift, Hl25GiftDto, Guid, GetHl25GiftListInput, CreateUpdateHl25GiftDto>,
    IHl25GiftAppService
{
    protected override string FeatureName => AppHl25Features.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHl25Wheel.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHl25Wheel.Default;

    private readonly IManageImageService _manageImageService;

    public Hl25GiftAppService(
        IRepository<Hl25Gift, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IManageImageService manageImageService)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppHl25Wheel.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHl25Wheel.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHl25Wheel.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHl25Wheel.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHl25Wheel.Delete;

        _manageImageService = manageImageService;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<Hl25GiftDto>> GetListAsync(GetHl25GiftListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(filter));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Hl25Gift.Name) : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<Hl25GiftDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<Hl25Gift>, System.Collections.Generic.List<Hl25GiftDto>>(items));
    }

    public override async Task<Hl25GiftDto> CreateAsync(CreateUpdateHl25GiftDto input)
    {
        await CheckCreatePolicyAsync();

        NormalizeStatus(input);

        var entity = new Hl25Gift(GuidGenerator.Create(), input.Name, CurrentTenant.Id);
        MapToEntity(input, entity);

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<Hl25Gift, Hl25GiftDto>(entity);
    }

    public override async Task<Hl25GiftDto> UpdateAsync(Guid id, CreateUpdateHl25GiftDto input)
    {
        await CheckUpdatePolicyAsync();

        NormalizeStatus(input);

        var entity = await Repository.GetAsync(id);
        MapToEntity(input, entity);

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<Hl25Gift, Hl25GiftDto>(entity);
    }

    public async Task<string> UploadGiftImageAsync(IRemoteStreamContent file)
    {
        await CheckUpdatePolicyAsync();

        if (file == null)
            throw new BusinessException("Hl25:GiftImageRequired");

        var length = file.ContentLength ?? file.GetStream().Length;
        if (length > Hl25Consts.MaxCardImageSizeBytes)
            throw new BusinessException("Hl25:AssetTooLarge").WithData("MaxBytes", Hl25Consts.MaxCardImageSizeBytes);

        return await _manageImageService.UploadImageAsync(
            file, CurrentTenant.Id?.ToString() ?? "host", Hl25Consts.DefaultImageSubFolder);
    }

    private static void MapToEntity(CreateUpdateHl25GiftDto input, Hl25Gift entity)
    {
        entity.Name = input.Name;
        entity.ImageUrl = input.ImageUrl;
        entity.Description = input.Description;
        entity.TotalQuantity = input.TotalQuantity;
        entity.RemainingQuantity = input.RemainingQuantity;
        entity.Value = input.Value;
        entity.Status = input.Status;
    }

    /// <summary>Tự set OutOfStock khi hết hàng (trừ khi bị Disabled thủ công).</summary>
    private static void NormalizeStatus(CreateUpdateHl25GiftDto input)
    {
        if (input.Status == Hl25GiftStatus.Disabled)
            return;

        input.Status = input.RemainingQuantity <= 0
            ? Hl25GiftStatus.OutOfStock
            : Hl25GiftStatus.Available;
    }
}
