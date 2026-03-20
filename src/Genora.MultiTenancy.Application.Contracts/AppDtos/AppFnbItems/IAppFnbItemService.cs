using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public interface IAppFnbItemService :
    ICrudAppService<
        FnbItemDto,
        Guid,
        GetFnbItemListInput,
        CreateUpdateFnbItemDto>
{
}