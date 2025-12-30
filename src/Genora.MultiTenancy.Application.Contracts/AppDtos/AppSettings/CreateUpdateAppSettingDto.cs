using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppSettings;

public class CreateUpdateAppSettingDto
{
    [Required]
    [StringLength(100)]
    public string SettingKey { get; set; }

    public string? SettingValue { get; set; }

    [Required]
    [StringLength(50)]
    public string SettingType { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsImageInput { get; set; } = false;
    public List<IRemoteStreamContent>? Images {  get; set; }
}