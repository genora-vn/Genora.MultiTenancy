using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppNews
{
    public interface IMiniAppNewsService : IApplicationService
    {
        Task<MiniAppNewsListDto> GetListAsync(GetMiniAppNewsDto input);
        Task<MiniAppNewsDetailDto> GetAsync(Guid id);
    }
}
