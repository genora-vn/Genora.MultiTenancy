
using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace Genora.MultiTenancy.AppServices.AppPromotionTypes
{
    public class MiniAppPromotionTypeService : ApplicationService,IMiniAppPromotionTypeService
    {
        private readonly IRepository<PromotionType, Guid> _repository;

        public MiniAppPromotionTypeService(IRepository<PromotionType, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<List<AppPromotionTypeDto>> GetAllAsync()
        {
            var query = await _repository.GetListAsync();
            
            return ObjectMapper.Map<List<PromotionType>, List<AppPromotionTypeDto>>(query);
        }
    }
}
