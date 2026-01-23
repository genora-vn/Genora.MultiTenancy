using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppMembershipTiers;

public class CreateUpdateAppMembershipTierDto
{
    [Required(ErrorMessage = "Validation:Required")]
    [StringLength(50, ErrorMessage = "Validation:StringMax")]
    [RegularExpression("^[A-Z0-9_-]+$", ErrorMessage = "Validation:CodeFormat")]

    public string Code { get; set; }          // GOLD / SILVER / BRONZE...

    [Required]
    [StringLength(100, ErrorMessage = "Validation:StringMax")]
    public string Name { get; set; }          // Vàng / Bạc / Đồng...

    [StringLength(500, ErrorMessage = "Validation:StringMax")]
    public string? Description { get; set; }

    public decimal? MinTotalSpending { get; set; }

    public int? MinRounds { get; set; }

    /// <summary>
    /// 0 = None, 1 = Rolling 12 months, 2 = Calendar year...
    /// </summary>
    public byte EvaluationPeriod { get; set; }

    public string? Benefits { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; } = 0;
}