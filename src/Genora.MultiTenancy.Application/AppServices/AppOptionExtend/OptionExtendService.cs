using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using System;
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
    }
}
