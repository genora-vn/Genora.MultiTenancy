using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService quản lý kho quà tặng (Hl25Gift) — CRUD chuẩn + upload ảnh quà.
/// </summary>
public interface IHl25GiftAppService :
    ICrudAppService<
        Hl25GiftDto,
        Guid,
        GetHl25GiftListInput,
        CreateUpdateHl25GiftDto>
{
    /// <summary>Upload ảnh quà qua ManageImageService (validate 5MB), trả URL.</summary>
    Task<string> UploadGiftImageAsync(IRemoteStreamContent file);
}
