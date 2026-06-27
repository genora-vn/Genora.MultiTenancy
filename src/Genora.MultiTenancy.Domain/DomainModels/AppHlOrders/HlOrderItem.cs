using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlOrders;

/// <summary>
/// Chi tiết sản phẩm trong đơn hàng Hoa Linh
/// </summary>
[Table("AppHlOrderItems", Schema = "HL")]
public class HlOrderItem : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    public Guid OrderId { get; set; }
    public virtual HlOrder Order { get; set; } = null!;

    /// <summary>Mã sản phẩm trên DMS Hoa Linh</summary>
    [Required]
    [StringLength(50)]
    public string ProductCode { get; set; } = null!;

    /// <summary>Tên sản phẩm</summary>
    [Required]
    [StringLength(500)]
    public string ProductName { get; set; } = null!;

    /// <summary>Mã nhóm sản phẩm</summary>
    [StringLength(50)]
    public string? ProductGroupCode { get; set; }

    /// <summary>Tên nhóm sản phẩm</summary>
    [StringLength(250)]
    public string? ProductGroupName { get; set; }

    /// <summary>Thương hiệu</summary>
    [StringLength(150)]
    public string? BrandName { get; set; }

    /// <summary>Đơn vị tính (Hộp, Cây, Chai...)</summary>
    [StringLength(50)]
    public string? ProductUnit { get; set; }

    /// <summary>Hình ảnh sản phẩm (URL)</summary>
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>Đơn giá</summary>
    public decimal Price { get; set; }

    /// <summary>Giá gốc (trước giảm)</summary>
    public decimal? OriginalPrice { get; set; }

    /// <summary>Số lượng</summary>
    public int Quantity { get; set; }

    /// <summary>Thành tiền = Price * Quantity</summary>
    public decimal Amount { get; set; }

    /// <summary>Ghi chú sản phẩm</summary>
    public string? Note { get; set; }

    protected HlOrderItem() { }

    public HlOrderItem(Guid id, Guid orderId, string productCode, string productName, decimal price, int quantity) : base(id)
    {
        OrderId = orderId;
        ProductCode = productCode;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
        Amount = price * quantity;
    }
}
