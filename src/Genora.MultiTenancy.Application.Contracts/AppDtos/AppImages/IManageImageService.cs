
using System.Threading.Tasks;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppImages
{
    public interface IManageImageService
    {
        Task<string> UploadImageAsync(IRemoteStreamContent file, string tenantId = "host", string subFolder = "images", string[] allowedExtensions = null);
        Task DeleteFileAsync(string fileUrl);
    }
}
