using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input lọc danh sách chiến dịch ghép ảnh.</summary>
public class GetHl25FrameCampaignListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Tìm theo tên chiến dịch.</summary>
    public string? FilterText { get; set; }

    /// <summary>Lọc theo trạng thái.</summary>
    public Hl25CampaignStatus? Status { get; set; }
}
