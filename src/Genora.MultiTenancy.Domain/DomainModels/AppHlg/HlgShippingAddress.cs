using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Địa chỉ giao hàng cho luồng nhận quà vật lý của khách consumer (BD-6).
/// Khớp contract ShippingAddressPayload { receiverName, phone, address, note? }. Schema: HLG.
/// </summary>
[Table("AppHlgShippingAddresses", Schema = "HLG")]
public class HlgShippingAddress : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Người sở hữu địa chỉ (dbo.AppCustomers).</summary>
    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(150)]
    public string ReceiverName { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = null!;

    [StringLength(500)]
    public string? Note { get; set; }

    protected HlgShippingAddress() { }

    public HlgShippingAddress(Guid id, Guid customerId, string receiverName, string phone, string address, Guid? tenantId = null) : base(id)
    {
        CustomerId = customerId;
        ReceiverName = receiverName;
        Phone = phone;
        Address = address;
        TenantId = tenantId;
    }
}
