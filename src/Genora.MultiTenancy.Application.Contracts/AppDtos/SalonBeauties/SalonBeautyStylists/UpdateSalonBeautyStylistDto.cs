using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;

public class UpdateSalonBeautyStylistDto
{
    [Required]
    [StringLength(255)]
    public string DisplayName { get; set; } = null!;

    [StringLength(500)]
    public string? Avatar { get; set; }

    public IRemoteStreamContent? Images { get; set; }

    public bool IsUploadImage { get; set; }

    [StringLength(15)]
    [RegularExpression(@"^$|^0\d{9,10}$")]
    public string? Phone { get; set; }

    public byte? Gender { get; set; }

    [Required]
    public byte? Role { get; set; }

    [Required]
    public byte? Level { get; set; }

    [Range(0, 50)]
    public int ExperienceYear { get; set; }

    public byte Status { get; set; }

    public bool IsShowOnApp { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}
