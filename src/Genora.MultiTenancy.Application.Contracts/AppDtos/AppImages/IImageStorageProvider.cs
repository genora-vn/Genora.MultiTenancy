using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppImages;

/// <summary>
/// Lưu trữ "backend" vật lý cho ảnh/video (local wwwroot HOẶC S3/compatible). <see cref="Genora.MultiTenancy.AppServices.AppImages.ManageImageService"/>
/// là facade public (không đổi API), chọn provider nào active theo cấu hình <c>Storage</c> + whitelist subFolder.
/// Dùng chung cho hl25 + hlg (và các module khác nếu opt-in sau), tránh viết lại logic lưu/đọc/copy/xóa file cho từng module.
/// </summary>
public interface IImageStorageProvider
{
    /// <summary>
    /// Upload ảnh (đã validate/định dạng) lên storage. <paramref name="key"/> là path tương đối đã dựng sẵn
    /// (ví dụ "uploads/hl25/{tenantId}/{guid}.jpg"), KHÔNG có dấu "/" ở đầu.
    /// Trả về URL công khai để lưu DB / trả FE (tuyệt đối với S3; tương đối "/..." với Local — ImageHelper/ToFullUrl
    /// vẫn dựng full URL đúng như hiện tại).
    /// </summary>
    Task<string> SaveAsync(string key, byte[] content, string contentType);

    /// <summary>
    /// Copy 1 object/file đã có sang key mới (dùng cho đổi tên ảnh thiệp theo ZaloUserId+timestamp ở hl25).
    /// <paramref name="sourceUrlOrKey"/> có thể là URL tuyệt đối (S3 cũ) hoặc path tương đối "/uploads/..." (local cũ).
    /// Trả về URL công khai của bản copy mới, hoặc null nếu nguồn không tồn tại (caller tự fallback giữ URL gốc).
    /// </summary>
    Task<string?> CopyAsync(string sourceUrlOrKey, string destKey);

    /// <summary>
    /// Đọc nội dung file/object về bytes (dùng cho ZIP tải toàn bộ ảnh thiệp — admin download-all).
    /// Trả về null nếu không tồn tại (caller tự skip entry đó, giữ hành vi hiện tại).
    /// </summary>
    Task<byte[]?> ReadAsync(string urlOrKey);

    /// <summary>Xóa 1 file/object. Không throw nếu không tồn tại (giữ hành vi hiện tại của ManageImageService).</summary>
    Task DeleteAsync(string urlOrKey);
}
