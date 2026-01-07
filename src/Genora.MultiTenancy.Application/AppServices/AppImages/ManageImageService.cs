using Genora.MultiTenancy.AppDtos.AppImages;
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
        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return Task.CompletedTask;

            var filePath = Path.Combine(_env.ContentRootPath, "wwwroot", fileUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); }
                catch (Exception ex) { throw ex; }
            }
            return Task.CompletedTask;
        }

        public async Task<string> UploadImageAsync(IRemoteStreamContent file, string tenantId, string subFolder = "images", string[] allowedExtensions = null)
        {
            tenantId = !string.IsNullOrEmpty(tenantId) ? tenantId : "host";
            if (file == null || file.ContentLength == 0)
                throw new UserFriendlyException("Vui lòng chọn file ảnh.");

            //if (file.ContentLength > 10 * 1024 * 1024)
            //    throw new UserFriendlyException("File không được quá 10MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            allowedExtensions ??= _defaultAllowed;

            if (!allowedExtensions.Contains(ext))
                throw new UserFriendlyException($"Chỉ chấp nhận định dạng: {string.Join(", ", allowedExtensions)}");

            // Tạo thư mục
            var uploadsRoot = Path.Combine("wwwroot","uploads", subFolder, tenantId);
            Directory.CreateDirectory(uploadsRoot);

            // Tên file duy nhất
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(uploadsRoot, fileName);

            // Resize + lưu ảnh (dùng ImageSharp - cực nhẹ và mạnh)
            using var image = await Image.LoadAsync(file.GetStream());
            //if (image.Width > maxWidth)
            //{
            //    image.Mutate(x => x.Resize(maxWidth, 0)); // giữ tỷ lệ
            //}
            await image.SaveAsync(filePath);

            // Trả về URL để lưu vào DB
            return $"/uploads/{subFolder}/{tenantId}/{fileName}";
        }
    }
}
