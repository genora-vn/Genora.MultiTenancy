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

    /// <summary>Tạo thiệp (ghi lịch sử tạo ảnh). CHƯA cộng lượt — chờ chia sẻ.</summary>
    Task<Hl25FrameResultDto> CreateFrameAsync(Hl25CreateFrameRequest request);

    /// <summary>Xác nhận chia sẻ thiệp thành công → hoàn tất 1 chu kỳ, +1 lượt (nếu chưa đạt trần).</summary>
    Task<Hl25ShareResultDto> ShareFrameAsync(Hl25ShareFrameRequest request);

    /// <summary>Lấy cấu hình vòng quay + số lượt còn lại của người chơi.</summary>
    Task<Hl25MiniAppWheelDto> GetWheelAsync(string zaloUserId);

    /// <summary>Thực hiện quay (transaction ACID: trừ lượt + trừ kho quà + ghi SpinLog).</summary>
    Task<Hl25SpinResultDto> SpinAsync(Hl25SpinRequest request);

    /// <summary>Lịch sử nhận quà của người chơi.</summary>
    Task<List<Hl25MyGiftDto>> GetMyGiftsAsync(string zaloUserId);
}
