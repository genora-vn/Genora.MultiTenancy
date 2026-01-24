using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppZaloAuth;

[Table("AppZaloLog")]
public class ZaloLog : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [StringLength(128)]
    public string Action { get; set; } = null!; // AUTHORIZE_URL / EXCHANGE_CODE / REFRESH_TOKEN / SEND_ZNS / SEND_OA_MSG

    [StringLength(512)]
    public string Endpoint { get; set; } = null!;

    public int? HttpStatus { get; set; }
    public long DurationMs { get; set; }

    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string? Error { get; set; }

    protected ZaloLog() { }

    public ZaloLog(Guid id) : base(id)
    {
    }
}