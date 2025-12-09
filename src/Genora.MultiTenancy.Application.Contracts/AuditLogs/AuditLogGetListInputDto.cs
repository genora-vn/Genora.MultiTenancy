using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AuditLogs;

public class AuditLogGetListInputDto : PagedAndSortedResultRequestDto
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Url { get; set; }
    public string HttpMethod { get; set; }
    public bool? HasException { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string CorrelationId { get; set; }
}
