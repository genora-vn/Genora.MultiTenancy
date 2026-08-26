using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input lọc danh sách người tham gia.</summary>
public class GetHl25ParticipantListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Tìm theo họ tên hoặc SĐT.</summary>
    public string? FilterText { get; set; }

    /// <summary>Lọc theo trạng thái Follow OA.</summary>
    public bool? IsFollowingOa { get; set; }

    /// <summary>Lọc theo trạng thái đồng ý chia sẻ thông tin.</summary>
    public bool? HasConsent { get; set; }

    public DateTime? JoinedFrom { get; set; }
    public DateTime? JoinedTo { get; set; }
}
