using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppPaymentConfigurations;

public class CreateUpdatePaymentConfigurationDto
{
    [Required]
    [StringLength(100)]
    public string PaymentProviderName { get; set; } = null!;

    [StringLength(20)]
    public string? BankBin { get; set; }

    [StringLength(50)]
    public string? AccountNumber { get; set; }

    [StringLength(200)]
    public string? AccountName { get; set; }

    [StringLength(200)]
    public string? MerchantId { get; set; }

    [StringLength(500)]
    public string? ApiKey { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; } = 0;
}
