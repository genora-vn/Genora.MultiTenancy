using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppOptionExtend
{
    public interface IOptionExtendService : ICrudAppService<AppOptionExtendDto, Guid, GetListOptionExtendInput, CreateUpdateOptionExtendDto>
    {
    }
}
