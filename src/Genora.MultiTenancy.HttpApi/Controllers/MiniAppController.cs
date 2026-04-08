using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.AppDtos.AppProCategories;
using Genora.MultiTenancy.AppDtos.AppProItems;
using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.AppDtos.AppPayments;
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
    private readonly IMiniAppHomePageConfigService _miniHomePage;
    private readonly IMiniAppFnbCategoryService   _miniAppFnbCategory;
    private readonly IMiniAppFnbItemService       _miniAppFnbItem;
    private readonly IMiniAppFnbOrderService      _miniAppFnbOrder;
    private readonly IMiniAppPaymentAppService    _miniPayment;
    private readonly IMiniAppFnbPaymentAppService _miniFnbPayment;
    private readonly IMiniAppProCategoryService   _miniProCategory;
    private readonly IMiniAppProItemService       _miniProItem;
    private readonly IMiniAppProOrderService      _miniProOrder;
    private readonly IMiniAppProPaymentAppService _miniProPayment;

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
                             IOptionExtendService optionExtendService,
                             IMiniAppHomePageConfigService miniHomePage,
                             IMiniAppFnbCategoryService miniAppFnbCategory,
                             IMiniAppFnbItemService miniAppFnbItem,
                             IMiniAppFnbOrderService miniAppFnbOrder,
                             IMiniAppPaymentAppService miniPayment,
                             IMiniAppFnbPaymentAppService miniFnbPayment,
                             IMiniAppProCategoryService miniProCategory,
                             IMiniAppProItemService miniProItem,
                             IMiniAppProOrderService miniProOrder,
                             IMiniAppProPaymentAppService miniProPayment)
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
        _miniHomePage = miniHomePage;
        _miniAppFnbCategory = miniAppFnbCategory;
        _miniAppFnbItem = miniAppFnbItem;
        _miniAppFnbOrder = miniAppFnbOrder;
        _miniPayment = miniPayment;
        _miniFnbPayment = miniFnbPayment;
        _miniProCategory = miniProCategory;
        _miniProItem = miniProItem;
        _miniProOrder = miniProOrder;
        _miniProPayment = miniProPayment;
    }

    [HttpPost("create-booking")]
    [AllowAnonymous]
    public Task<MiniAppBookingDetailDto> CreateBookingAsync([FromBody] MiniAppCreateBookingDto input)
        => _miniBooking.CreateFromMiniAppAsync(input);

    [HttpPut("update-bookings/{id}")]
    [AllowAnonymous]
    public Task<MiniAppBookingDetailDto> UpdateBookingAsync(Guid id, [FromBody] MiniAppUpdateBookingDto input)
    => _miniBooking.UpdateFromMiniAppAsync(id, input);

    [HttpGet("get-bookings")]
    [AllowAnonymous]
    public Task<MiniAppBookingListDto> GetBookingsAsync([FromQuery] GetMiniAppBookingListInput input)
        => _miniBooking.GetListMiniAppAsync(input);

    [HttpGet("get-bookings/{id}")]
    [AllowAnonymous]
    public Task<MiniAppBookingDetailDto> GetBookingAsync(Guid id, [FromQuery] Guid customerId)
        => _miniBooking.GetMiniAppAsync(id, customerId);

    /// <summary>
    /// Huỷ booking từ Mini App.
    /// Chỉ chủ booking (customerId khớp) mới được huỷ.
    /// Status → CancelledRefund. Tự động gửi ZBS + Email cancel.
    /// </summary>
    [HttpPost("cancel-booking/{id}")]
    [AllowAnonymous]
    public Task<MiniAppBookingDetailDto> CancelBookingAsync(Guid id, [FromBody] MiniAppCancelBookingDto input)
        => _miniBooking.CancelFromMiniAppAsync(id, input);
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
    public async Task<IActionResult> DecodePhone([FromBody] ZaloDecodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Missing code or accessToken");

        var result = await _zaloApiClient.DecodePhoneAsync(request.Code, request.AccessToken, ct);

        return Ok(result);
    }

    /// <summary>
    /// Giải mã vị trí (lat/lon) từ token getLocation()
    /// </summary>
    [HttpPost("decode-location")]
    [AllowAnonymous]
    public async Task<IActionResult> DecodeLocation([FromBody] ZaloDecodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Missing code or accessToken");

        var result = await _zaloApiClient.DecodeLocationAsync(request.Code, request.AccessToken, ct);

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

    [HttpGet("get-home-page-widgets")]
    [AllowAnonymous]
    public Task<MiniAppHomePageConfigDto> GetHomePageWidgetsAsync()
    => _miniHomePage.GetHomePageConfigAsync();

    [HttpGet("get-home-page-feature-grid")]
    [AllowAnonymous]
    public Task<FeatureGridDto> GetHomePageFeatureGridAsync([FromQuery] Guid widgetId)
        => _miniHomePage.GetFeatureGridAsync(widgetId);

    // Fnb
    [HttpGet("get-fnb-categories")]
    [AllowAnonymous]
    public Task<MiniAppFnbCategoryListDto> GetFnbCategoriesAsync()
    => _miniAppFnbCategory.GetListAsync();

    [HttpGet("get-fnb-items")]
    [AllowAnonymous]
    public Task<MiniAppFnbItemListDto> GetFnbItemsAsync([FromQuery] GetMiniAppFnbItemListInput input)
        => _miniAppFnbItem.GetListAsync(input);

    [HttpGet("get-fnb-items/{id}")]
    [AllowAnonymous]
    public Task<MiniAppFnbItemDetailDto> GetFnbItemAsync(Guid id)
        => _miniAppFnbItem.GetAsync(id);

    [HttpPost("create-fnb-order")]
    [AllowAnonymous]
    public Task<MiniAppFnbOrderDetailDto> CreateFnbOrderAsync([FromBody] CreateFnbOrderDto input)
        => _miniAppFnbOrder.CreateAsync(input);

    [HttpGet("get-fnb-orders")]
    [AllowAnonymous]
    public Task<MiniAppFnbOrderListDto> GetFnbOrdersAsync([FromQuery] GetMiniAppFnbOrderListInput input)
        => _miniAppFnbOrder.GetListAsync(input);

    [HttpGet("get-fnb-orders/{id}")]
    [AllowAnonymous]
    public Task<MiniAppFnbOrderDetailDto> GetFnbOrderAsync(Guid id)
        => _miniAppFnbOrder.GetAsync(id);

    [HttpPost("cancel-fnb-orders/{id}")]
    [AllowAnonymous]
    public Task<MiniAppFnbOrderDetailDto> CancelFnbOrderAsync(Guid id, [FromBody] CancelMiniAppFnbOrderDto input)
    => _miniAppFnbOrder.CancelAsync(id, input);

    // ── Payment — Booking (Đặt sân) ──────────────────────────────────────────

    /// <summary>
    /// Tạo dữ liệu order đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder().
    /// POST /api/mini-app/payment/prepare-order
    /// </summary>
    [HttpPost("payment/prepare-order")]
    [AllowAnonymous]
    public Task<PrepareOrderResult> PrepareOrderAsync([FromBody] PrepareOrderInput input)
        => _miniPayment.PrepareOrderAsync(input);

    /// <summary>
    /// Kiểm tra trạng thái giao dịch sau khi Mini App gọi createOrder() xong.
    /// GET /api/mini-app/payment/check-transaction/{orderId}
    /// </summary>
    [HttpGet("payment/check-transaction/{orderId}")]
    [AllowAnonymous]
    public Task<CheckTransactionResult> CheckTransactionAsync(string orderId)
        => _miniPayment.CheckTransactionAsync(orderId);

    // ── Payment — FnB (Đặt món) ───────────────────────────────────────────────

    /// <summary>
    /// Tạo dữ liệu order đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder() cho đơn FnB.
    /// orderId format: {FnbOrderCode}_{timestamp} — prefix "FNB" để phân biệt với Booking.
    /// POST /api/mini-app/payment/fnb/prepare-order
    /// </summary>
    [HttpPost("payment/fnb/prepare-order")]
    [AllowAnonymous]
    public Task<PrepareOrderResult> PrepareFnbOrderAsync([FromBody] PrepareFnbOrderInput input)
        => _miniFnbPayment.PrepareOrderAsync(input);

    /// <summary>
    /// Kiểm tra trạng thái giao dịch FnbOrder sau khi Mini App gọi createOrder() xong.
    /// GET /api/mini-app/payment/fnb/check-transaction/{orderId}
    /// </summary>
    [HttpGet("payment/fnb/check-transaction/{orderId}")]
    [AllowAnonymous]
    public Task<CheckTransactionResult> CheckFnbTransactionAsync(string orderId)
        => _miniFnbPayment.CheckTransactionAsync(orderId);

    // ── Proshop — Danh mục sản phẩm ─────────────────────────────────────────

    [HttpGet("get-pro-categories")]
    [AllowAnonymous]
    public Task<MiniAppProCategoryListDto> GetProCategoriesAsync()
        => _miniProCategory.GetListAsync();

    // ── Proshop — Sản phẩm ───────────────────────────────────────────────────

    [HttpGet("get-pro-items")]
    [AllowAnonymous]
    public Task<MiniAppProItemListDto> GetProItemsAsync([FromQuery] GetMiniAppProItemListInput input)
        => _miniProItem.GetListAsync(input);

    [HttpGet("get-pro-items/{id}")]
    [AllowAnonymous]
    public Task<MiniAppProItemDetailDto> GetProItemAsync(Guid id)
        => _miniProItem.GetAsync(id);

    // ── Proshop — Đơn hàng ───────────────────────────────────────────────────

    [HttpPost("create-pro-order")]
    [AllowAnonymous]
    public Task<MiniAppProOrderDetailDto> CreateProOrderAsync([FromBody] CreateProOrderDto input)
        => _miniProOrder.CreateAsync(input);

    [HttpGet("get-pro-orders")]
    [AllowAnonymous]
    public Task<MiniAppProOrderListDto> GetProOrdersAsync([FromQuery] GetMiniAppProOrderListInput input)
        => _miniProOrder.GetListAsync(input);

    [HttpGet("get-pro-orders/{id}")]
    [AllowAnonymous]
    public Task<MiniAppProOrderDetailDto> GetProOrderAsync(Guid id)
        => _miniProOrder.GetAsync(id);

    [HttpPost("cancel-pro-orders/{id}")]
    [AllowAnonymous]
    public Task<MiniAppProOrderDetailDto> CancelProOrderAsync(Guid id, [FromBody] CancelMiniAppProOrderDto input)
        => _miniProOrder.CancelAsync(id, input);

    // ── Proshop — Thanh toán ─────────────────────────────────────────────────

    /// <summary>
    /// Tạo dữ liệu order đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder() cho đơn Proshop.
    /// orderId format: {ProOrderCode}_{timestamp} — prefix "PRO" để phân biệt với Booking và FnB.
    /// POST /api/mini-app/payment/pro/prepare-order
    /// </summary>
    [HttpPost("payment/pro/prepare-order")]
    [AllowAnonymous]
    public Task<PrepareOrderResult> PrepareProOrderAsync([FromBody] PrepareProOrderInput input)
        => _miniProPayment.PrepareOrderAsync(input);

    /// <summary>
    /// Kiểm tra trạng thái giao dịch ProOrder sau khi Mini App gọi createOrder() xong.
    /// GET /api/mini-app/payment/pro/check-transaction/{orderId}
    /// </summary>
    [HttpGet("payment/pro/check-transaction/{orderId}")]
    [AllowAnonymous]
    public Task<CheckTransactionResult> CheckProTransactionAsync(string orderId)
        => _miniProPayment.CheckTransactionAsync(orderId);
}