using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AuditLogs;

public class AuditLogListDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public DateTime ExecutionTime { get; set; }
    public string Url { get; set; }
    public string HttpMethod { get; set; }
    public string UserName { get; set; }
    public int ExecutionDuration { get; set; }
    public bool HasException { get; set; }
    public string ClientIpAddress { get; set; }
    public string CorrelationId { get; set; }
}
