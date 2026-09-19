using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;
[Authorize]
public class HlgFulfillmentAdminAppService : ApplicationService, IHlgFulfillmentAdminAppService
{
    public HlgFulfillmentAdminAppService() { LocalizationResource=typeof(MultiTenancyResource); }
    private IRepository<T,Guid> Repo<T>() where T:class,Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T,Guid>>();
    private async Task CheckAsync(bool edit=false) {
        await AuthorizationService.CheckAsync("MultiTenancy."+(CurrentTenant.IsAvailable?"AppHlg":"HostAppHlg")+"Rewards"+(edit?".Edit":""));
        if(CurrentTenant.IsAvailable && !await LazyServiceProvider.LazyGetRequiredService<IFeatureChecker>().IsEnabledAsync(AppHlgFeatures.Management)) throw new AbpAuthorizationException();
    }
    private async Task<IQueryable<HlgFulfillmentDto>> QueryAsync() {
        var q=await Repo<HlgRewardHistory>().GetQueryableAsync(); var customers=await Repo<Customer>().GetQueryableAsync(); var addresses=await Repo<HlgShippingAddress>().GetQueryableAsync();
        return from h in q join c in customers on h.CustomerId equals c.Id
            join a in addresses on h.ShippingAddressId equals (Guid?)a.Id into aa from a in aa.DefaultIfEmpty()
            where h.TenantId==CurrentTenant.Id
            select new HlgFulfillmentDto { Id=h.Id,CustomerId=h.CustomerId,CustomerName=c.FullName,RewardName=h.RewardName,
                ReceiverName=a!=null && a.CustomerId==h.CustomerId?a.ReceiverName:null,Phone=a!=null && a.CustomerId==h.CustomerId?a.Phone:null,
                Address=a!=null && a.CustomerId==h.CustomerId?a.Address:null,Status=h.Status,CreationTime=h.CreationTime };
    }
    public virtual async Task<PagedResultDto<HlgFulfillmentDto>> GetListAsync(GetHlgAdminListInput input) {
        await CheckAsync(); var q=await QueryAsync();
        if(!string.IsNullOrWhiteSpace(input.FilterText)) { var term=input.FilterText.Trim(); q=q.Where(x=>x.CustomerName.Contains(term)||x.RewardName.Contains(term)); }
        if(input.Status.HasValue) q=q.Where(x=>(byte)x.Status==input.Status);
        return new(await AsyncExecuter.CountAsync(q),await AsyncExecuter.ToListAsync(q.OrderByDescending(x=>x.CreationTime).ThenBy(x=>x.Id).Skip(Math.Max(0,input.SkipCount)).Take(Math.Clamp(input.MaxResultCount,1,100))));
    }
    public virtual async Task<HlgFulfillmentDto> GetAsync(Guid id) { await CheckAsync(); return await AsyncExecuter.FirstOrDefaultAsync((await QueryAsync()).Where(x=>x.Id==id)) ?? throw new UserFriendlyException(L["Hlg:NotFound"]); }
    public virtual async Task UpdateAsync(Guid id,HlgFulfillmentInput input) {
        await CheckAsync(true); HlgContentValidation.Validate(input);
        var item=await Repo<HlgRewardHistory>().GetAsync(id); HlgContentValidation.Scope(item.TenantId,CurrentTenant.Id);
        if(!CanTransition(item.Status,input.Status,item.RewardType)) throw new UserFriendlyException(L["Hlg:InvalidStatusTransition"]);
        item.Status=input.Status; await Repo<HlgRewardHistory>().UpdateAsync(item,autoSave:true);
    }
    public static bool CanTransition(HlgRewardHistoryStatus current,HlgRewardHistoryStatus next,HlgRewardType type) => current==next ||
        (type==HlgRewardType.Physical && ((current==HlgRewardHistoryStatus.Pending && next==HlgRewardHistoryStatus.Shipping) || (current==HlgRewardHistoryStatus.Shipping && next==HlgRewardHistoryStatus.Delivered))) ||
        (type==HlgRewardType.Voucher && current==HlgRewardHistoryStatus.Pending && next==HlgRewardHistoryStatus.Done);
}
