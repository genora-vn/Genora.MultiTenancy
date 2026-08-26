using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input lọc danh sách mẫu frame.</summary>
public class GetHl25FrameTemplateListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Lọc theo chiến dịch.</summary>
    public Guid? CampaignId { get; set; }

    /// <summary>Tìm theo tên mẫu.</summary>
    public string? FilterText { get; set; }

    /// <summary>Lọc theo trạng thái kích hoạt.</summary>
    public bool? IsActive { get; set; }
}
