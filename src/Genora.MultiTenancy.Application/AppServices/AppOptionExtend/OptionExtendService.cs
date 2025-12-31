using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppOptionExtend
{
    public class OptionExtendService : FeatureProtectedCrudAppService<OptionExtend, AppOptionExtendDto, Guid, GetListOptionExtendInput, CreateUpdateOptionExtendDto>, IOptionExtendService
    {
        public OptionExtendService(IRepository<OptionExtend, Guid> repository, ICurrentTenant currentTenant, IFeatureChecker featureChecker) : base(repository, currentTenant, featureChecker)
        {
        }

        protected override string FeatureName => throw new NotImplementedException();

        protected override string TenantDefaultPermission => throw new NotImplementedException();

        protected override string HostDefaultPermission => throw new NotImplementedException();

        public override async Task<AppOptionExtendDto> CreateAsync(CreateUpdateOptionExtendDto input)
        {
            var query = await Repository.GetQueryableAsync();
            var lastOption = query.Where(o => o.Type == input.Type).OrderByDescending(x => x.OptionId).FirstOrDefault() ?? null;
            int optionId = input.OptionId == 0? (int)(lastOption?.OptionId + 1) : input.OptionId;
            var newOption = new OptionExtend(optionId, input.OptionName, input.Type, input.Description);
            await Repository.InsertAsync(newOption);
            return ObjectMapper.Map<OptionExtend, AppOptionExtendDto>(newOption);
        }
        public async Task<List<GolfCourseUtilityDto>> GetUtilitiesAsync()
        {
            var query = await Repository.GetQueryableAsync();
            var utilities = query.Where(x => x.Type == OptionExtendTypeEnum.GolfCourseUlitity.Value).Select(x => new GolfCourseUtilityDto { UtilityId = x.OptionId, UtilityName = x.OptionName}).ToList();
            return utilities;
        }
    }
}
