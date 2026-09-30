using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlBlouse;

/// <summary>
/// Đơn "Đăng ký nhận áo Blouse" — tạo từ Zalo Mini App.
/// Aggregate root chứa thông tin người đăng ký, thông tin in trên áo, tổng hợp và danh sách dòng áo (Items).
/// </summary>
[Table("AppHlBlouseRegistrations", Schema = "HL")]
public class HlBlouseRegistration : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Mã đăng ký (format: HLBL-{yyMMdd}{seq})</summary>
    [Required]
    [StringLength(50)]
    public string RegistrationCode { get; set; } = null!;

    // ===== Thông tin người đăng ký (auto-fill từ B2B profile) =====

    /// <summary>Mã khách hàng (Mã KH — VD: HL-12345)</summary>
    [StringLength(50)]
    public string? CustomerCode { get; set; }

    /// <summary>Tên khách hàng / Nhà thuốc</summary>
    [StringLength(250)]
    public string? CustomerName { get; set; }

    /// <summary>Số điện thoại người đăng ký</summary>
    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    /// <summary>ZaloUserId — định danh người dùng Mini App</summary>
    [StringLength(100)]
    public string? ZaloUserId { get; set; }

    /// <summary>Đại diện tiếp nhận (VD: DS. Nguyễn Hoàng Nam)</summary>
    [StringLength(150)]
    public string? ReceiverName { get; set; }

    /// <summary>Địa chỉ giao nhận quà</summary>
    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    // ===== Thông tin hiển thị trên áo =====

    /// <summary>Loại hình kinh doanh (dropdown)</summary>
    public HlBlouseBusinessType? BusinessType { get; set; }

    /// <summary>Tên loại hình kinh doanh (khi chọn "Khác")</summary>
    [StringLength(250)]
    public string? BusinessTypeName { get; set; }

    /// <summary>Tên cửa hàng (tên ngắn gọn)</summary>
    [StringLength(250)]
    public string? StoreName { get; set; }

    /// <summary>Tên sẽ in trên áo (VD: Nhà thuốc Minh An)</summary>
    [StringLength(250)]
    public string? PrintedName { get; set; }

    /// <summary>Ghi chú thêm (thời gian nhận hàng, yêu cầu thêu tên...)</summary>
    public string? Note { get; set; }

    // ===== Tổng hợp (tính từ Items) =====

    /// <summary>Số áo tặng miễn phí</summary>
    public int FreeQuantity { get; set; }

    /// <summary>Số áo đổi bằng điểm</summary>
    public int ExchangeQuantity { get; set; }

    /// <summary>Tổng số áo = FreeQuantity + ExchangeQuantity</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Tổng số điểm tích lũy đã dùng (chỉ tính dòng đổi điểm)</summary>
    public int TotalPointsUsed { get; set; }

    // ===== Xử lý =====

    /// <summary>Trạng thái xử lý đơn</summary>
    public HlBlouseRegistrationStatus Status { get; set; } = HlBlouseRegistrationStatus.Pending;

    /// <summary>Ghi chú nội bộ (lý do từ chối/hủy...)</summary>
    public string? InternalNote { get; set; }

    /// <summary>Người duyệt/xử lý</summary>
    public Guid? ProcessedBy { get; set; }

    /// <summary>Thời gian duyệt/xử lý</summary>
    public DateTime? ProcessedAt { get; set; }

    public virtual ICollection<HlBlouseRegistrationItem> Items { get; set; } = new List<HlBlouseRegistrationItem>();

    protected HlBlouseRegistration() { }

    public HlBlouseRegistration(Guid id, string registrationCode, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        RegistrationCode = registrationCode;
    }
}
