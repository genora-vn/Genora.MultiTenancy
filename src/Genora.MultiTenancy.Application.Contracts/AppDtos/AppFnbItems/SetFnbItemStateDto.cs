using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public class SetFnbItemStateDto
{
    public bool? IsActive { get; set; }
    public bool? IsAvailable { get; set; }
}