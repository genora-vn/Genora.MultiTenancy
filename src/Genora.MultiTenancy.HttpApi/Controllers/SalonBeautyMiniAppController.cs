using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Controllers;

[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Area("MultiTenancy")]
[Route("api/mini-app/salon-beauty")]
public class SalonBeautyMiniAppController : MultiTenancyController
{
    private readonly IMiniAppSalonBeautyCustomerAppService _customerService;
    private readonly IMiniAppSalonBeautyLoyaltyAppService _loyaltyService;
    private readonly IMiniAppSalonBeautyServiceCategoryAppService _serviceCategoryService;
    private readonly IMiniAppSalonBeautyServiceAppService _serviceService;
    private readonly IMiniAppSalonBeautyStylistAppService _stylistService;
    private readonly IMiniAppSalonBeautyBookingAppService _bookingService;
    private readonly IMiniAppSalonBeautyLocationAppService _locationService;
    private readonly IMiniAppSalonBeautyTimeSlotAppService _timeSlotService;
    private readonly IZaloApiClient _zaloApiClient;

    public SalonBeautyMiniAppController(
        IMiniAppSalonBeautyCustomerAppService customerService,
        IMiniAppSalonBeautyLoyaltyAppService loyaltyService,
        IMiniAppSalonBeautyServiceCategoryAppService serviceCategoryService,
        IMiniAppSalonBeautyServiceAppService serviceService,
        IMiniAppSalonBeautyStylistAppService stylistService,
        IMiniAppSalonBeautyBookingAppService bookingService,
        IMiniAppSalonBeautyLocationAppService locationService,
        IMiniAppSalonBeautyTimeSlotAppService timeSlotService,
        IZaloApiClient zaloApiClient)
    {
        _customerService = customerService;
        _loyaltyService = loyaltyService;
        _serviceCategoryService = serviceCategoryService;
        _serviceService = serviceService;
        _stylistService = stylistService;
        _bookingService = bookingService;
        _locationService = locationService;
        _timeSlotService = timeSlotService;
        _zaloApiClient = zaloApiClient;
    }

    /// <summary>
    /// Giải mã số điện thoại từ code
    /// </summary>
    [HttpPost("decode-phone")]
    [AllowAnonymous]
    public async Task<IActionResult> DecodePhone([FromBody] ZaloDecodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Missing code or accessToken");

        var result = await _zaloApiClient.DecodePhoneAsync(request.Code, request.AccessToken, ct);

        return Ok(result);
    }

    [HttpGet("customers")]
    [AllowAnonymous]
    public Task<PagedResultDto<SalonBeautyCustomerDto>> GetCustomersAsync([FromQuery] GetSalonBeautyListInput input)
        => _customerService.GetListMiniAppAsync(input);

    [HttpGet("customers/{id}")]
    [AllowAnonymous]
    public Task<SalonBeautyCustomerDto> GetCustomerAsync(Guid id)
        => _customerService.GetMiniAppAsync(id);

    [HttpGet("customer/by-phone")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCustomerByPhoneAsync([FromQuery] string phoneNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return BadRequest("Missing phoneNumber");
        }

        var result = await _customerService.GetByPhoneAsync(phoneNumber, ct);

        return Ok(result);
    }

    [HttpPost("customer/upsert")]
    [AllowAnonymous]
    public async Task<IActionResult> UpsertCustomerAsync([FromBody] MiniAppSalonBeautyUpsertCustomerRequest input, CancellationToken ct)
    {
        var result = await _customerService.UpsertFromMiniAppAsync(input, ct);

        return Ok(result);
    }

    [HttpGet("customers/{customerId}/loyalty")]
    [AllowAnonymous]
    public Task<CustomerLoyaltyBalanceDto> GetCustomerLoyaltyAsync(Guid customerId)
        => _loyaltyService.GetBalanceMiniAppAsync(customerId);

    [HttpGet("service-categories")]
    [AllowAnonymous]
    public Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetServiceCategoriesAsync([FromQuery] GetSalonBeautyListInput input)
        => _serviceCategoryService.GetListMiniAppAsync(input);

    [HttpGet("service-categories/{id}")]
    [AllowAnonymous]
    public Task<SalonBeautyServiceCategoryDto> GetServiceCategoryAsync(Guid id)
        => _serviceCategoryService.GetMiniAppAsync(id);

    [HttpGet("services")]
    [AllowAnonymous]
    public Task<PagedResultDto<SalonBeautyServiceDto>> GetServicesAsync([FromQuery] GetSalonBeautyListInput input)
        => _serviceService.GetListMiniAppAsync(input);

    [HttpGet("services/{id}")]
    [AllowAnonymous]
    public Task<SalonBeautyServiceDto> GetServiceAsync(Guid id)
        => _serviceService.GetMiniAppAsync(id);

    [HttpGet("stylists")]
    [AllowAnonymous]
    public Task<PagedResultDto<SalonBeautyStylistDto>> GetStylistsAsync([FromQuery] GetSalonBeautyListInput input)
        => _stylistService.GetListMiniAppAsync(input);

    [HttpGet("stylists/{id}")]
    [AllowAnonymous]
    public Task<SalonBeautyStylistDto> GetStylistAsync(Guid id)
        => _stylistService.GetMiniAppAsync(id);

    [HttpGet("bookings")]
    [AllowAnonymous]
    public Task<PagedResultDto<SalonBeautyBookingDetailDto>> GetBookingsAsync([FromQuery] GetSalonBeautyBookingListInput input)
        => _bookingService.GetListMiniAppAsync(input);

    [HttpGet("bookings/{id}")]
    [AllowAnonymous]
    public Task<SalonBeautyBookingDetailDto> GetBookingAsync(Guid id)
        => _bookingService.GetMiniAppAsync(id);

    [HttpPost("bookings")]
    [AllowAnonymous]
    public Task<SalonBeautyBookingDetailDto> CreateBookingAsync([FromBody] CreateSalonBeautyBookingDto input)
        => _bookingService.CreateMiniAppAsync(input);

    [HttpPost("bookings/{id}/cancel")]
    [AllowAnonymous]
    public Task<SalonBeautyBookingDetailDto> CancelBookingAsync(Guid id, [FromBody] CancelBookingDto input)
        => _bookingService.CancelMiniAppAsync(id, input);

    [HttpGet("business-establishments")]
    [AllowAnonymous]
    public Task<List<MiniAppSalonBeautyLocationDto>> GetBusinessEstablishments([FromQuery] GetMiniAppLocationListInput input)
        => _locationService.GetListAsyncLocations(input);

    [HttpGet("tee-times")]
    [AllowAnonymous]
    public Task<List<MiniAppSalonBeautyTimeSlotDto>> GetTeeTimes([FromQuery] GetMiniAppTimeSlotListInput input)
        => _timeSlotService.GetListAsyncTimeSlots(input);
}
