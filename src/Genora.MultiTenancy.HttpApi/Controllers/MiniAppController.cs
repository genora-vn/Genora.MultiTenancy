using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
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
[Route("api/mini-app")]
public class MiniAppController : MultiTenancyController
{
    private readonly IZaloApiClient _zaloApiClient;
    private readonly IMiniAppBookingAppService _miniBooking;
    private readonly IMiniAppSettingService _miniAppSetting;
    private readonly IMiniAppCustomerTypeService _miniAppCustomerType;
    private readonly IMiniAppGolfCourseService _miniAppGolfCourse;
    private readonly IMiniAppMembershipTierService _miniAppMembershipTier;
    private readonly IMiniAppNewsService _miniAppNews;
    private readonly IMiniAppCalendarSlotService _miniAppCalendarSlot;
    private readonly IMiniAppCustomerAppService _miniCustomer;

    public MiniAppController(IZaloApiClient zaloApiClient,
                             IMiniAppBookingAppService miniBooking,
                             IMiniAppSettingService miniAppSetting,
                             IMiniAppCustomerTypeService miniAppCustomerType,
                             IMiniAppGolfCourseService miniAppGolfCourse,
                             IMiniAppMembershipTierService miniAppMembershipTier,
                             IMiniAppNewsService miniAppNews,
                             IMiniAppCalendarSlotService miniAppCalendarSlot,
                             IMiniAppCustomerAppService miniCustomer)
    {
        _zaloApiClient = zaloApiClient;
        _miniBooking = miniBooking;
        _miniAppSetting = miniAppSetting;
        _miniAppCustomerType = miniAppCustomerType;
        _miniAppGolfCourse = miniAppGolfCourse;
        _miniAppMembershipTier = miniAppMembershipTier;
        _miniAppNews = miniAppNews;
        _miniAppCalendarSlot = miniAppCalendarSlot;
        _miniCustomer = miniCustomer;
    }

    [HttpPost("create-booking")]
    [AllowAnonymous] // tạm: sẽ thay bằng custom auth sau
    public Task<AppBookingDto> CreateBookingAsync([FromBody] MiniAppCreateBookingDto input)
        => _miniBooking.CreateFromMiniAppAsync(input);

    [HttpGet("get-bookings")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppBookingDto>> GetBookingsAsync([FromQuery] GetMiniAppBookingListInput input)
        => _miniBooking.GetListMiniAppAsync(input);

    [HttpGet("get-bookings/{id}")]
    [AllowAnonymous]
    public Task<AppBookingDto> GetBookingAsync(Guid id, [FromQuery] Guid customerId)
        => _miniBooking.GetMiniAppAsync(id, customerId);

    [HttpGet("app-settings")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppSettingDto>> GetAppSettingsAsync([FromQuery] GetMiniAppSettingListInput input)
        => _miniAppSetting.GetListAsync(input);

    [HttpGet("app-settings/{id}")]
    [AllowAnonymous]
    public Task<AppSettingDto> GetAppSettingAsync(Guid id)
        => _miniAppSetting.GetAsync(id);

    [HttpGet("customer-types")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppCustomerTypeDto>> GetCustomerTypesAsync([FromQuery] PagedAndSortedResultRequestDto input)
        => _miniAppCustomerType.GetListAsync(input);

    [HttpGet("get-golf-courses")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppGolfCourseDto>> GetGolfCoursesAsync([FromQuery] GetMiniAppGolfCourseListInput input)
        => _miniAppGolfCourse.GetListAsync(input);
    [HttpGet("get-golf-courses/{id}")]
    [AllowAnonymous]
    public Task<AppGolfCourseDto> GetGolfCourseAsync(Guid id)
        => _miniAppGolfCourse.GetAsync(id);
    [HttpGet("membership-tiers")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppMembershipTierDto>> GetMembershipTiersAsync([FromQuery] PagedAndSortedResultRequestDto input)
        => _miniAppMembershipTier.GetListAsync(input);

    [HttpGet("get-news")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppNewsDto>> GetNewsAsync([FromQuery] GetNewsListInput input)
        => _miniAppNews.GetListAsync(input);

    [HttpGet("get-news/{id}")]
    [AllowAnonymous]
    public Task<AppNewsDto> GetNewsAsync(Guid id)
        => _miniAppNews.GetAsync(id);

    [HttpGet("get-calendar-slots")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppCalendarSlotDto>> GetCalendarSlotsAsync([FromQuery] GetCalendarSlotListInput input)
        => _miniAppCalendarSlot.GetListMiniAppAsync(input);
    [HttpGet("get-calendar-slots/{id}")]
    [AllowAnonymous]
    public Task<AppCalendarSlotDto> GetCalendarSlotAsync(Guid id)
        => _miniAppCalendarSlot.GetMiniAppAsync(id);

    // <summary>
    /// Lấy thông tin user từ Zalo Graph API
    /// </summary>
    [HttpGet("get-zalo-me")]
    [AllowAnonymous]
    public async Task<IActionResult> GetZaloMe([FromQuery] string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return BadRequest("Missing accessToken");

        var result = await _zaloApiClient.GetZaloMeAsync(accessToken, ct);

        if (result.Error != 0)
            return StatusCode(400, result);

        return Ok(result);
    }

    /// <summary>
    /// Giải mã số điện thoại từ code
    /// </summary>
    [HttpPost("decode-phone")]
    [AllowAnonymous]
    public async Task<IActionResult> DecodePhone([FromBody] ZaloDecodePhoneRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Missing code or accessToken");

        var result = await _zaloApiClient.DecodePhoneAsync(request.Code, request.AccessToken, ct);

        if (result.Error != 0)
            return StatusCode(400, result);

        return Ok(result);
    }

    [HttpPost("customer/upsert")]
    [AllowAnonymous]
    public async Task<IActionResult> UpsertCustomer([FromBody] MiniAppUpsertCustomerRequest input)
        => Ok(await _miniCustomer.UpsertFromMiniAppAsync(input));

    [HttpGet("customer/by-phone")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPhone([FromQuery] string phoneNumber)
        => Ok(await _miniCustomer.GetByPhoneAsync(phoneNumber));
}