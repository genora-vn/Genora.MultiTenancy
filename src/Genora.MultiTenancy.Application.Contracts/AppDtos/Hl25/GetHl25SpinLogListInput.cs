using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input lọc lịch sử lượt quay.</summary>
public class GetHl25SpinLogListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Lọc theo người tham gia.</summary>
    public Guid? ParticipantId { get; set; }

    /// <summary>Lọc theo quà.</summary>
    public Guid? GiftId { get; set; }

    /// <summary>Lọc theo trạng thái trao thưởng.</summary>
    public Hl25RewardStatus? RewardStatus { get; set; }

    /// <summary>Tìm theo tên/SĐT người tham gia.</summary>
    public string? FilterText { get; set; }

    public DateTime? SpinFrom { get; set; }
    public DateTime? SpinTo { get; set; }
}
