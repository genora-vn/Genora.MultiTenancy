using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppCustomerTypes;

public class CreateUpdateAppCustomerTypeDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    // VD: #FF9800
    [StringLength(7)]
    public string ColorCode { get; set; }

    public bool IsActive { get; set; } = true;
}