using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyaltyBonusTiers;

public interface ISalonBeautyLoyaltyBonusTierAppService :
    ICrudAppService<
        SalonBeautyLoyaltyBonusTierDto,
        Guid,
        GetSalonBeautyLoyaltyBonusTierListInput,
        CreateSalonBeautyLoyaltyBonusTierDto,
        UpdateSalonBeautyLoyaltyBonusTierDto>
{
}
