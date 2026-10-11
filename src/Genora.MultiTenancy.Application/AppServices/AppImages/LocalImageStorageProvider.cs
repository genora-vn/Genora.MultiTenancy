using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.AppImages;

/// <summary>
/// Lưu trữ local wwwroot — hành vi GIỮ NGUYÊN như <c>ManageImageService</c> cũ trước khi tách provider.
/// Không đăng ký trực tiếp qua DI tag (ITransientDependency) để tránh ABP tự bind nhầm <see cref="Genora.MultiTenancy.AppDtos.AppImages.IImageStorageProvider"/>
/// khi có 2 implementation cùng lúc; <see cref="ManageImageService"/> tự new 2 provider và chọn theo config.
/// </summary>
public class LocalImageStorageProvider : Genora.MultiTenancy.AppDtos.AppImages.IImageStorageProvider
{
    public Task<string> SaveAsync(string key, byte[] content, string contentType)
    {
        var relative = key.TrimStart('/');
        var filePath = Path.Combine("wwwroot", relative.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(filePath, content);
        return Task.FromResult("/" + relative);
    }

    public Task<string?> CopyAsync(string sourceUrlOrKey, string destKey)
    {
        var sourcePath = ToPhysicalPath(sourceUrlOrKey);
        if (!File.Exists(sourcePath)) return Task.FromResult<string?>(null);

        var destRelative = destKey.TrimStart('/');
        var destPath = Path.Combine("wwwroot", destRelative.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.Copy(sourcePath, destPath, overwrite: false);
        return Task.FromResult<string?>("/" + destRelative);
    }

    public Task<byte[]?> ReadAsync(string urlOrKey)
    {
        var path = ToPhysicalPath(urlOrKey);
        if (!File.Exists(path)) return Task.FromResult<byte[]?>(null);
        return Task.FromResult<byte[]?>(File.ReadAllBytes(path));
    }

    public Task DeleteAsync(string urlOrKey)
    {
        var path = ToPhysicalPath(urlOrKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    /// <summary>Chấp nhận URL tuyệt đối (lấy AbsolutePath) hoặc path tương đối "/uploads/...".</summary>
    private static string ToPhysicalPath(string urlOrKey)
    {
        string relative;
        if (urlOrKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            urlOrKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            relative = new Uri(urlOrKey).AbsolutePath;
        }
        else
        {
            relative = urlOrKey;
        }
        return Path.Combine("wwwroot", relative.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
    }
}
