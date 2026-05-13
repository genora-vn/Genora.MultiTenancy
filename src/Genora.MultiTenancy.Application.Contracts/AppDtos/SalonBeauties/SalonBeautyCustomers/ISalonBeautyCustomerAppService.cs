using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

public interface ISalonBeautyCustomerAppService :
    ICrudAppService<
        SalonBeautyCustomerDto,
        Guid,
        GetSalonBeautyListInput,
        CreateSalonBeautyCustomerDto,
        UpdateSalonBeautyCustomerDto>
{
    Task<List<SalonBeautyCustomerBookingHistoryDto>> GetBookingHistoryAsync(Guid id, int maxResultCount = 20);
    Task<List<SalonBeautyCustomerLoyaltyTransactionDto>> GetLoyaltyTransactionsAsync(Guid id, int maxResultCount = 20);
}
