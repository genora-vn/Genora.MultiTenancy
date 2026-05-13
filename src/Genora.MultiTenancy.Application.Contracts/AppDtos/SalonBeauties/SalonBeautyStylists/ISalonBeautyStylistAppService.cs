using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;

public interface ISalonBeautyStylistAppService :
    ICrudAppService<
        SalonBeautyStylistDto,
        Guid,
        GetSalonBeautyListInput,
        CreateSalonBeautyStylistDto,
        UpdateSalonBeautyStylistDto>
{
    Task UpdateShowOnAppAsync(Guid id, bool isShowOnApp);
}
