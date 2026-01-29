using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppImages
{
    public class ManageImageService : IManageImageService, ITransientDependency
    {
        private readonly IHostEnvironment _env;

        public ManageImageService(IHostEnvironment env)
        {
            _env = env;
        }

        private readonly string[] _defaultAllowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static BusinessException Err(string code, string field, object? value = null)
        {
            var ex = new BusinessException(code)
                .WithData("Field", field);

            if (value != null) ex.WithData("Value", value);
            return ex;
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.CompletedTask;

            // NOTE: giữ behavior cũ: fileUrl dạng "/uploads/..."
            var filePath = Path.Combine("wwwroot", fileUrl.TrimStart('/'));

            if (!File.Exists(filePath))
                return Task.CompletedTask;

            try
            {
                File.Delete(filePath);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw Err(ImageErrorCodes.DeleteFailed, "FileUrl", fileUrl)
                    .WithData("FilePath", filePath)
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

            // allowed extensions
            allowedExtensions ??= _defaultAllowed;

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
            {
                throw Err(ImageErrorCodes.InvalidExtension, "FileName", file.FileName)
                    .WithData("Allowed", string.Join(", ", allowedExtensions))
                    .WithData("Extension", ext);
            }

            try
            {
                // Tạo thư mục
                var uploadsRoot = Path.Combine("wwwroot", "uploads", subFolder, tenantId);
                Directory.CreateDirectory(uploadsRoot);

                // Tên file duy nhất
                var fileName = Guid.NewGuid().ToString("N") + ext;
                var filePath = Path.Combine(uploadsRoot, fileName);

                // Decode + lưu ảnh
                try
                {
                    using var image = await Image.LoadAsync(file.GetStream());
                    await image.SaveAsync(filePath);
                }
                catch (Exception exDecode)
                {
                    throw Err(ImageErrorCodes.DecodeFailed, "FileName", file.FileName)
                        .WithData("Allowed", string.Join(", ", allowedExtensions))
                        .WithData("Exception", exDecode.GetType().FullName ?? "Exception")
                        .WithData("Message", exDecode.Message);
                }

                // Trả về URL để lưu vào DB
                return $"/uploads/{subFolder}/{tenantId}/{fileName}";
            }
            catch (BusinessException)
            {
                // giữ nguyên lỗi chuẩn hoá
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
    }
}
