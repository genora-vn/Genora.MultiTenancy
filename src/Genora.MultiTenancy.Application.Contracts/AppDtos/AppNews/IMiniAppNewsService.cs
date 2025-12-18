using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppNews
{
    public interface IMiniAppNewsService : IApplicationService
    {
        Task<PagedResultDto<AppNewsDto>> GetListAsync(GetNewsListInput input);
        Task<AppNewsDto> GetAsync(Guid id);
    }
}
