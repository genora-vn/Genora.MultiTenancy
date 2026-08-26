using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService quản lý mẫu frame (Hl25FrameTemplate) — CRUD chuẩn + upload ảnh mẫu frame.
/// </summary>
public interface IHl25FrameTemplateAppService :
    ICrudAppService<
        Hl25FrameTemplateDto,
        Guid,
        GetHl25FrameTemplateListInput,
        CreateUpdateHl25FrameTemplateDto>
{
    /// <summary>Upload ảnh mẫu frame qua ManageImageService (validate 5MB), trả URL.</summary>
    Task<string> UploadTemplateImageAsync(IRemoteStreamContent file);
}
