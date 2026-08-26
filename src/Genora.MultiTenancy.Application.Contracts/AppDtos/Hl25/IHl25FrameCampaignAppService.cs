using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// AppService quản lý chiến dịch ghép ảnh (Hl25FrameCampaign) — CRUD chuẩn.
/// Danh sách mẫu frame (templates) được quản lý riêng qua IHl25FrameTemplateAppService.
/// </summary>
public interface IHl25FrameCampaignAppService :
    ICrudAppService<
        Hl25FrameCampaignDto,
        Guid,
        GetHl25FrameCampaignListInput,
        CreateUpdateHl25FrameCampaignDto>
{
}
