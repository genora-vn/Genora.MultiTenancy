
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppMembershipTiers
{
    internal class MiniAppMembershipTierService : ApplicationService, IMiniAppMembershipTierService
    {
        private readonly IRepository<MembershipTier, Guid> _repository;

        public MiniAppMembershipTierService(IRepository<MembershipTier, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<PagedResultDto<AppMembershipTierDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await _repository.GetQueryableAsync();
            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var itemDtos = ObjectMapper.Map<List<MembershipTier>, List<AppMembershipTierDto>>(items);
            return new PagedResultDto<AppMembershipTierDto>(total, itemDtos);
        }
    }
}
