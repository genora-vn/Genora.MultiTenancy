using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppNews;
public interface IAppNewsService :
        ICrudAppService<
            AppNewsDto,
            Guid,
            GetNewsListInput,
            CreateUpdateAppNewsDto>
{
}