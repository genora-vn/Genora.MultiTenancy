using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppPaymentConfigurations;

public class PaymentConfigurationDto : FullAuditedEntityDto<Guid>
{
    public string PaymentProviderName { get; set; } = null!;
    public string? BankBin { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? MerchantId { get; set; }
    public string? ApiKey { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}
