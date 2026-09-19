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
public class HlgContentAdminAppService : FeatureProtectedCrudAppService<HlgContentItem, HlgContentAdminDto, Guid, GetHlgAdminListInput, CreateHlgContentInput, UpdateHlgContentInput>, IHlgContentAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgContent.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgContent.Default;
    public HlgContentAdminAppService(IRepository<HlgContentItem, Guid> repository, ICurrentTenant tenant, IFeatureChecker features) : base(repository,tenant,features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        GetPolicyName = GetListPolicyName = TenantDefaultPermission;
        CreatePolicyName = TenantDefaultPermission + ".Create"; UpdatePolicyName = TenantDefaultPermission + ".Edit"; DeletePolicyName = TenantDefaultPermission + ".Delete";
    }
    private IRepository<T, Guid> Repo<T>() where T : class, Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();
    private void Scope(Guid? id) => HlgContentValidation.Scope(id, CurrentTenant.Id);
    public override async Task<PagedResultDto<HlgContentAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = (await Repository.GetQueryableAsync()).Where(x => x.TenantId == CurrentTenant.Id);
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Title.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive);

        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).Skip(Math.Max(0,input.SkipCount)).Take(Math.Clamp(input.MaxResultCount,1,100)));
        return new(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgContentAdminDto> GetAsync(Guid id) { await CheckGetPolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); return Map(entity); }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgContentAdminDto> CreateAsync(CreateHlgContentInput input)
    {
        await CheckCreatePolicyAsync(); await ValidateAsync(input,null);
        var entity = new HlgContentItem(GuidGenerator.Create(),CurrentTenant.Id); Apply(input,entity);
        await Repository.InsertAsync(entity,autoSave:true); return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgContentAdminDto> UpdateAsync(Guid id, UpdateHlgContentInput input)
    {
        await CheckUpdatePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);
        await ValidateAsync(input,id); Apply(input,entity);
        await Repository.UpdateAsync(entity,autoSave:true); return Map(entity);
    }
    private async Task ValidateAsync(HlgContentInput input, Guid? id)
    { HlgContentValidation.Validate(input);
        HlgContentValidation.Localized(() => { HlgContentValidation.Url(input.ImageUrl); HlgContentValidation.Url(input.TargetUrl); }, key => L[key]);
        if (input.GameId.HasValue) { var game = await Repo<HlgGame>().GetAsync(input.GameId.Value); Scope(game.TenantId); }
        if (input.Slot == Genora.MultiTenancy.Hlg.HlgContentSlot.ShareLink && string.IsNullOrWhiteSpace(input.TargetUrl)) throw new UserFriendlyException(L["Hlg:LinkRequired"]);
        if (input.IsActive && input.Slot != Genora.MultiTenancy.Hlg.HlgContentSlot.HomeBanner && await Repository.AnyAsync(x => x.TenantId == CurrentTenant.Id && x.Slot == input.Slot && x.IsActive && x.Id != id)) throw new UserFriendlyException(L["Hlg:SlotAlreadyActive"]);
 }
    public override async Task DeleteAsync(Guid id)
    { await CheckDeletePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);  await Repository.DeleteAsync(entity); }
    private static void Apply(HlgContentInput input, HlgContentItem entity) { entity.Slot = input.Slot; entity.Title = input.Title; entity.Summary = input.Summary; entity.BadgeText = input.BadgeText; entity.ImageUrl = input.ImageUrl; entity.TargetUrl = input.TargetUrl; entity.GameId = input.GameId; entity.DisplayOrder = input.DisplayOrder; entity.IsActive = input.IsActive; }
    private static HlgContentAdminDto Map(HlgContentItem entity) => new() { Id = entity.Id, Slot = entity.Slot, Title = entity.Title, Summary = entity.Summary, BadgeText = entity.BadgeText, ImageUrl = entity.ImageUrl, TargetUrl = entity.TargetUrl, GameId = entity.GameId, DisplayOrder = entity.DisplayOrder, IsActive = entity.IsActive };
}
