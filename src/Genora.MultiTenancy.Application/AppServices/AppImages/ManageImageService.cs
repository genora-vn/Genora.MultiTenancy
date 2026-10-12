using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppImages
{
    /// <summary>
    /// Facade public cho upload/copy/đọc/xóa ảnh+video — API giữ nguyên như trước (callers không đổi).
    /// Chọn LocalImageStorageProvider hoặc S3ImageStorageProvider theo cấu hình Storage:Provider + whitelist
    /// Storage:S3:SubFolders (chỉ subFolder nằm trong whitelist mới dùng S3; subFolder khác luôn Local dù
    /// Provider=S3 — cho phép bật S3 riêng cho hl25+hlg mà không ảnh hưởng module khác như salon/news/pro).
    /// </summary>
    public class ManageImageService : IManageImageService, ITransientDependency
    {
        private readonly LocalImageStorageProvider _local;
        private readonly S3ImageStorageProvider _s3;
        private readonly StorageOptions _options;

        private readonly string[] _defaultAllowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ManageImageService(
            LocalImageStorageProvider local,
            S3ImageStorageProvider s3,
            IOptions<StorageOptions> options)
        {
            _local = local;
            _s3 = s3;
            _options = options.Value;
        }

        private static BusinessException Err(string code, string field, object value = null)
        {
            var ex = new BusinessException(code)
                .WithData("Field", field);

            if (value != null) ex.WithData("Value", value);
            return ex;
        }

        /// <summary>
        /// S3 chỉ dùng khi Provider=S3 VÀ subFolder (hoặc SEGMENT ĐẦU của subFolder, VD "hlg/ranking-share" → "hlg")
        /// nằm trong whitelist S3:SubFolders. Match theo tiền tố để các subFolder lồng nhau của cùng module
        /// (VD "hlg/videos", "hlg/ranking-share") đều dùng đúng storage theo cấu hình của module đó.
        /// </summary>
        private IImageStorageProvider ResolveProvider(string subFolder)
        {
            if (!string.Equals(_options.Provider, "S3", StringComparison.OrdinalIgnoreCase)) return _local;
            var rootSegment = subFolder.Split('/')[0];
            var useS3 = _options.S3.SubFolders.Any(x => string.Equals(x, rootSegment, StringComparison.OrdinalIgnoreCase));
            return useS3 ? _s3 : _local;
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return;

            try
            {
                // URL tuyệt đối (S3) hay path tương đối "/uploads/..." (Local) đều tự nhận diện qua provider tương ứng.
                var provider = fileUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                               fileUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? (IImageStorageProvider)_s3
                    : _local;
                await provider.DeleteAsync(fileUrl);
            }
            catch (Exception ex)
            {
                throw Err(ImageErrorCodes.DeleteFailed, "FileUrl", fileUrl)
                    .WithData("Exception", ex.GetType().FullName ?? "Exception")
                    .WithData("Message", ex.Message);
            }
        }

        public async Task<string> UploadImageAsync(
            IRemoteStreamContent file,
            string tenantId,
            string subFolder = "images",
            string[] allowedExtensions = null)
        {
            tenantId = !string.IsNullOrWhiteSpace(tenantId) ? tenantId : "host";

            if (file == null || file.ContentLength == 0)
                throw Err(ImageErrorCodes.FileRequired, "File");

            allowedExtensions ??= _defaultAllowed;

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
            {
                throw Err(ImageErrorCodes.InvalidExtension, "FileName", file.FileName)
                    .WithData("Allowed", string.Join(", ", allowedExtensions))
                    .WithData("Extension", ext);
            }

            byte[] encoded;
            try
            {
                // Decode qua ImageSharp (loại payload giả mạo ext + validate định dạng ảnh thật) rồi re-encode ra bytes.
                using var image = await Image.LoadAsync(file.GetStream());
                using var ms = new MemoryStream();
                await image.SaveAsync(ms, GetEncoder(ext));
                encoded = ms.ToArray();
            }
            catch (Exception exDecode)
            {
                throw Err(ImageErrorCodes.DecodeFailed, "FileName", file.FileName)
                    .WithData("Allowed", string.Join(", ", allowedExtensions))
                    .WithData("Exception", exDecode.GetType().FullName ?? "Exception")
                    .WithData("Message", exDecode.Message);
            }

            try
            {
                var fileName = Guid.NewGuid().ToString("N") + ext;
                var key = $"uploads/{subFolder}/{tenantId}/{fileName}";
                return await ResolveProvider(subFolder).SaveAsync(key, encoded, GetContentType(ext));
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw Err(ImageErrorCodes.UploadFailed, "FileName", file.FileName)
                    .WithData("TenantId", tenantId)
                    .WithData("SubFolder", subFolder)
                    .WithData("ContentLength", file.ContentLength)
                    .WithData("Exception", ex.GetType().FullName ?? "Exception")
                    .WithData("Message", ex.Message);
            }
        }

        public async Task<string> UploadBytesAsync(byte[] content, string contentType, string tenantId, string subFolder, string fileName = null)
        {
            tenantId = !string.IsNullOrWhiteSpace(tenantId) ? tenantId : "host";
            if (content == null || content.Length == 0)
                throw Err(ImageErrorCodes.FileRequired, "Content");

            var name = !string.IsNullOrWhiteSpace(fileName) ? fileName : Guid.NewGuid().ToString("N");
            var key = $"uploads/{subFolder}/{tenantId}/{name}";
            try
            {
                return await ResolveProvider(subFolder).SaveAsync(key, content, contentType ?? "application/octet-stream");
            }
            catch (Exception ex)
            {
                throw Err(ImageErrorCodes.UploadFailed, "FileName", name)
                    .WithData("TenantId", tenantId)
                    .WithData("SubFolder", subFolder)
                    .WithData("Exception", ex.GetType().FullName ?? "Exception")
                    .WithData("Message", ex.Message);
            }
        }

        public async Task<string> CopyAsync(string sourceUrlOrPath, string tenantId, string subFolder, string newFileName)
        {
            if (string.IsNullOrWhiteSpace(sourceUrlOrPath))
                return null;

            tenantId = !string.IsNullOrWhiteSpace(tenantId) ? tenantId : "host";
            var destKey = $"uploads/{subFolder}/{tenantId}/{newFileName}";

            // Nguồn có thể đang ở Local (ảnh cũ) hoặc S3 (ảnh mới) — thử đúng provider theo định dạng URL trước,
            // provider hiện tại (theo cấu hình) sau, để không miss trường hợp mixed state.
            var isAbsolute = sourceUrlOrPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             sourceUrlOrPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            var primary = isAbsolute ? (IImageStorageProvider)_s3 : _local;
            var fallback = isAbsolute ? (IImageStorageProvider)_local : _s3;

            var result = await primary.CopyAsync(sourceUrlOrPath, destKey);
            if (result != null) return result;
            return await fallback.CopyAsync(sourceUrlOrPath, destKey);
        }

        public async Task<byte[]> OpenReadAsync(string urlOrPath)
        {
            if (string.IsNullOrWhiteSpace(urlOrPath))
                return null;

            var isAbsolute = urlOrPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             urlOrPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            var primary = isAbsolute ? (IImageStorageProvider)_s3 : _local;
            var fallback = isAbsolute ? (IImageStorageProvider)_local : _s3;

            var result = await primary.ReadAsync(urlOrPath);
            if (result != null) return result;
            return await fallback.ReadAsync(urlOrPath);
        }

        private static string GetContentType(string ext) => ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        private static SixLabors.ImageSharp.Formats.IImageEncoder GetEncoder(string ext) => ext switch
        {
            ".jpg" or ".jpeg" => new JpegEncoder(),
            ".png" => new PngEncoder(),
            ".gif" => new GifEncoder(),
            ".webp" => new WebpEncoder(),
            _ => new PngEncoder()
        };
    }
}
