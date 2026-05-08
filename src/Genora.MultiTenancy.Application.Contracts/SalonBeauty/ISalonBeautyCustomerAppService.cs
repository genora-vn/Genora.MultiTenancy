using Volo.Abp.Application.Services;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.SalonBeauty;

public interface ISalonBeautyCustomerAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyCustomerDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyCustomerDto> GetAsync(Guid id);
    Task<List<SalonBeautyCustomerBookingHistoryDto>> GetBookingHistoryAsync(Guid id, int maxResultCount = 20);
    Task<List<SalonBeautyCustomerLoyaltyTransactionDto>> GetLoyaltyTransactionsAsync(Guid id, int maxResultCount = 20);
    Task<SalonBeautyCustomerDto> CreateAsync(CreateSalonBeautyCustomerDto input);
    Task<SalonBeautyCustomerDto> UpdateAsync(Guid id, UpdateSalonBeautyCustomerDto input);
    Task DeleteAsync(Guid id);
}
