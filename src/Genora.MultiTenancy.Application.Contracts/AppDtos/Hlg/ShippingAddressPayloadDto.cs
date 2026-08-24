namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Payload địa chỉ giao hàng. Khớp contract ShippingAddressPayload { receiverName, phone, address, note? }.</summary>
public class ShippingAddressPayloadDto
{
    public string? ReceiverName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
}
