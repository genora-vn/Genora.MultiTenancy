using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class CreateUpdateEmailDto
{
    [Required] public string ToEmails { get; set; } = default!;
    public string? CcEmails { get; set; }
    public string? BccEmails { get; set; }

    [Required] public string Subject { get; set; } = default!;
    [Required] public string Body { get; set; } = default!;

    public Guid? BookingId { get; set; }
    public string? BookingCode { get; set; }
}