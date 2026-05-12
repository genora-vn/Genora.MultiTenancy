using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;

namespace Genora.MultiTenancy.HttpApi.Controllers;

[ApiController]
[Route("api/app/salon-beauty")]
[Authorize]
public class SalonBeautyController : AbpController
{
    private readonly ISalonBeautyCustomerAppService _customerService;
    private readonly ISalonBeautyServiceCategoryAppService _categoryService;
    private readonly ISalonBeautyServiceAppService _serviceService;
    private readonly ISalonBeautyStylistAppService _stylistService;
    private readonly ISalonBeautyBookingAppService _bookingService;
    private readonly ISalonBeautyLoyaltyAppService _loyaltyService;

    public SalonBeautyController(
        ISalonBeautyCustomerAppService customerService,
        ISalonBeautyServiceCategoryAppService categoryService,
        ISalonBeautyServiceAppService serviceService,
        ISalonBeautyStylistAppService stylistService,
        ISalonBeautyBookingAppService bookingService,
        ISalonBeautyLoyaltyAppService loyaltyService)
    {
        _customerService = customerService;
        _categoryService = categoryService;
        _serviceService = serviceService;
        _stylistService = stylistService;
        _bookingService = bookingService;
        _loyaltyService = loyaltyService;
    }

    #region Customers
    [HttpGet("customers")]
    public async Task<PagedResultDto<SalonBeautyCustomerDto>> GetCustomersAsync([FromQuery] GetSalonBeautyListInput input)
    {
        return await _customerService.GetListAsync(input);
    }

    [HttpGet("customers/{id}")]
    public async Task<SalonBeautyCustomerDto> GetCustomerAsync(Guid id)
    {
        return await _customerService.GetAsync(id);
    }

    [HttpGet("customers/{id}/bookings")]
    public async Task<List<SalonBeautyCustomerBookingHistoryDto>> GetCustomerBookingHistoryAsync(Guid id, [FromQuery] int maxResultCount = 50)
    {
        return await _customerService.GetBookingHistoryAsync(id, maxResultCount);
    }

    [HttpGet("customers/{id}/loyalty-transactions")]
    public async Task<List<SalonBeautyCustomerLoyaltyTransactionDto>> GetCustomerLoyaltyTransactionsAsync(Guid id, [FromQuery] int maxResultCount = 50)
    {
        return await _customerService.GetLoyaltyTransactionsAsync(id, maxResultCount);
    }

    [HttpPost("customers")]
    [Authorize(MultiTenancyPermissions.SalonBeautyCustomers.Create)]
    public async Task<SalonBeautyCustomerDto> CreateCustomerAsync([FromBody] CreateSalonBeautyCustomerDto input)
    {
        return await _customerService.CreateAsync(input);
    }

