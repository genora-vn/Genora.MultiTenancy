using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppEmails;

[Table("AppEmails")]
public class Email : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string TemplateName { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;               // rendered body (để admin xem lại)

    public string ToEmails { get; set; } = default!;           // "a@x.com;b@y.com"
    public string? CcEmails { get; set; }
    public string? BccEmails { get; set; }

    public string? ModelJson { get; set; }                     // lưu model để audit / rebuild

    public EmailStatus Status { get; set; } = EmailStatus.Pending;

    public int TryCount { get; set; } = 0;
    public DateTime? LastTryTime { get; set; }
    public DateTime? NextTryTime { get; set; }
    public DateTime? SentTime { get; set; }

    public string? LastError { get; set; }

    // liên kết nghiệp vụ (để filter theo booking)
    public Guid? BookingId { get; set; }
    public string? BookingCode { get; set; }

    protected Email() { }

    public Email(Guid id) : base(id) { }
}