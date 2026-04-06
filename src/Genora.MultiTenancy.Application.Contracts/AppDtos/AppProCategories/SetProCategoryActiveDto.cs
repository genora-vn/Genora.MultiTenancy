using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppProCategories;

public class SetProCategoryActiveDto
{
    [Required]
    public bool IsActive { get; set; }
}
