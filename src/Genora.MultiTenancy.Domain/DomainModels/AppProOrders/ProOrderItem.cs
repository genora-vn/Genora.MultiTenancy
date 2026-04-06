using Genora.MultiTenancy.DomainModels.AppProItems;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppProOrders;

[Table("AppProOrderItems")]
public class ProOrderItem : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    public Guid OrderId { get; set; }
    public virtual ProOrder Order { get; set; } = null!;

    public Guid? ItemId { get; set; }
    public virtual ProItem? Item { get; set; }

    [Required]
    [StringLength(255)]
    public string ItemName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }

    protected ProOrderItem() { }

    public ProOrderItem(Guid id, Guid orderId, string itemName, decimal price, int quantity) : base(id)
    {
        OrderId = orderId;
        ItemName = itemName;
        Price = price;
        Quantity = quantity;
    }
}
