using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.Web.Pages.AppPaymentConfigurations;

/// <summary>Page-level ViewModel dùng trong CreateModal và EditModal.</summary>
public class CreateUpdatePaymentConfigurationPageModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "PaymentProviderName")]
    public string PaymentProviderName { get; set; } = null!;

    [StringLength(20)]
    [Display(Name = "BankBin")]
    public string? BankBin { get; set; }

    [StringLength(50)]
    [Display(Name = "AccountNumber")]
    public string? AccountNumber { get; set; }

    [StringLength(200)]
    [Display(Name = "AccountName")]
    public string? AccountName { get; set; }

    [StringLength(200)]
    [Display(Name = "MerchantId")]
    public string? MerchantId { get; set; }

    [StringLength(500)]
    [Display(Name = "ApiKey")]
    public string? ApiKey { get; set; }

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(500)]
    [Display(Name = "LogoUrl")]
    public string? LogoUrl { get; set; }

    [Display(Name = "IsActive")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "DisplayOrder")]
    public int DisplayOrder { get; set; } = 0;
}
