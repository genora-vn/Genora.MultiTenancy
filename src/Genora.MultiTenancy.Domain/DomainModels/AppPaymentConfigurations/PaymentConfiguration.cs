using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppPaymentConfigurations;

[Table("AppPaymentConfigurations")]
public class PaymentConfiguration : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(100)]
    public string PaymentProviderName { get; set; } = null!; // Ví dụ: VietQR, PayOS, Momo

    [StringLength(20)]
    public string? BankBin { get; set; }       // Mã BIN ngân hàng (VietQR)

    [StringLength(50)]
    public string? AccountNumber { get; set; } // Số tài khoản

    [StringLength(200)]
    public string? AccountName { get; set; }   // Tên chủ tài khoản

    [StringLength(200)]
    public string? MerchantId { get; set; }    // Dùng cho Mini App / cổng thanh toán

    [StringLength(500)]
    public string? ApiKey { get; set; }        // Khóa bảo mật

    [StringLength(500)]
    public string? Description { get; set; }   // Mô tả hiển thị cho người dùng

    [StringLength(500)]
    public string? LogoUrl { get; set; }       // Icon phương thức thanh toán

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; } = 0;

    protected PaymentConfiguration() { }

    public PaymentConfiguration(Guid id, string paymentProviderName) : base(id)
    {
        PaymentProviderName = paymentProviderName;
    }
}
