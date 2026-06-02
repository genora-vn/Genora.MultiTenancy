using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace Genora.MultiTenancy.Controllers;

[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Area("MultiTenancy")]
[Route("api/mini-app/caddie")]
public class CaddieMiniAppController : MultiTenancyController
{
    private readonly MiniAppCaddieAppService _caddieService;
    private readonly IZaloApiClient _zaloApiClient;

    public CaddieMiniAppController(
        MiniAppCaddieAppService caddieService,
        IZaloApiClient zaloApiClient)
    {
        _caddieService = caddieService;
        _zaloApiClient = zaloApiClient;
    }

    /// <summary>
    /// Giải mã số điện thoại từ code
    /// POST /api/mini-app/caddie/decode-phone
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
    /// GET danh sách caddie available theo ngày + giờ
    /// GET /api/mini-app/caddie/available?bookingDate=2026-06-10&startTime=08:00
    /// </summary>
    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableCaddies([FromQuery] DateTime bookingDate, [FromQuery] TimeSpan? startTime)
    {
        var result = await _caddieService.GetAvailableCaddiesAsync(bookingDate, startTime);
        return Ok(result);
    }

    /// <summary>
    /// GET chi tiết caddie + recent reviews
    /// GET /api/mini-app/caddie/{id}
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCaddieDetail(Guid id)
    {
        var result = await _caddieService.GetCaddieDetailAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// POST đặt caddie
    /// POST /api/mini-app/caddie/booking
    /// Body: { caddieId, bookingDate, startTime, numberOfHoles, note }
    /// Header: X-Customer-Id, X-Customer-Name, X-Customer-Phone (từ Mini App user context)
    /// </summary>
    [HttpPost("booking")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateBooking([FromBody] MiniAppCreateCaddieBookingDto input, CancellationToken ct)
    {
        // Extract customer info from headers (Mini App middleware hoặc từ Zalo user context)
        var customerIdHeader = Request.Headers["X-Customer-Id"].ToString();
        var customerName = Request.Headers["X-Customer-Name"].ToString();
        var phone = Request.Headers["X-Customer-Phone"].ToString();

        if (string.IsNullOrWhiteSpace(customerIdHeader) || !Guid.TryParse(customerIdHeader, out var customerId))
            return BadRequest("Missing or invalid X-Customer-Id header");

        if (string.IsNullOrWhiteSpace(customerName))
            return BadRequest("Missing X-Customer-Name header");

        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest("Missing X-Customer-Phone header");

        var result = await _caddieService.CreateBookingAsync(input, customerId, customerName, phone);
        return Ok(result);
    }

    /// <summary>
    /// GET lịch sử booking của customer
    /// GET /api/mini-app/caddie/booking/history?customerId={guid}
    /// </summary>
    [HttpGet("booking/history")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBookingHistory([FromQuery] Guid customerId)
    {
        if (customerId == Guid.Empty)
            return BadRequest("Missing customerId");

        var result = await _caddieService.GetBookingHistoryAsync(customerId);
        return Ok(result);
    }

    /// <summary>
    /// POST đánh giá caddie
    /// POST /api/mini-app/caddie/rating
    /// Body: { bookingId, overallRating, comment, skillRatings: [{ skillId, score }] }
    /// Header: X-Customer-Id
    /// </summary>
    [HttpPost("rating")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateRating([FromBody] MiniAppCreateCaddieRatingDto input)
    {
        var customerIdHeader = Request.Headers["X-Customer-Id"].ToString();

        if (string.IsNullOrWhiteSpace(customerIdHeader) || !Guid.TryParse(customerIdHeader, out var customerId))
            return BadRequest("Missing or invalid X-Customer-Id header");

        await _caddieService.CreateRatingAsync(input, customerId);
        return Ok(new { message = "Đánh giá thành công. Chờ quản trị viên duyệt." });
    }

    /// <summary>
    /// GET danh sách kỹ năng active (cho form đánh giá)
    /// GET /api/mini-app/caddie/skills
    /// </summary>
    [HttpGet("skills")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveSkills()
    {
        var result = await _caddieService.GetActiveSkillsAsync();
        return Ok(result);
    }
}
