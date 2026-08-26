using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input lọc danh sách quà tặng.</summary>
public class GetHl25GiftListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Tìm theo tên quà.</summary>
    public string? FilterText { get; set; }

    /// <summary>Lọc theo trạng thái.</summary>
    public Hl25GiftStatus? Status { get; set; }
}
