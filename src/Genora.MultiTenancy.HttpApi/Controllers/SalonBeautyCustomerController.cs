using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.SalonBeauty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Genora.MultiTenancy.HttpApi.Controllers;

[ApiController]
[Route("api/app/salon-beauty/[controller]")]
[Authorize]
public class SalonBeautyCustomerController : AbpController
{
    private readonly ISalonBeautyCustomerAppService _service;

    public SalonBeautyCustomerController(ISalonBeautyCustomerAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<PagedResultDto<SalonBeautyCustomerDto>> GetListAsync([FromQuery] GetSalonBeautyListInput input)
    {
        return await _service.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public async Task<SalonBeautyCustomerDto> GetAsync(Guid id)
    {
        return await _service.GetAsync(id);
    }

    [HttpGet("{id}/booking-history")]
    public async Task<List<SalonBeautyCustomerBookingHistoryDto>> GetBookingHistoryAsync(Guid id, [FromQuery] int maxResultCount = 20)
    {
        return await _service.GetBookingHistoryAsync(id, maxResultCount);
    }

    [HttpGet("{id}/loyalty-transactions")]
    public async Task<List<SalonBeautyCustomerLoyaltyTransactionDto>> GetLoyaltyTransactionsAsync(Guid id, [FromQuery] int maxResultCount = 20)
    {
        return await _service.GetLoyaltyTransactionsAsync(id, maxResultCount);
    }

    [HttpPost]
    [Authorize(MultiTenancyPermissions.SalonBeautyCustomers.Create)]
    public async Task<SalonBeautyCustomerDto> CreateAsync([FromBody] CreateSalonBeautyCustomerDto input)
    {
        return await _service.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyCustomers.Edit)]
    public async Task<SalonBeautyCustomerDto> UpdateAsync(Guid id, [FromBody] UpdateSalonBeautyCustomerDto input)
    {
        return await _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyCustomers.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
    }
}
