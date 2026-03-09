using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppHomePageConfigs;

/// <summary>
/// Item cho widget dạng list (đặc biệt FeatureGrid)
/// </summary>
public class AppHomePageWidgetItem : AuditedAggregateRoot<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid AppHomePageWidgetId { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public string Text { get; set; } = default!;
    public string? Icon { get; set; }       // key từ icon library
    public string? Action { get; set; }     // deeplink / action key

    public bool IsEnabled { get; set; } = true;

    protected AppHomePageWidgetItem() { }

    public AppHomePageWidgetItem(Guid id, Guid widgetId, Guid? tenantId, string text, int displayOrder)
        : base(id)
    {
        AppHomePageWidgetId = widgetId;
        TenantId = tenantId;
        Text = text.Trim();
        DisplayOrder = displayOrder;
        IsEnabled = true;
    }
}