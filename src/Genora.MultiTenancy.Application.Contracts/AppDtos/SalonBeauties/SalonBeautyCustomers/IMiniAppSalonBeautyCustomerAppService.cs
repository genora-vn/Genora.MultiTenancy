using System;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;

public interface IMiniAppSalonBeautyCustomerAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyCustomerDto>> GetListMiniAppAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyCustomerDto> GetMiniAppAsync(Guid id);
    Task<SalonBeautyCustomerDto?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default);
    Task<SalonBeautyCustomerDto> UpsertFromMiniAppAsync(MiniAppSalonBeautyUpsertCustomerRequest input, CancellationToken ct = default);
}
