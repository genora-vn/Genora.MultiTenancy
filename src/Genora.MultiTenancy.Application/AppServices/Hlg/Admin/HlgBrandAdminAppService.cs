using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
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
public class HlgBrandAdminAppService : FeatureProtectedCrudAppService<HlgBrand, HlgBrandAdminDto, Guid, GetHlgAdminListInput, CreateHlgBrandInput, UpdateHlgBrandInput>, IHlgBrandAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgKnowledge.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgKnowledge.Default;
    public HlgBrandAdminAppService(IRepository<HlgBrand, Guid> repository, ICurrentTenant tenant, IFeatureChecker features) : base(repository,tenant,features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        GetPolicyName = GetListPolicyName = TenantDefaultPermission;
        CreatePolicyName = TenantDefaultPermission + ".Create"; UpdatePolicyName = TenantDefaultPermission + ".Edit"; DeletePolicyName = TenantDefaultPermission + ".Delete";
    }
    private IRepository<T, Guid> Repo<T>() where T : class, Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();
    private void Scope(Guid? id) => HlgContentValidation.Scope(id, CurrentTenant.Id);
    public override async Task<PagedResultDto<HlgBrandAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = (await Repository.GetQueryableAsync()).Where(x => x.TenantId == CurrentTenant.Id);
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Name.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive);
        if (input.ParentId.HasValue) query = query.Where(x => x.CategoryId == input.ParentId);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).Skip(Math.Max(0,input.SkipCount)).Take(Math.Clamp(input.MaxResultCount,1,100)));
        return new(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgBrandAdminDto> GetAsync(Guid id) { await CheckGetPolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); return Map(entity); }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgBrandAdminDto> CreateAsync(CreateHlgBrandInput input)
    {
        await CheckCreatePolicyAsync(); await ValidateAsync(input,null);
        var entity = new HlgBrand(GuidGenerator.Create(),CurrentTenant.Id); Apply(input,entity);
        await Repository.InsertAsync(entity,autoSave:true); return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgBrandAdminDto> UpdateAsync(Guid id, UpdateHlgBrandInput input)
    {
        await CheckUpdatePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);
        await ValidateAsync(input,id); Apply(input,entity);
        await Repository.UpdateAsync(entity,autoSave:true); return Map(entity);
    }
    private async Task ValidateAsync(HlgBrandInput input, Guid? id)
    { HlgContentValidation.Validate(input);
        var category = await Repo<HlgKnowledgeCategory>().GetAsync(input.CategoryId); Scope(category.TenantId);
        if (id.HasValue && await Repo<HlgProduct>().AnyAsync(x => x.BrandId == id && x.CategoryId != input.CategoryId)) throw new UserFriendlyException(L["Hlg:BrandCategoryInUse"]);
 }
    public override async Task DeleteAsync(Guid id)
    { await CheckDeletePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); if (await Repo<HlgProduct>().AnyAsync(x => x.BrandId == id)) throw new UserFriendlyException(L["Hlg:BrandHasProducts"]); await Repository.DeleteAsync(entity); }
    private static void Apply(HlgBrandInput input, HlgBrand entity) { entity.CategoryId = input.CategoryId; entity.Name = input.Name; entity.DisplayOrder = input.DisplayOrder; entity.IsActive = input.IsActive; }
    private static HlgBrandAdminDto Map(HlgBrand entity) => new() { Id = entity.Id, CategoryId = entity.CategoryId, Name = entity.Name, DisplayOrder = entity.DisplayOrder, IsActive = entity.IsActive };
}
