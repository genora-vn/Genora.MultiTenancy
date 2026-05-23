using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyaltyBonusTiers;
using Genora.MultiTenancy.AppServices.SalonBeauty;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyLoyaltyBonusTierAppService :
    FeatureProtectedCrudAppService<
        SalonBeautyLoyaltyBonusTier,
        SalonBeautyLoyaltyBonusTierDto,
        Guid,
        GetSalonBeautyLoyaltyBonusTierListInput,
        CreateSalonBeautyLoyaltyBonusTierDto,
        UpdateSalonBeautyLoyaltyBonusTierDto>,
    ISalonBeautyLoyaltyBonusTierAppService
{
    protected override string FeatureName => string.Empty;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default;

    private readonly IRepository<SalonBeautyLoyaltyBonusTier, Guid> _repository;

    public SalonBeautyLoyaltyBonusTierAppService(
        IRepository<SalonBeautyLoyaltyBonusTier, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        _repository = repository;
    }

    public override async Task<PagedResultDto<SalonBeautyLoyaltyBonusTierDto>> GetListAsync(GetSalonBeautyLoyaltyBonusTierListInput input)
    {
        await CheckTierPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default,
            MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 50 : Math.Min(input.MaxResultCount, 200);

        var query = await _repository.GetQueryableAsync();
        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText!.Trim();
            query = query.Where(x => x.Name.Contains(keyword) || (x.Description != null && x.Description.Contains(keyword)));
        }
        if (input.IsActive.HasValue)
            query = query.Where(x => x.IsActive == input.IsActive.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.MinAmount)
                 .Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyLoyaltyBonusTierDto>(totalCount, items.Select(MapToDto).ToList());
    }

    public override async Task<SalonBeautyLoyaltyBonusTierDto> GetAsync(Guid id)
    {
        await CheckTierPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default,
            MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default);

        return MapToDto(await _repository.GetAsync(id));
    }

    public override async Task<SalonBeautyLoyaltyBonusTierDto> CreateAsync(CreateSalonBeautyLoyaltyBonusTierDto input)
    {
        await CheckTierPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Edit,
            MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Edit);

        Validate(input.Name, input.MinAmount, input.BonusPoint);

        var entity = new SalonBeautyLoyaltyBonusTier(
            GuidGenerator.Create(),
            input.Name.Trim(),
            input.MinAmount,
            input.BonusPoint)
        {
            TenantId = CurrentTenant.Id,
            Description = NullIfWhiteSpace(input.Description),
            IsActive = input.IsActive,
            DisplayOrder = input.DisplayOrder
        };

        await _repository.InsertAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public override async Task<SalonBeautyLoyaltyBonusTierDto> UpdateAsync(Guid id, UpdateSalonBeautyLoyaltyBonusTierDto input)
    {
        await CheckTierPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Edit,
            MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Edit);

        Validate(input.Name, input.MinAmount, input.BonusPoint);

        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name.Trim();
        entity.MinAmount = input.MinAmount;
        entity.BonusPoint = input.BonusPoint;
        entity.Description = NullIfWhiteSpace(input.Description);
        entity.IsActive = input.IsActive;
        entity.DisplayOrder = input.DisplayOrder;

        await _repository.UpdateAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckTierPolicyAsync(
            MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Edit,
            MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Edit);

        await _repository.DeleteAsync(id, autoSave: true);
    }

    private static void Validate(string? name, decimal minAmount, int bonusPoint)
    {
        if (name.IsNullOrWhiteSpace())
            throw new UserFriendlyException("Tên mốc nạp tiền không được để trống.");
        if (minAmount <= 0)
            throw new UserFriendlyException("Số tiền tối thiểu phải > 0.");
        if (bonusPoint < 0)
            throw new UserFriendlyException("Số điểm bonus không được < 0.");
    }

    private async Task CheckTierPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing loyalty config permission.");
        await AuthorizationService.CheckAsync(permission);
    }

    private static SalonBeautyLoyaltyBonusTierDto MapToDto(SalonBeautyLoyaltyBonusTier x)
        => new()
        {
            Id = x.Id,
            Name = x.Name,
            MinAmount = x.MinAmount,
            BonusPoint = x.BonusPoint,
            Description = x.Description,
            IsActive = x.IsActive,
            DisplayOrder = x.DisplayOrder,
            CreationTime = x.CreationTime,
            CreatorId = x.CreatorId,
            LastModificationTime = x.LastModificationTime,
            LastModifierId = x.LastModifierId
        };

    private static string? NullIfWhiteSpace(string? value)
        => value.IsNullOrWhiteSpace() ? null : value!.Trim();
}
