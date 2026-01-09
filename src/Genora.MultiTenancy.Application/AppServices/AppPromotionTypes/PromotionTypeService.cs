using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppPromotionTypes;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppPromotionTypes
{
    [Authorize]
    public class PromotionTypeService : FeatureProtectedCrudAppService<PromotionType, AppPromotionTypeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePromotionTypeDto>, IPromotionTypeService
    {
        protected override string FeatureName => AppPromotionTypeFeature.Management;

        protected override string TenantDefaultPermission => MultiTenancyPermissions.AppPromotionType.Default;

        protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppPromotionType.Default;
        public PromotionTypeService(IRepository<PromotionType, Guid> repository, ICurrentTenant currentTenant, IFeatureChecker featureChecker) : base(repository, currentTenant, featureChecker)
        {
            GetPolicyName = MultiTenancyPermissions.AppPromotionType.Default;
            GetListPolicyName = MultiTenancyPermissions.AppPromotionType.Default;
            CreatePolicyName = MultiTenancyPermissions.AppPromotionType.Create;
            UpdatePolicyName = MultiTenancyPermissions.AppPromotionType.Edit;
            DeletePolicyName = MultiTenancyPermissions.AppPromotionType.Delete;
        }



        public override async Task<PagedResultDto<AppPromotionTypeDto>> GetListAsync (PagedAndSortedResultRequestDto input)
        {
            await CheckGetListPolicyAsync();
            var queryable = await Repository.GetQueryableAsync();

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(PromotionType.CreationTime) + " desc"
                : input.Sorting;
            var query = queryable
           .OrderBy(sorting)
           .Skip(input.SkipCount)
           .Take(input.MaxResultCount);

            var items = await AsyncExecuter.ToListAsync(query);
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            return new PagedResultDto<AppPromotionTypeDto>(totalCount, ObjectMapper.Map<List<PromotionType>, List<AppPromotionTypeDto>>(items));
        }
    }
}
