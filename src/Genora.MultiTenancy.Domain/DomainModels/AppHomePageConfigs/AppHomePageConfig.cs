using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppHomePageConfigs;
public class AppHomePageConfig : AuditedAggregateRoot<Guid>
{
    public Guid? TenantId { get; set; } // null = host/global template

    public string ThemeKey { get; set; } = "default";
    public bool IsActive { get; set; } = true;

    public ICollection<AppHomePageWidget> Widgets { get; set; } = new List<AppHomePageWidget>();

    protected AppHomePageConfig() { }

    public AppHomePageConfig(Guid id, Guid? tenantId, string themeKey = "default")
        : base(id)
    {
        TenantId = tenantId;
        ThemeKey = string.IsNullOrWhiteSpace(themeKey) ? "default" : themeKey.Trim();
        IsActive = true;
    }
}