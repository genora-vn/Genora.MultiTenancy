using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppProOrderActivity;

[Table("AppProOrderActivity")]
public class ProOrderActivity : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid OrderId { get; set; }
    public string ActionType { get; set; } = default!; // Created, ServiceStatusChanged, Cancelled, PaymentStatusChanged
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime ActionTime { get; set; }
    public bool IsDanger { get; set; }

    protected ProOrderActivity() { }

    public ProOrderActivity(
        Guid id,
        Guid orderId,
        string actionType,
        string title,
        string? description,
        DateTime actionTime,
        bool isDanger = false,
        Guid? tenantId = null) : base(id)
    {
        OrderId = orderId;
        ActionType = actionType;
        Title = title;
        Description = description;
        ActionTime = actionTime;
        IsDanger = isDanger;
        TenantId = tenantId;
    }
}
