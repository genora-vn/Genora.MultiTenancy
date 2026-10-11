
using System.Threading.Tasks;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppImages
{
    public interface IManageImageService
    {
        Task<string> UploadImageAsync(IRemoteStreamContent file, string tenantId = "host", string subFolder = "images", string[] allowedExtensions = null);
        Task DeleteFileAsync(string fileUrl);

        /// <summary>
        /// Upload bytes thô (KHÔNG decode/re-encode qua ImageSharp — dùng cho ảnh chia sẻ ranking HLG cần giữ
        /// nguyên bytes gốc, hoặc video admin). Tự chọn Local/S3 theo subFolder whitelist như UploadImageAsync.
        /// </summary>
        Task<string> UploadBytesAsync(byte[] content, string contentType, string tenantId, string subFolder, string fileName = null);

        /// <summary>
        /// Copy 1 ảnh đã upload sang tên/key mới (dùng cho đổi tên ảnh thiệp hl25 theo ZaloUserId+timestamp).
        /// Trả về URL mới, hoặc null nếu nguồn không tồn tại (caller tự fallback giữ URL gốc).
        /// </summary>
        Task<string> CopyAsync(string sourceUrlOrPath, string tenantId, string subFolder, string newFileName);

        /// <summary>Đọc bytes 1 ảnh/file đã upload (dùng cho ZIP tải toàn bộ ảnh thiệp admin). Null nếu không tồn tại.</summary>
        Task<byte[]> OpenReadAsync(string urlOrPath);
    }
}
