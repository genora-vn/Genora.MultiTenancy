using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Apps.AppSettings;

[Audited]
public class AppSetting : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string SettingKey { get; set; }
    public string SettingValue { get; set; }
    public string SettingType { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}
