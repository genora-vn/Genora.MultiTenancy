using Genora.MultiTenancy.DomainModels.AppFnbItems;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace Genora.MultiTenancy.DomainModels.AppFnbOrders;

[Table("AppFnbOrderItems")]
public class FnbOrderItem : Entity<Guid>
{
    [Required]
    public Guid OrderId { get; set; }
    public virtual FnbOrder Order { get; set; } = null!;

    public Guid? ItemId { get; set; }
    public virtual FnbItem? Item { get; set; }

    [Required]
    [StringLength(255)]
    public string ItemName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }

    protected FnbOrderItem() { }

    public FnbOrderItem(Guid id, Guid orderId, string itemName, decimal price, int quantity) : base(id)
    {
        OrderId = orderId;
        ItemName = itemName;
        Price = price;
        Quantity = quantity;
    }
}