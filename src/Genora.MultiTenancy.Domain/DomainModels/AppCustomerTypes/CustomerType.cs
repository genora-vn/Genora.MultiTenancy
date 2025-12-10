using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCustomerTypes;

[Table("AppCustomerTypes")]
public class CustomerType : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "char(7)")]
    public string? ColorCode { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public virtual ICollection<CalendarSlotPrice> CalendarSlotPrices { get; set; } = new List<CalendarSlotPrice>();

    protected CustomerType() { }

    public CustomerType(Guid id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }
}