using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
    private readonly IZaloApiClient _zaloApiClient;

    public SalonBeautyMiniAppController(
        IMiniAppSalonBeautyCustomerAppService customerService,
        IMiniAppSalonBeautyLoyaltyAppService loyaltyService,
        IMiniAppSalonBeautyServiceCategoryAppService serviceCategoryService,
        IMiniAppSalonBeautyServiceAppService serviceService,
        IMiniAppSalonBeautyStylistAppService stylistService,
        IMiniAppSalonBeautyBookingAppService bookingService,
        IZaloApiClient zaloApiClient)
    {
        _customerService = customerService;
        _loyaltyService = loyaltyService;
        _serviceCategoryService = serviceCategoryService;
        _serviceService = serviceService;
        _stylistService = stylistService;
        _bookingService = bookingService;
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
    public IActionResult GetBusinessEstablishments()
    {
        var result = new[]
        {
        new
        {
            Id = "AMI01",
            Name = "Ami Hair Salon",
            Address = "PDKT - 07 Vinhome Ocean Park 2, Xã Nghĩa Trụ, Tỉnh Hưng Yên",
            Phone = "0389466633",
            Image = "https://scontent.fhan14-2.fna.fbcdn.net/v/t39.30808-6/686370972_122219740148287170_3705809270086126578_n.jpg?_nc_cat=108&ccb=1-7&_nc_sid=cc71e4&_nc_ohc=g0D8iIP7SpQQ7kNvwF7qLxH&_nc_oc=AdrPjI7cdTNqYapeYnYhyBvpwGby8qivE6C-8kYbxoqXt88VrjjHXKFlQIWbDb0FiJ8&_nc_zt=23&_nc_ht=scontent.fhan14-2.fna&_nc_gid=iNojTzIGd6Rc95hGwl916w&_nc_ss=7a2a8&oh=00_Af4MitegUzl6KrpylwTtFlBfLCeLWbYDUp9nIt2s_ZA3Rg&oe=6A0DF06E"
        }
    };

        return Ok(result);
    }

    [HttpGet("tee-times")]
    [AllowAnonymous]
    public IActionResult GetTeeTimes([FromQuery] string businessEstablishmentCode, [FromQuery] DateTime date)
    {
        if (string.IsNullOrWhiteSpace(businessEstablishmentCode))
        {
            return BadRequest("Missing businessEstablishmentCode");
        }

        if (date == default)
        {
            return BadRequest("Missing date");
        }

        var result = new
        {
            BusinessEstablishmentCode = businessEstablishmentCode,
            Date = date.ToString("yyyy-MM-dd"),
            Items = new[]
            {
            new { Id = "9412BDA1-CFE4-14E1-B488-3A205EAF2CD7", Time = "8:00", Status = "available" },
            new { Id = "5484BE01-097E-7651-004D-3A205EAF2CDA", Time = "9:00", Status = "peak" },
            new { Id = "CB9B70A2-76EC-A0EB-0130-3A205EAF2CDA", Time = "10:00", Status = "full" },
            new { Id = "22B3A652-EDFC-3767-0164-3A205EAF2CDA", Time = "11:00", Status = "full" },
            new { Id = "CF8D6ED3-9B1F-5B37-04EB-3A205EAF2CDA", Time = "12:00", Status = "peak" },
            new { Id = "D6D05B0F-2B78-2D93-064A-3A205EAF2CDA", Time = "13:00", Status = "peak" },
            new { Id = "FDB4E2DE-8BF5-CB65-09C6-3A205EAF2CDA", Time = "14:00", Status = "peak" },
            new { Id = "78C44B00-BB4E-9AA1-0CF4-3A205EAF2CDA", Time = "15:00", Status = "peak" },
            new { Id = "DC120563-CE5D-A8E6-0DC6-3A205EAF2CDA", Time = "16:00", Status = "available" },
            new { Id = "58A54E5A-3A9B-9C31-0E99-3A205EAF2CDA", Time = "17:00", Status = "available" },
            new { Id = "472F4BDD-E7A7-C71B-10B2-3A205EAF2CDA", Time = "18:00", Status = "available" },
            new { Id = "0E1D35C0-AFFC-0AE7-113F-3A205EAF2CDA", Time = "19:00", Status = "full" },
            new { Id = "83BFD823-4413-A7EC-1431-3A205EAF2CDA", Time = "20:00", Status = "peak" }
        }
        };

        return Ok(result);
    }
}
