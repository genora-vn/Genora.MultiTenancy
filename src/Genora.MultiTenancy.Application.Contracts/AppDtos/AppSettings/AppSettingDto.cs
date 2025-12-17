using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppSettings;

public class AppSettingDto : AuditedEntityDto<Guid>
{
    public string SettingKey { get; set; }
    public string SettingValue { get; set; }
    public string SettingType { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}