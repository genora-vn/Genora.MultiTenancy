using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
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
[Authorize(AuthenticationSchemes = "MiniAppJwt", Policy = "MiniAppOnly")]
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
    public MiniAppController(IZaloApiClient zaloApiClient,
                             IMiniAppBookingAppService miniBooking,
                             IMiniAppSettingService miniAppSetting,
                             IMiniAppCustomerTypeService miniAppCustomerType,
                             IMiniAppGolfCourseService miniAppGolfCourse,
                             IMiniAppMembershipTierService miniAppMembershipTier,
                             IMiniAppNewsService miniAppNews,
                             IMiniAppCalendarSlotService miniAppCalendarSlot)
    {
        _zaloApiClient = zaloApiClient;
        _miniBooking = miniBooking;
        _miniAppSetting = miniAppSetting;
        _miniAppCustomerType = miniAppCustomerType;
        _miniAppGolfCourse = miniAppGolfCourse;
        _miniAppMembershipTier = miniAppMembershipTier;
        _miniAppNews = miniAppNews;
        _miniAppCalendarSlot = miniAppCalendarSlot;
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

    [HttpPost("decode-phone")]
    [AllowAnonymous]
    public async Task<IActionResult> DecodePhone([FromBody] DecodePhoneRequest body, CancellationToken ct)
    {
        var zaloPhoneResponse = new ZaloPhoneResponse();

        if (body == null || string.IsNullOrWhiteSpace(body.Code) || string.IsNullOrWhiteSpace(body.AccessToken))
        {
            zaloPhoneResponse.Error = 400;
            zaloPhoneResponse.Message = "Missing required parameter.";
            return BadRequest(zaloPhoneResponse);
        }

        var resp = await _zaloApiClient.DecodePhoneAsync(body.Code, body.AccessToken, ct);

        if (resp == null || resp.Error != 0 || resp.Data == null)
        {
            zaloPhoneResponse.Error = resp?.Error ?? -1;
            zaloPhoneResponse.Message = resp?.Message ?? "DecodePhone failed.";
            return BadRequest(zaloPhoneResponse);
        }

        return Ok(resp);
    }
}