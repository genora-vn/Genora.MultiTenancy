using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService cấu hình chung Mini App "Dược Phẩm Hoa Linh 25 Năm".
/// Singleton theo tenant: mỗi tenant có đúng 1 bản ghi cấu hình (tự tạo mặc định nếu chưa có).
/// </summary>
public interface IHl25AppConfigAppService : IApplicationService
{
    /// <summary>Lấy cấu hình của tenant hiện tại (tạo mặc định nếu chưa có).</summary>
    Task<Hl25AppConfigDto> GetAsync();

    /// <summary>Cập nhật cấu hình của tenant hiện tại.</summary>
    Task<Hl25AppConfigDto> UpdateAsync(CreateUpdateHl25AppConfigDto input);

    /// <summary>
    /// Upload asset (logo/banner) qua ManageImageService, trả URL.
    /// assetType: "logo" | "banner". Validate 5MB trước khi upload.
    /// </summary>
    Task<string> UploadAssetAsync(IRemoteStreamContent file, string assetType);
}
