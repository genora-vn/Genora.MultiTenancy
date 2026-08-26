using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input lọc lịch sử nhận lượt quay.</summary>
public class GetHl25SpinTurnLogListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Lọc theo người tham gia.</summary>
    public Guid? ParticipantId { get; set; }

    /// <summary>Lọc theo nguồn cộng lượt.</summary>
    public Hl25SpinTurnSource? Source { get; set; }

    /// <summary>Tìm theo tên/SĐT người tham gia.</summary>
    public string? FilterText { get; set; }

    public DateTime? GrantedFrom { get; set; }
    public DateTime? GrantedTo { get; set; }
}
