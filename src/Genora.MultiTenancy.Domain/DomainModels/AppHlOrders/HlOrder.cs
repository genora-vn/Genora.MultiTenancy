using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlOrders;

/// <summary>
/// Đơn hàng Hoa Linh — tạo từ Mini App, lưu trên Genora, push sang DMS Hoa Linh
/// </summary>
[Table("AppHlOrders", Schema = "HL")]
public class HlOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Mã đơn hàng (format: HL-{yyMMdd}{seq})</summary>
    [Required]
    [StringLength(50)]
    public string OrderCode { get; set; } = null!;

    /// <summary>Mã khách hàng trên DMS Hoa Linh</summary>
    [StringLength(50)]
    public string? CustomerCode { get; set; }

    /// <summary>Tên khách hàng / Đại lý / Nhà thuốc</summary>
    [StringLength(250)]
    public string? CustomerName { get; set; }

    /// <summary>Số điện thoại khách hàng</summary>
    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    /// <summary>Mã chi nhánh nhận hàng trên DMS</summary>
    [StringLength(50)]
    public string? BranchCode { get; set; }

    /// <summary>Tên chi nhánh nhận hàng</summary>
    [StringLength(250)]
    public string? BranchName { get; set; }

    /// <summary>Địa chỉ giao hàng</summary>
    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    /// <summary>Người nhận hàng</summary>
    [StringLength(150)]
    public string? ReceiverName { get; set; }

    /// <summary>Mã trình dược viên (DSR) phụ trách — map dsrCode trên DMS Hoa Linh</summary>
    [StringLength(50)]
    public string? ReceiverCode { get; set; }

    /// <summary>SĐT người nhận</summary>
    [StringLength(20)]
    public string? ReceiverPhone { get; set; }

    /// <summary>Tạm tính (tổng giá sản phẩm trước discount)</summary>
    public decimal SubTotal { get; set; }

    /// <summary>Mã giảm giá (discount code)</summary>
    [StringLength(50)]
    public string? DiscountCode { get; set; }

    /// <summary>Số tiền giảm giá</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Chiết khấu hệ thống</summary>
    public decimal SystemDiscount { get; set; }

    /// <summary>Tổng thanh toán = SubTotal - DiscountAmount - SystemDiscount</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Trạng thái giao hàng</summary>
    public HlOrderDeliveryStatus DeliveryStatus { get; set; } = HlOrderDeliveryStatus.PendingConfirmation;

    /// <summary>Trạng thái thanh toán</summary>
    public HlOrderPaymentStatus PaymentStatus { get; set; } = HlOrderPaymentStatus.Unpaid;

    /// <summary>Phương thức thanh toán</summary>
    public HlPaymentMethod? PaymentMethod { get; set; }

    /// <summary>Ghi chú khách hàng</summary>
    public string? Note { get; set; }

    /// <summary>Ghi chú nội bộ</summary>
    public string? InternalNote { get; set; }

    /// <summary>Lý do hủy</summary>
    [StringLength(500)]
    public string? CancelNote { get; set; }

    /// <summary>Người hủy</summary>
    public Guid? CancelledBy { get; set; }

    /// <summary>Thời gian hủy</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Mã đơn hàng trên DMS Hoa Linh (sau khi push thành công)</summary>
    [StringLength(50)]
    public string? ExternalOrderCode { get; set; }

    /// <summary>Đã push sang DMS Hoa Linh chưa</summary>
    public bool IsSyncedToHl { get; set; } = false;

    /// <summary>Thời gian push sang DMS</summary>
    public DateTime? SyncedAt { get; set; }

    public virtual ICollection<HlOrderItem> Items { get; set; } = new List<HlOrderItem>();

    protected HlOrder() { }

    public HlOrder(Guid id, string orderCode, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        OrderCode = orderCode;
    }
}
