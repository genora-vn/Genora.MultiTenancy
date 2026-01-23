using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;

[Table("AppCalendarSlotPrices")]
public class CalendarSlotPrice : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CalendarSlotId { get; set; }
    public virtual CalendarSlot CalendarSlot { get; set; } = null!;

    public Guid CustomerTypeId { get; set; }
    public virtual CustomerType CustomerType { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price9 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price18 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price27 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price36 { get; set; }

    protected CalendarSlotPrice() { }

    public CalendarSlotPrice(
        Guid id,
        Guid calendarSlotId,
        Guid customerTypeId,
        decimal? price9,
        decimal price18,
        decimal? price27,
        decimal? price36
    ) : base(id)
    {
        CalendarSlotId = calendarSlotId;
        CustomerTypeId = customerTypeId;
        Price9 = price9;
        Price18 = price18;
        Price27 = price27;
        Price36 = price36;
    }
}