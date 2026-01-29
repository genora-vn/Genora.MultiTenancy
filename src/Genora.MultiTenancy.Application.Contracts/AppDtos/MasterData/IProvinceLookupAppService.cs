using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.MasterData;
public interface IProvinceLookupAppService : IApplicationService
{
    Task<List<ProvinceLookupDto>> GetProvincesAsync(bool forceRefresh = false);
}