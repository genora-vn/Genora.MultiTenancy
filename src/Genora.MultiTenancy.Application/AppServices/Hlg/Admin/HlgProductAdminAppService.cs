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
public class HlgProductAdminAppService : FeatureProtectedCrudAppService<HlgProduct, HlgProductAdminDto, Guid, GetHlgAdminListInput, CreateHlgProductInput, UpdateHlgProductInput>, IHlgProductAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgKnowledge.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgKnowledge.Default;
    private readonly IRepository<HlgKnowledgeCategory, Guid> _categories;
    public HlgProductAdminAppService(IRepository<HlgProduct, Guid> repository, ICurrentTenant tenant, IFeatureChecker features, IRepository<HlgKnowledgeCategory, Guid> categories) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        _categories = categories;
        GetPolicyName = MultiTenancyPermissions.AppHlgKnowledge.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgKnowledge.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgKnowledge.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgKnowledge.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgKnowledge.Delete;
    }
    public override async Task<PagedResultDto<HlgProductAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Name.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        if (input.BrandId.HasValue) query = query.Where(x => x.BrandId == input.BrandId);
        if (input.ParentId.HasValue) query = query.Where(x => x.CategoryId == input.ParentId.Value);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlgProductAdminDto>(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgProductAdminDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return Map(await Repository.GetAsync(id));
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgProductAdminDto> CreateAsync(CreateHlgProductInput input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);
        var entity = new HlgProduct(GuidGenerator.Create(), input.CategoryId, input.Name.Trim(), CurrentTenant.Id);
        Apply(input, entity);
        await Repository.InsertAsync(entity, autoSave: true);
        return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgProductAdminDto> UpdateAsync(Guid id, UpdateHlgProductInput input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        await ValidateAsync(input, id);
        Apply(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        return Map(entity);
    }
    private async Task ValidateAsync(HlgProductInput input, Guid? id)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        var category = await _categories.GetAsync(input.CategoryId);
        HlgContentValidation.Scope(category.TenantId, CurrentTenant.Id);
        if (input.BrandId.HasValue) {
            var brand = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgBrand, Guid>>().GetAsync(input.BrandId.Value);
            HlgContentValidation.Scope(brand.TenantId, CurrentTenant.Id);
            if (brand.CategoryId != input.CategoryId) throw new UserFriendlyException(L["Hlg:BrandCategoryMismatch"]);
        }
        HlgContentValidation.Localized(() => { HlgContentValidation.Url(input.ThumbnailUrl); HlgContentValidation.Product(input.Details, id); }, key => L[key]);
        foreach (var relatedId in input.Details.RelatedProductIds) {
            var related = await Repository.GetAsync(relatedId); HlgContentValidation.Scope(related.TenantId, CurrentTenant.Id);
        }
        if (input.Details.GameId.HasValue) {
            var game = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame, Guid>>().GetAsync(input.Details.GameId.Value);
            HlgContentValidation.Scope(game.TenantId, CurrentTenant.Id);
        }
        await Task.CompletedTask;
    }
    private static void Apply(HlgProductInput input, HlgProduct entity)
    {
        entity.BrandId = input.BrandId;
        entity.DetailsJson = JsonSerializer.Serialize(input.Details);
        entity.CategoryId = input.CategoryId;
        entity.Name = input.Name.Trim();
        entity.ThumbnailUrl = input.ThumbnailUrl;
        entity.Summary = input.Summary;
        entity.Content = input.Content?.Trim();
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;
        entity.ImagesJson = JsonSerializer.Serialize((input.ImageUrls ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
    private static HlgProductAdminDto Map(HlgProduct entity) => new()
    {
        Id = entity.Id,
        BrandId = entity.BrandId,
        Details = JsonSerializer.Deserialize<Genora.MultiTenancy.Hlg.HlgProductContent>(entity.DetailsJson ?? "{}") ?? new(),
        CategoryId = entity.CategoryId,
        Name = entity.Name,
        ThumbnailUrl = entity.ThumbnailUrl,
        Summary = entity.Summary,
        Content = entity.Content,
        DisplayOrder = entity.DisplayOrder,
        IsActive = entity.IsActive,
        ImageUrls = string.Join("\n", JsonSerializer.Deserialize<string[]>(entity.ImagesJson ?? "[]") ?? Array.Empty<string>()),
    };
}
