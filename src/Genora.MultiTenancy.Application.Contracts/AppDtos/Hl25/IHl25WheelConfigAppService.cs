using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService cấu hình vòng quay may mắn (singleton theo tenant).
/// Quản lý cấu hình UI + danh sách ô quay (slots) kèm tỷ lệ trúng.
/// </summary>
public interface IHl25WheelConfigAppService : IApplicationService
{
    /// <summary>Lấy cấu hình vòng quay của tenant hiện tại (tạo mặc định nếu chưa có), kèm slots.</summary>
    Task<Hl25WheelConfigDto> GetAsync();

    /// <summary>
    /// Cập nhật cấu hình + toàn bộ slots (thay thế danh sách slots).
    /// Validate: tổng WinRate các ô phải = 100.
    /// </summary>
    Task<Hl25WheelConfigDto> UpdateAsync(CreateUpdateHl25WheelConfigDto input);

    /// <summary>Upload asset vòng quay (background/pointer/slot image) qua ManageImageService (validate 5MB).</summary>
    Task<string> UploadWheelImageAsync(IRemoteStreamContent file);
}
