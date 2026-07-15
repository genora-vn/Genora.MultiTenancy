using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;

/// <summary>
/// Yêu cầu đổi quà Hoa Linh — tạo từ Mini App, lưu trên Genora, gọi UrBox API
/// </summary>
[Table("AppHlGiftExchanges", Schema = "HL")]
public class HlGiftExchange : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Mã yêu cầu đổi quà (format: HLGE-{yyMMdd}{seq})</summary>
    [Required]
    [StringLength(50)]
    public string ExchangeCode { get; set; } = null!;

    /// <summary>Mã khách hàng trên DMS Hoa Linh</summary>
    [StringLength(50)]
    public string? CustomerCode { get; set; }

    /// <summary>Tên khách hàng</summary>
    [StringLength(250)]
    public string? CustomerName { get; set; }

    /// <summary>SĐT khách hàng</summary>
    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    /// <summary>Tên quà tặng / Voucher</summary>
    [Required]
    [StringLength(500)]
    public string GiftName { get; set; } = null!;

    /// <summary>Mã quà / Voucher code trên UrBox</summary>
    [StringLength(100)]
    public string? GiftCode { get; set; }

    /// <summary>Hình ảnh quà tặng</summary>
    [StringLength(500)]
    public string? GiftImageUrl { get; set; }

    /// <summary>Số điểm cần đổi</summary>
    public int PointsRequired { get; set; }

    /// <summary>Số lượng đổi</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Tổng điểm sử dụng = PointsRequired * Quantity</summary>
    public int TotalPointsUsed { get; set; }

    /// <summary>Trạng thái xử lý</summary>
    public HlGiftExchangeStatus Status { get; set; } = HlGiftExchangeStatus.Processing;

    /// <summary>Ghi chú từ khách hàng</summary>
    public string? Note { get; set; }

    /// <summary>Ghi chú nội bộ (lý do từ chối, etc.)</summary>
    public string? InternalNote { get; set; }

    /// <summary>Người duyệt / từ chối</summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>Thời gian duyệt / từ chối</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Mã voucher UrBox trả về (sau khi đổi thành công)</summary>
    [StringLength(200)]
    public string? UrBoxVoucherCode { get; set; }

    /// <summary>Response từ UrBox API</summary>
    public string? UrBoxResponse { get; set; }

    /// <summary>Địa chỉ nhận quà (nếu quà vật lý)</summary>
    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    protected HlGiftExchange() { }

    public HlGiftExchange(Guid id, string exchangeCode, string giftName, int pointsRequired, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        ExchangeCode = exchangeCode;
        GiftName = giftName;
        PointsRequired = pointsRequired;
    }
}
