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

    public decimal Price { get; set; }

    protected CalendarSlotPrice() { }

    public CalendarSlotPrice(Guid id, Guid calendarSlotId, Guid customerTypeId, decimal price) : base(id)
    {
        CalendarSlotId = calendarSlotId;
        CustomerTypeId = customerTypeId;
        Price = price;
    }
}