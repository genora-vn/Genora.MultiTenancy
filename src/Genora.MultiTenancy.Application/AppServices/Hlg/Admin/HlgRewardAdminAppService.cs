using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.Hlg.Admin;

/// <summary>
/// Admin CRUD cho quà HLG. Expose remote (sinh JS proxy cho DataTables). Map thủ công (không AutoMapper).
/// </summary>
[Authorize]
public class HlgRewardAdminAppService :
    FeatureProtectedCrudAppService<
        HlgReward,
        HlgRewardAdminDto,
        Guid,
        GetHlgListInput,
        CreateHlgRewardDto,
        UpdateHlgRewardDto>,
    IHlgRewardAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgRewards.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgRewards.Default;

    private readonly IRepository<HlgReward, Guid> _repository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public HlgRewardAdminAppService(
        IRepository<HlgReward, Guid> repository,
        IStringLocalizer<MultiTenancyResource> l,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        _repository = repository;
        _l = l;
        LocalizationResource = typeof(MultiTenancyResource);

        GetPolicyName = MultiTenancyPermissions.AppHlgRewards.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgRewards.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgRewards.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgRewards.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgRewards.Delete;
    }

    public override async Task<PagedResultDto<HlgRewardAdminDto>> GetListAsync(GetHlgListInput input)
    {
        await CheckGetListPolicyAsync();

        input.MaxResultCount = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var query = await _repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(keyword) || (x.VoucherCode != null && x.VoucherCode.Contains(keyword)));
        }

        if (input.IsActive.HasValue)
            query = query.Where(x => x.IsActive == input.IsActive.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.PointCost)
                 .Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<HlgRewardAdminDto>(totalCount, items.Select(MapToDto).ToList());
    }

    public override async Task<HlgRewardAdminDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return MapToDto(await _repository.GetAsync(id));
    }

    public override async Task<HlgRewardAdminDto> CreateAsync(CreateHlgRewardDto input)
    {
        await CheckCreatePolicyAsync();
        Validate(input.Name, input.PointCost, input.Type);

        var entity = new HlgReward(GuidGenerator.Create(), input.Name.Trim(), (HlgRewardType)input.Type, input.PointCost, CurrentTenant.Id)
        {
            ImageUrl = NullIfBlank(input.ImageUrl),
            StockQuantity = input.StockQuantity,
            VoucherCode = NullIfBlank(input.VoucherCode),
            DisplayOrder = input.DisplayOrder,
            IsActive = input.IsActive
        };

        return MapToDto(await _repository.InsertAsync(entity, autoSave: true));
    }

    public override async Task<HlgRewardAdminDto> UpdateAsync(Guid id, UpdateHlgRewardDto input)
    {
        await CheckUpdatePolicyAsync();
        Validate(input.Name, input.PointCost, input.Type);

        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name.Trim();
        entity.ImageUrl = NullIfBlank(input.ImageUrl);
        entity.PointCost = input.PointCost;
        entity.Type = (HlgRewardType)input.Type;
        entity.StockQuantity = input.StockQuantity;
        entity.VoucherCode = NullIfBlank(input.VoucherCode);
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;

        return MapToDto(await _repository.UpdateAsync(entity, autoSave: true));
    }

    private void Validate(string? name, int pointCost, byte type)
    {
        if (name.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("Hlg:RewardNameRequired"));
        if (pointCost < 0)
            throw new UserFriendlyException(L("Hlg:RewardPointCostInvalid"));
        if (!Enum.IsDefined(typeof(HlgRewardType), type))
            throw new UserFriendlyException(L("Hlg:RewardTypeInvalid"));
    }

    private HlgRewardAdminDto MapToDto(HlgReward e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        ImageUrl = e.ImageUrl,
        PointCost = e.PointCost,
        Type = (byte)e.Type,
        TypeText = HlgEnumMapper.RewardTypeToString(e.Type),
        StockQuantity = e.StockQuantity,
        VoucherCode = e.VoucherCode,
        DisplayOrder = e.DisplayOrder,
        IsActive = e.IsActive
    };

    private string L(string key)
    {
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? key : text;
    }

    private static string? NullIfBlank(string? v) => v.IsNullOrWhiteSpace() ? null : v!.Trim();
}
