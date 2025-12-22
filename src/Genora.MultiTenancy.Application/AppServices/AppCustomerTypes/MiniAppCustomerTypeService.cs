using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppCustomerTypes
{
    public class MiniAppCustomerTypeService : ApplicationService, IMiniAppCustomerTypeService
    {
        private readonly IRepository<CustomerType, Guid> _customerTypeRepository;

        public MiniAppCustomerTypeService(IRepository<CustomerType, Guid> customerTypeRepository)
        {
            _customerTypeRepository = customerTypeRepository;
        }

        public async Task<AppCustomerTypeDto> GetCustomerTypeByCode(string code)
        {
            var item = await _customerTypeRepository.FirstOrDefaultAsync(x => x.Code == code);
            var result = ObjectMapper.Map<CustomerType, AppCustomerTypeDto>(item);
            return result;
        }

        public async Task<PagedResultDto<AppCustomerTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await _customerTypeRepository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var dtos = ObjectMapper.Map<List<CustomerType>, List<AppCustomerTypeDto>>(items);
            return new PagedResultDto<AppCustomerTypeDto>(totalCount, dtos);
        }
    }
}
