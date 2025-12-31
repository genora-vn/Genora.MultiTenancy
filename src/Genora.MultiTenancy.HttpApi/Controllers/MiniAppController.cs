using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
    private readonly IStringLocalizer<MultiTenancyResource> _localizer;
    private readonly IOptionExtendService _optionExtendService;
    public MiniAppController(IZaloApiClient zaloApiClient,
                             IMiniAppBookingAppService miniBooking,
                             IMiniAppSettingService miniAppSetting,
                             IMiniAppCustomerTypeService miniAppCustomerType,
                             IMiniAppGolfCourseService miniAppGolfCourse,
                             IMiniAppMembershipTierService miniAppMembershipTier,
                             IMiniAppNewsService miniAppNews,
                             IMiniAppCalendarSlotService miniAppCalendarSlot,
                             IStringLocalizer<MultiTenancyResource> localizer,
                             IMiniAppCustomerAppService miniCustomer,
                             IOptionExtendService optionExtendService)
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
        _localizer = localizer;
        _optionExtendService = optionExtendService;
    }

    [HttpPost("create-booking")]
    [AllowAnonymous] // tạm: sẽ thay bằng custom auth sau
    public Task<MiniAppBookingDetailDto> CreateBookingAsync([FromBody] MiniAppCreateBookingDto input)
        => _miniBooking.CreateFromMiniAppAsync(input);

    [HttpGet("get-bookings")]
    [AllowAnonymous]
    public Task<MiniAppBookingListDto> GetBookingsAsync([FromQuery] GetMiniAppBookingListInput input)
        => _miniBooking.GetListMiniAppAsync(input);

    [HttpGet("get-bookings/{id}")]
    [AllowAnonymous]
    public Task<MiniAppBookingDetailDto> GetBookingAsync(Guid id, [FromQuery] Guid customerId)
        => _miniBooking.GetMiniAppAsync(id, customerId);
    //[HttpGet("get-booking-histories")]
    //[AllowAnonymous]
    //public Task<MiniAppBookingListDto> GetBookingHistoties([FromQuery] GetMiniAppBookingListInput input)
    //    => _miniBooking.GetBookingHistoryAsync(input);
    [HttpGet("get-app-settings")]
    [AllowAnonymous]
    public Task<MiniAppAppSettingListDto> GetAppSettingsAsync([FromQuery] GetMiniAppSettingListInput input)
        => _miniAppSetting.GetListAsync(input);

    [HttpGet("get-app-settings/{id}")]
    [AllowAnonymous]
    public Task<MiniAppAppSettingDetailDto> GetAppSettingAsync(Guid id)
        => _miniAppSetting.GetAsync(id);

    [HttpGet("get-customer-types")]
    [AllowAnonymous]
    public Task<PagedResultDto<AppCustomerTypeDto>> GetCustomerTypesAsync([FromQuery] PagedAndSortedResultRequestDto input)
        => _miniAppCustomerType.GetListAsync(input);

    [HttpGet("get-golf-courses")]
    [AllowAnonymous]
    public Task<MiniAppGolfCourseListDto> GetGolfCoursesAsync([FromQuery] GetMiniAppGolfCourseListInput input)
        => _miniAppGolfCourse.GetListAsync(input);
    [HttpGet("get-golf-courses/{id}")]
    [AllowAnonymous]
    public Task<MiniAppGolfCourseDetailDto> GetGolfCourseAsync(Guid id)
        => _miniAppGolfCourse.GetAsync(id);
    [HttpGet("get-membership-tiers")]
    [AllowAnonymous]
    public Task<MiniAppMembershipTierListDto> GetMembershipTiersAsync([FromQuery] PagedAndSortedResultRequestDto input)
        => _miniAppMembershipTier.GetListAsync(input);

    [HttpGet("get-news")]
    [AllowAnonymous]
    public Task<MiniAppNewsListDto> GetNewsAsync([FromQuery] GetMiniAppNewsDto input)
        => _miniAppNews.GetListAsync(input);

    [HttpGet("get-news/{id}")]
    [AllowAnonymous]
    public Task<MiniAppNewsDetailDto> GetNewsAsync(Guid id)
        => _miniAppNews.GetAsync(id);

    [HttpGet("get-calendar-slots")]
    [AllowAnonymous]
    public async Task<MiniAppCalendarSlotDto> GetCalendarSlotsAsync([FromQuery] GetMiniAppCalendarListInput input)
    {
        var result = await _miniAppCalendarSlot.GetListMiniAppAsync(input);
        if (result.FrameTimeOfDays != null)
        {
            foreach(var item in result.FrameTimeOfDays)
            {
                item.Name = _localizer[item.Name];
            }
        }
        return result;
    }
        
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

        return Ok(result);
    }

    [HttpPost("customer/upsert")]
    [AllowAnonymous]
    public async Task<IActionResult> UpsertCustomer([FromBody] MiniAppUpsertCustomerRequest input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.PhoneNumber) || string.IsNullOrWhiteSpace(input.FullName))
            return BadRequest("Missing PhoneNumber or FullName");

        var result = await _miniCustomer.UpsertFromMiniAppAsync(input, ct);

        return Ok(result);
    }

    [HttpGet("customer/by-phone")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPhone([FromQuery] string phoneNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return BadRequest("Missing accessToken");

        var result = await _miniCustomer.GetByPhoneAsync(phoneNumber, ct);

        return Ok(result);
    }
    [HttpGet("get-ulitities")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUlitities()
    {
        var result = await _optionExtendService.GetUtilitiesAsync();
        return Ok(result);
    }
}