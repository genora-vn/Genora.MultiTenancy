using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppMembershipTiers;

public interface IAppMembershipTierService :
        ICrudAppService<
            AppMembershipTierDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAppMembershipTierDto>
{
}