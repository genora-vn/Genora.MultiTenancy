using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;
[Authorize]
public class HlgCategoryAdminAppService : FeatureProtectedCrudAppService<HlgKnowledgeCategory, HlgCategoryAdminDto, Guid, GetHlgAdminListInput, CreateHlgCategoryInput, UpdateHlgCategoryInput>, IHlgCategoryAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgKnowledge.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgKnowledge.Default;
    private readonly IRepository<HlgProduct, Guid> _products;
    public HlgCategoryAdminAppService(IRepository<HlgKnowledgeCategory, Guid> repository, ICurrentTenant tenant, IFeatureChecker features, IRepository<HlgProduct, Guid> products) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        _products = products;
        GetPolicyName = MultiTenancyPermissions.AppHlgKnowledge.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgKnowledge.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgKnowledge.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgKnowledge.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgKnowledge.Delete;
    }
    public override async Task<PagedResultDto<HlgCategoryAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Name.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlgCategoryAdminDto>(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgCategoryAdminDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return Map(await Repository.GetAsync(id));
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgCategoryAdminDto> CreateAsync(CreateHlgCategoryInput input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);
        var entity = new HlgKnowledgeCategory(GuidGenerator.Create(), input.Name.Trim(), CurrentTenant.Id);
        Apply(input, entity);
        await Repository.InsertAsync(entity, autoSave: true);
        return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgCategoryAdminDto> UpdateAsync(Guid id, UpdateHlgCategoryInput input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        await ValidateAsync(input, id);
        Apply(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        return Map(entity);
    }
    private async Task ValidateAsync(HlgCategoryInput input, Guid? id)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        HlgContentValidation.Localized(() => HlgContentValidation.Url(input.ImageUrl), key => L[key]);
        await Task.CompletedTask;
    }
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        if (await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgBrand,Guid>>().AnyAsync(x => x.CategoryId == id)) throw new UserFriendlyException(L["Hlg:CategoryHasBrands"]);
        if (await _products.AnyAsync(x => x.CategoryId == id)) throw new UserFriendlyException(L["Hlg:CategoryHasProducts"]);
        await Repository.DeleteAsync(id);
    }
    private static void Apply(HlgCategoryInput input, HlgKnowledgeCategory entity)
    {
        entity.Name = input.Name.Trim();
        entity.Description = input.Description;
        entity.ImageUrl = input.ImageUrl;
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;
    }
    private static HlgCategoryAdminDto Map(HlgKnowledgeCategory entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        ImageUrl = entity.ImageUrl,
        DisplayOrder = entity.DisplayOrder,
        IsActive = entity.IsActive,
    };
}
