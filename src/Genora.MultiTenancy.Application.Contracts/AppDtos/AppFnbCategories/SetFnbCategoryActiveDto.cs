using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public class SetFnbCategoryActiveDto
{
    [Required]
    public bool IsActive { get; set; }
}