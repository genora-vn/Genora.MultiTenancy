using Genora.MultiTenancy.Books;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.Apps.AppSettings;

public class CreateUpdateAppSettingDto
{
    [Required]
    [StringLength(100)]
    public string SettingKey { get; set; }

    [Required]
    public string SettingValue { get; set; }

    [Required]
    [StringLength(50)]
    public string SettingType { get; set; }

    [StringLength(255)]
    public string Description { get; set; }
    public bool IsActive { get; set; }
}