    [HttpPut("customers/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyCustomers.Edit)]
    public async Task<SalonBeautyCustomerDto> UpdateCustomerAsync(Guid id, [FromBody] UpdateSalonBeautyCustomerDto input)
    {
        return await _customerService.UpdateAsync(id, input);
    }

    [HttpDelete("customers/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyCustomers.Delete)]
    public async Task DeleteCustomerAsync(Guid id)
    {
        await _customerService.DeleteAsync(id);
    }
    #endregion

    #region Service Categories
    [HttpGet("categories")]
    public async Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetCategoriesAsync([FromQuery] GetSalonBeautyListInput input)
    {
        return await _categoryService.GetListAsync(input);
    }

    [HttpGet("categories/{id}")]
    public async Task<SalonBeautyServiceCategoryDto> GetCategoryAsync(Guid id)
    {
        return await _categoryService.GetAsync(id);
    }

    [HttpPost("categories")]
    [Authorize(MultiTenancyPermissions.SalonBeautyServiceCategories.Create)]
    public async Task<SalonBeautyServiceCategoryDto> CreateCategoryAsync([FromBody] CreateSalonBeautyServiceCategoryDto input)
    {
        return await _categoryService.CreateAsync(input);
    }

    [HttpPut("categories/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyServiceCategories.Edit)]
    public async Task<SalonBeautyServiceCategoryDto> UpdateCategoryAsync(Guid id, [FromBody] UpdateSalonBeautyServiceCategoryDto input)
    {
        return await _categoryService.UpdateAsync(id, input);
    }

    [HttpDelete("categories/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyServiceCategories.Delete)]
    public async Task DeleteCategoryAsync(Guid id)
    {
        await _categoryService.DeleteAsync(id);
    }
    #endregion

    #region Services
    [HttpGet("services")]
    public async Task<PagedResultDto<SalonBeautyServiceDto>> GetServicesAsync([FromQuery] GetSalonBeautyListInput input)
    {
        return await _serviceService.GetListAsync(input);
    }

    [HttpGet("services/{id}")]
    public async Task<SalonBeautyServiceDto> GetServiceAsync(Guid id)
    {
        return await _serviceService.GetAsync(id);
    }

    [HttpPost("services")]
    [Authorize(MultiTenancyPermissions.SalonBeautyServices.Create)]
    public async Task<SalonBeautyServiceDto> CreateServiceAsync([FromBody] CreateSalonBeautyServiceDto input)
    {
        return await _serviceService.CreateAsync(input);
    }

    [HttpPut("services/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyServices.Edit)]
    public async Task<SalonBeautyServiceDto> UpdateServiceAsync(Guid id, [FromBody] UpdateSalonBeautyServiceDto input)
    {
        return await _serviceService.UpdateAsync(id, input);
    }

    [HttpDelete("services/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyServices.Delete)]
    public async Task DeleteServiceAsync(Guid id)
    {
        await _serviceService.DeleteAsync(id);
    }
    #endregion

    #region Stylists
    [HttpGet("stylists")]
    public async Task<PagedResultDto<SalonBeautyStylistDto>> GetStylistsAsync([FromQuery] GetSalonBeautyListInput input)
    {
        return await _stylistService.GetListAsync(input);
    }

    [HttpGet("stylists/{id}")]
    public async Task<SalonBeautyStylistDto> GetStylistAsync(Guid id)
    {
        return await _stylistService.GetAsync(id);
    }

    [HttpPost("stylists")]
    [Authorize(MultiTenancyPermissions.SalonBeautyStylists.Create)]
    public async Task<SalonBeautyStylistDto> CreateStylistAsync([FromBody] CreateSalonBeautyStylistDto input)
    {
        return await _stylistService.CreateAsync(input);
    }

    [HttpPut("stylists/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyStylists.Edit)]
    public async Task<SalonBeautyStylistDto> UpdateStylistAsync(Guid id, [FromBody] UpdateSalonBeautyStylistDto input)
    {
        return await _stylistService.UpdateAsync(id, input);
    }

    [HttpDelete("stylists/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyStylists.Delete)]
    public async Task DeleteStylistAsync(Guid id)
    {
        await _stylistService.DeleteAsync(id);
    }
    #endregion

    #region Bookings
    [HttpGet("bookings")]
    public async Task<PagedResultDto<SalonBeautyBookingListDto>> GetBookingsAsync([FromQuery] GetSalonBeautyBookingListInput input)
    {
        return await _bookingService.GetListAsync(input);
    }

    [HttpGet("bookings/{id}")]
    public async Task<SalonBeautyBookingDetailDto> GetBookingAsync(Guid id)
    {
        return await _bookingService.GetAsync(id);
    }

    [HttpPost("bookings")]
    [Authorize(MultiTenancyPermissions.SalonBeautyBookings.Create)]
    public async Task<SalonBeautyBookingDetailDto> CreateBookingAsync([FromBody] CreateSalonBeautyBookingDto input)
    {
        return await _bookingService.CreateAsync(input);
    }

    [HttpPut("bookings/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyBookings.Edit)]
    public async Task<SalonBeautyBookingDetailDto> UpdateBookingAsync(Guid id, [FromBody] UpdateSalonBeautyBookingDto input)
    {
        return await _bookingService.UpdateAsync(id, input);
    }

    [HttpPost("bookings/{id}/checkin")]
    [Authorize(MultiTenancyPermissions.SalonBeautyBookings.Checkin)]
    public async Task<SalonBeautyBookingDetailDto> CheckinBookingAsync(Guid id)
    {
        return await _bookingService.CheckinAsync(id);
    }

    [HttpPost("bookings/{id}/payment")]
    [Authorize(MultiTenancyPermissions.SalonBeautyBookings.UpdatePayment)]
    public async Task<SalonBeautyBookingDetailDto> UpdatePaymentAsync(Guid id, [FromBody] UpdateBookingPaymentDto input)
    {
        return await _bookingService.UpdatePaymentAsync(id, input);
    }

    [HttpPost("bookings/{id}/cancel")]
    [Authorize(MultiTenancyPermissions.SalonBeautyBookings.Cancel)]
    public async Task<SalonBeautyBookingDetailDto> CancelBookingAsync(Guid id, [FromBody] CancelBookingDto input)
    {
        return await _bookingService.CancelAsync(id, input);
    }

    [HttpDelete("bookings/{id}")]
    [Authorize(MultiTenancyPermissions.SalonBeautyBookings.Delete)]
    public async Task DeleteBookingAsync(Guid id)
    {
        await _bookingService.DeleteAsync(id);
    }
    #endregion

    #region Loyalty
    [HttpGet("loyalty/{customerId}")]
    public async Task<CustomerLoyaltyBalanceDto> GetLoyaltyBalanceAsync(Guid customerId)
    {
        return await _loyaltyService.GetBalanceAsync(customerId);
    }

    [HttpPost("loyalty/{customerId}/add-points")]
    [Authorize]
    public async Task<CustomerLoyaltyBalanceDto> AddPointsAsync(Guid customerId, [FromBody] AddPointsInput input)
    {
        return await _loyaltyService.AddPointsAsync(customerId, input.Points, input.Description);
    }

    [HttpPost("loyalty/{customerId}/deduct-points")]
    [Authorize]
    public async Task<CustomerLoyaltyBalanceDto> DeductPointsAsync(Guid customerId, [FromBody] DeductPointsInput input)
    {
        return await _loyaltyService.DeductPointsAsync(customerId, input.Points, input.Description);
    }
    #endregion
}

public class AddPointsInput
{
    public int Points { get; set; }
    public string Description { get; set; } = null!;
}

public class DeductPointsInput
{
    public int Points { get; set; }
    public string Description { get; set; } = null!;
}
