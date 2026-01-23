using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class AppEmailDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string TemplateName { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string ToEmails { get; set; } = default!;
    public string? CcEmails { get; set; }
    public string? BccEmails { get; set; }
    public string? ModelJson { get; set; }

    public EmailStatus Status { get; set; }
    public int TryCount { get; set; }
    public DateTime? LastTryTime { get; set; }
    public DateTime? NextTryTime { get; set; }
    public DateTime? SentTime { get; set; }
    public string? LastError { get; set; }

    public Guid? BookingId { get; set; }
    public string? BookingCode { get; set; }
}