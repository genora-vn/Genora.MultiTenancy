using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25.MiniApp;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hl25.MiniApp;

/// <summary>
/// AppService phục vụ Zalo Mini App "Dược Phẩm Hoa Linh 25 Năm" (public, anonymous).
/// Định danh người chơi qua ZaloUserId (upsert theo (TenantId, ZaloUserId)).
/// </summary>
public interface IMiniAppHl25Service : IApplicationService
{
    /// <summary>Lấy cấu hình + thể lệ chương trình.</summary>
    Task<Hl25MiniAppConfigDto> GetConfigAsync();

    /// <summary>Đăng ký/upsert người tham gia theo ZaloUserId.</summary>
    Task<Hl25MeDto> RegisterAsync(Hl25RegisterRequest request);

    /// <summary>Lấy thông tin người chơi hiện tại theo ZaloUserId.</summary>
    Task<Hl25MeDto> GetMeAsync(string zaloUserId);

    /// <summary>Cập nhật thông tin cá nhân.</summary>
    Task<Hl25MeDto> UpdateProfileAsync(Hl25UpdateProfileRequest request);

    /// <summary>Tạo thiệp và cấp lượt tự nhận đầu tiên; tạo thêm thiệp không cộng lượt.</summary>
    Task<Hl25FrameResultDto> CreateFrameAsync(Hl25CreateFrameRequest request);

    /// <summary>Xác nhận chia sẻ thiệp thành công → cấp lượt tự nhận thứ hai, chỉ một lần.</summary>
    Task<Hl25ShareResultDto> ShareFrameAsync(Hl25ShareFrameRequest request);

    /// <summary>Lấy cấu hình vòng quay + số lượt còn lại của người chơi.</summary>
    Task<Hl25MiniAppWheelDto> GetWheelAsync(string zaloUserId);

    /// <summary>Thực hiện quay (transaction ACID: trừ lượt + trừ kho quà + ghi SpinLog).</summary>
    Task<Hl25SpinResultDto> SpinAsync(Hl25SpinRequest request);

    /// <summary>Lịch sử nhận quà của người chơi.</summary>
    Task<List<Hl25MyGiftDto>> GetMyGiftsAsync(string zaloUserId);

    // ===== (Delta 2026-09) Frame — public read =====

    /// <summary>Danh sách chiến dịch ghép ảnh đang hoạt động (kèm số mẫu frame).</summary>
    Task<List<Hl25FrameCampaignPublicDto>> GetFrameCampaignsAsync();

    /// <summary>Danh sách mẫu frame (khung ảnh) đang bật để người dùng ướm ảnh. Lọc theo chiến dịch nếu truyền campaignId.</summary>
    Task<List<Hl25FrameTemplatePublicDto>> GetFrameTemplatesAsync(Guid? campaignId);

    /// <summary>Lịch sử tạo ảnh của người chơi (theo ZaloUserId).</summary>
    Task<List<Hl25FrameCreationPublicDto>> GetMyFrameCreationsAsync(string zaloUserId);

    // ===== (Delta 2026-09) Wheel — public read =====

    /// <summary>Danh sách kho quà tặng (FE hiển thị thông tin quà khi trúng).</summary>
    Task<List<Hl25GiftPublicDto>> GetGiftsAsync();

    /// <summary>Lịch sử NHẬN lượt quay của người chơi (theo ZaloUserId).</summary>
    Task<List<Hl25SpinTurnLogPublicDto>> GetMySpinTurnLogsAsync(string zaloUserId);

    /// <summary>Lịch sử lượt quay ĐÃ THỰC HIỆN của người chơi (theo ZaloUserId).</summary>
    Task<List<Hl25SpinLogPublicDto>> GetMySpinLogsAsync(string zaloUserId);

    // ===== (Delta 2026-09) Upload ảnh =====

    /// <summary>Upload ảnh (ảnh thiệp / ảnh người dùng) lên server, trả về URL đầy đủ (endpoint + path).</summary>
    Task<Hl25UploadImageResultDto> UploadImageAsync(Volo.Abp.Content.IRemoteStreamContent file);
}
