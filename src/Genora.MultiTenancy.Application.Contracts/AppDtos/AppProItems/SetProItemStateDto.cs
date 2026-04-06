using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppProItems;

public class SetProItemStateDto
{
    public bool? IsActive { get; set; }
    public bool? IsAvailable { get; set; }
}
