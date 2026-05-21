using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;

public interface IMiniAppSalonBeautyLocationAppService : IApplicationService
{
    Task<List<MiniAppSalonBeautyLocationDto>> GetListAsyncLocations(GetMiniAppLocationListInput input);
}
