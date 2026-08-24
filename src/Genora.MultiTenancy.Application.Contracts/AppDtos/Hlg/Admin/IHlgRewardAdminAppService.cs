using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hlg.Admin;

/// <summary>
/// Admin CRUD service cho quà HLG. Expose remote để sinh JS proxy cho DataTables.
/// </summary>
public interface IHlgRewardAdminAppService :
    ICrudAppService<
        HlgRewardAdminDto,
        Guid,
        GetHlgListInput,
        CreateHlgRewardDto,
        UpdateHlgRewardDto>
{
}
