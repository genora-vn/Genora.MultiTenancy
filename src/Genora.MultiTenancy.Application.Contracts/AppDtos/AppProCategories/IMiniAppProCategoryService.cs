using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppProCategories;

public interface IMiniAppProCategoryService : IApplicationService
{
    Task<MiniAppProCategoryListDto> GetListAsync();
}
