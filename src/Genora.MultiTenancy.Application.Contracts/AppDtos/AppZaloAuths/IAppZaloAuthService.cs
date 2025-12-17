using Genora.MultiTenancy.AppDtos.ZaloAuths;
using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IAppZaloAuthAppService :
    ICrudAppService<
        AppZaloAuthDto,
        Guid,
        GetAppZaloAuthListInput,
        CreateUpdateZaloAuthDto>
{
}