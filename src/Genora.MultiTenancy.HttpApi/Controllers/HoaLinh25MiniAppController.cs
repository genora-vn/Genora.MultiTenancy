using Genora.MultiTenancy.AppDtos.Hl25.MiniApp;
using Genora.MultiTenancy.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;

namespace Genora.MultiTenancy.HttpApi.Controllers;

/// <summary>
/// Controller cho Zalo Mini App "Dược Phẩm Hoa Linh 25 Năm".
/// Public/anonymous — định danh người chơi qua ZaloUserId. Tenant resolve theo ABP mặc định
/// (query/header __tenant). Bọc mọi response trong Hl25ApiResult&lt;T&gt;.
/// </summary>
[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Area("MultiTenancy")]
[Route("api/mini-app/hl25")]
[AllowAnonymous]
public class HoaLinh25MiniAppController : MultiTenancyController
{
    private readonly IMiniAppHl25Service _service;

    public HoaLinh25MiniAppController(IMiniAppHl25Service service)
    {
        _service = service;
    }

    /// <summary>Cấu hình + thể lệ chương trình.</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        var data = await _service.GetConfigAsync();
        return Ok(Hl25ApiResult<Hl25MiniAppConfigDto>.Ok(data));
    }

    /// <summary>Đăng ký/upsert người tham gia theo ZaloUserId.</summary>
    [HttpPost("participants/register")]
    public async Task<IActionResult> Register([FromBody] Hl25RegisterRequest request)
    {
        try
        {
            var data = await _service.RegisterAsync(request);
            return Ok(Hl25ApiResult<Hl25MeDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25MeDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Thông tin người chơi hiện tại.</summary>
    [HttpGet("participants/me")]
    public async Task<IActionResult> GetMe([FromQuery] string zaloUserId)
    {
        try
        {
            var data = await _service.GetMeAsync(zaloUserId);
            return Ok(Hl25ApiResult<Hl25MeDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25MeDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Cập nhật thông tin cá nhân.</summary>
    [HttpPut("participants/me")]
    public async Task<IActionResult> UpdateProfile([FromBody] Hl25UpdateProfileRequest request)
    {
        try
        {
            var data = await _service.UpdateProfileAsync(request);
            return Ok(Hl25ApiResult<Hl25MeDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25MeDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Tạo thiệp (chưa cộng lượt — chờ chia sẻ).</summary>
    [HttpPost("frames")]
    public async Task<IActionResult> CreateFrame([FromBody] Hl25CreateFrameRequest request)
    {
        try
        {
            var data = await _service.CreateFrameAsync(request);
            return Ok(Hl25ApiResult<Hl25FrameResultDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25FrameResultDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Xác nhận chia sẻ thiệp → cộng lượt (theo chu kỳ, trần 2).</summary>
    [HttpPost("frames/share")]
    public async Task<IActionResult> ShareFrame([FromBody] Hl25ShareFrameRequest request)
    {
        try
        {
            var data = await _service.ShareFrameAsync(request);
            return Ok(Hl25ApiResult<Hl25ShareResultDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25ShareResultDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Cấu hình vòng quay + số lượt còn lại.</summary>
    [HttpGet("wheel")]
    public async Task<IActionResult> GetWheel([FromQuery] string zaloUserId)
    {
        try
        {
            var data = await _service.GetWheelAsync(zaloUserId);
            return Ok(Hl25ApiResult<Hl25MiniAppWheelDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25MiniAppWheelDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Thực hiện quay (ACID: trừ lượt + trừ kho quà + ghi SpinLog).</summary>
    [HttpPost("wheel/spin")]
    public async Task<IActionResult> Spin([FromBody] Hl25SpinRequest request)
    {
        try
        {
            var data = await _service.SpinAsync(request);
            return Ok(Hl25ApiResult<Hl25SpinResultDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25SpinResultDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Lịch sử nhận quà của người chơi.</summary>
    [HttpGet("me/gifts")]
    public async Task<IActionResult> GetMyGifts([FromQuery] string zaloUserId)
    {
        try
        {
            var data = await _service.GetMyGiftsAsync(zaloUserId);
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25MyGiftDto>>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25MyGiftDto>>.Fail(ex.Code ?? "error", ex.Message));
        }
    }
}
