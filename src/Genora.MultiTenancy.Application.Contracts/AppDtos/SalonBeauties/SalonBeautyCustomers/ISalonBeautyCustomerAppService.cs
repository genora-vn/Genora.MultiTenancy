using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

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
