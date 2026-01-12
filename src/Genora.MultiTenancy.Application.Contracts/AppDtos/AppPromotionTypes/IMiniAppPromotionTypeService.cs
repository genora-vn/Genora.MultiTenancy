

using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppPromotionTypes
{
    public interface IMiniAppPromotionTypeService : IApplicationService
    {
        Task<List<AppPromotionTypeDto>> GetAllAsync();
    }
}
