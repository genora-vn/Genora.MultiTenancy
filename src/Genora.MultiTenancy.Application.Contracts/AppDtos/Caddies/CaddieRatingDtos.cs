using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class CaddieRatingDto : EntityDto<Guid>
{
    public Guid BookingId { get; set; }
    public string? BookingCode { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAvatar { get; set; }
    public Guid CaddieId { get; set; }
    public string? CaddieName { get; set; }
    public string? CaddieCode { get; set; }
    public int OverallRating { get; set; }
    public string? Comment { get; set; }
    public byte ApprovalStatus { get; set; }
    public string? ApprovalStatusText { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? RejectReason { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? BookingDate { get; set; }
    public TimeSpan? BookingStartTime { get; set; }
    public List<CaddieRatingDetailDto> Details { get; set; } = new();
}

public class CaddieRatingDetailDto
{
    public Guid SkillId { get; set; }
    public string? SkillName { get; set; }
    public int Score { get; set; }
}

public class GetCaddieRatingListInput : PagedAndSortedResultRequestDto
{
    public Guid? CaddieId { get; set; }
    public byte? ApprovalStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class ApproveRejectRatingDto
{
    public byte ApprovalStatus { get; set; }
    public string? RejectReason { get; set; }
}
