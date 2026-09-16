using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.AppDtos.Hl25.MiniApp;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
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
    private readonly IZaloApiClient _zaloApiClient;

    public HoaLinh25MiniAppController(IMiniAppHl25Service service, IZaloApiClient zaloApiClient)
    {
        _service = service;
        _zaloApiClient = zaloApiClient;
    }

    /// <summary>Giải mã số điện thoại từ Zalo code + accessToken (FE lấy SĐT người dùng).</summary>
    [HttpPost("decode-phone")]
    public async Task<IActionResult> DecodePhone([FromBody] ZaloDecodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Missing code or accessToken");

        var result = await _zaloApiClient.DecodePhoneAsync(request.Code, request.AccessToken, ct);
        return Ok(result);
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

    /// <summary>Tạo thiệp; chỉ cấp lượt tự nhận đầu tiên. Chia sẻ để nhận lượt thứ hai.</summary>
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

    /// <summary>Xác nhận chia sẻ thiệp → nhận lượt thứ hai sau lượt tạo thiệp.</summary>
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

    // ===== (Delta 2026-09) Frame — public read =====

    /// <summary>Danh sách chiến dịch ghép ảnh đang hoạt động.</summary>
    [HttpGet("frames/campaigns")]
    public async Task<IActionResult> GetFrameCampaigns()
    {
        try
        {
            var data = await _service.GetFrameCampaignsAsync();
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25FrameCampaignPublicDto>>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25FrameCampaignPublicDto>>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Danh sách mẫu frame (khung ảnh) để người dùng ướm ảnh. Lọc theo chiến dịch nếu truyền campaignId.</summary>
    [HttpGet("frames/templates")]
    public async Task<IActionResult> GetFrameTemplates([FromQuery] System.Guid? campaignId)
    {
        try
        {
            var data = await _service.GetFrameTemplatesAsync(campaignId);
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25FrameTemplatePublicDto>>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25FrameTemplatePublicDto>>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Lịch sử tạo ảnh của người chơi.</summary>
    [HttpGet("me/frames")]
    public async Task<IActionResult> GetMyFrameCreations([FromQuery] string zaloUserId)
    {
        try
        {
            var data = await _service.GetMyFrameCreationsAsync(zaloUserId);
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25FrameCreationPublicDto>>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25FrameCreationPublicDto>>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    // ===== (Delta 2026-09) Wheel — public read =====

    /// <summary>Danh sách kho quà tặng (FE hiển thị thông tin quà khi trúng).</summary>
    [HttpGet("gifts")]
    public async Task<IActionResult> GetGifts()
    {
        try
        {
            var data = await _service.GetGiftsAsync();
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25GiftPublicDto>>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25GiftPublicDto>>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Lịch sử NHẬN lượt quay của người chơi.</summary>
    [HttpGet("me/spin-turns")]
    public async Task<IActionResult> GetMySpinTurnLogs([FromQuery] string zaloUserId)
    {
        try
        {
            var data = await _service.GetMySpinTurnLogsAsync(zaloUserId);
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25SpinTurnLogPublicDto>>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25SpinTurnLogPublicDto>>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    /// <summary>Lịch sử lượt quay ĐÃ THỰC HIỆN của người chơi.</summary>
    [HttpGet("me/spins")]
    public async Task<IActionResult> GetMySpinLogs([FromQuery] string zaloUserId)
    {
        try
        {
            var data = await _service.GetMySpinLogsAsync(zaloUserId);
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25SpinLogPublicDto>>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<System.Collections.Generic.List<Hl25SpinLogPublicDto>>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    // ===== (Delta 2026-09) Upload ảnh =====

    /// <summary>Upload ảnh (ảnh thiệp / ảnh người dùng) lên server, trả về URL đầy đủ (endpoint + path).</summary>
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Ok(Hl25ApiResult<Hl25UploadImageResultDto>.Fail("Hl25:ImageRequired", "Thiếu file ảnh."));

            await using var stream = file.OpenReadStream();
            var content = new Volo.Abp.Content.RemoteStreamContent(stream, file.FileName, file.ContentType, file.Length);
            var data = await _service.UploadImageAsync(content);
            return Ok(Hl25ApiResult<Hl25UploadImageResultDto>.Ok(data));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(Hl25ApiResult<Hl25UploadImageResultDto>.Fail(ex.Code ?? "error", ex.Message));
        }
    }

    // ===== Export/Download (Admin) =====

    /// <summary>Download toàn bộ ảnh thiệp đã tạo (ZIP). Yêu cầu đăng nhập + quyền Frames.</summary>
    [HttpGet("admin/frame-creations/download-all-images")]
    [Authorize]
    public async Task<IActionResult> DownloadAllFrameImages([FromServices] IHl25FrameCreationAppService frameService)
    {
        var stream = await frameService.DownloadAllImagesAsync();
        return File(stream.GetStream(), stream.ContentType, stream.FileName);
    }
}
