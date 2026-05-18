using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppPromotionPolicies;

public interface IAppPromotionPolicyService :
    ICrudAppService<
        AppPromotionPolicyDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAppPromotionPolicyDto>
{
    Task<CreateUpdateAppPromotionPolicyDto> GetEditDataAsync(Guid? id);
}
