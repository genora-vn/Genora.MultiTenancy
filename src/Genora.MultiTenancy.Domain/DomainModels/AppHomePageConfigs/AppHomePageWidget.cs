using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppHomePageConfigs;
public class AppHomePageWidget : AuditedAggregateRoot<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid AppHomePageConfigId { get; set; }

    /// <summary>
    /// FE mini app map widget, ví dụ: "FeatureGrid", "UpcomingRounds", "Weather7Days", ...
    /// </summary>
    public string WidgetKey { get; set; } = default!;

    /// <summary>
    /// Module phụ thuộc. Ví dụ: "Core", "GolfBooking", "News", "ZaloOA", "Loyalty", "FnB", "Free"
    /// </summary>
    public string ModuleKey { get; set; } = "Free";

    public bool IsEnabled { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    public string? Title { get; set; }
    public int? Limit { get; set; }

    /// <summary>
    /// JSON mở rộng (cho widget khác). FeatureGrid sẽ dùng bảng Items riêng.
    /// </summary>
    public string? ConfigJson { get; set; }

    public ICollection<AppHomePageWidgetItem> Items { get; set; } = new List<AppHomePageWidgetItem>();

    protected AppHomePageWidget() { }

    public AppHomePageWidget(Guid id, Guid configId, Guid? tenantId, string widgetKey, string moduleKey, int displayOrder)
        : base(id)
    {
        AppHomePageConfigId = configId;
        TenantId = tenantId;

        WidgetKey = widgetKey.Trim();
        ModuleKey = string.IsNullOrWhiteSpace(moduleKey) ? "Free" : moduleKey.Trim();
        DisplayOrder = displayOrder;

        IsEnabled = true;
    }
}