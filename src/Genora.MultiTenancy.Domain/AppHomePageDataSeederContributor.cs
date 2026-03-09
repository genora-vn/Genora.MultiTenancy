using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.DomainModels.AppHomePageConfigs;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy;

public class AppHomePageDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AppHomePageConfig, Guid> _configRepo;
    private readonly IRepository<AppHomePageWidget, Guid> _widgetRepo;
    private readonly IRepository<AppHomePageWidgetItem, Guid> _itemRepo;

    public AppHomePageDataSeederContributor(
        IRepository<AppHomePageConfig, Guid> configRepo,
        IRepository<AppHomePageWidget, Guid> widgetRepo,
        IRepository<AppHomePageWidgetItem, Guid> itemRepo)
    {
        _configRepo = configRepo;
        _widgetRepo = widgetRepo;
        _itemRepo = itemRepo;
    }

    private sealed record WidgetSeedDef(
        string WidgetKey,
        string ModuleKey,
        bool IsEnabled,
        int DisplayOrder,
        string? Title = null,
        int? Limit = null
    );

    private sealed record FeatureGridItemSeedDef(
        string Text,
        string? Icon,
        string? Action,
        bool IsEnabled,
        int DisplayOrder
    );

    // Default widgets
    private static readonly List<WidgetSeedDef> DefaultWidgets = new()
    {
        // Fixed
        new WidgetSeedDef("Profile",      "Fixed",    true,  1),

        // Modules
        new WidgetSeedDef("FeatureGrid",  "Core",     true,  2,  "Tính năng"),
        new WidgetSeedDef("UpcomingRounds","GolfBooking", false, 3, "Lịch chơi sắp tới"),
        new WidgetSeedDef("HotDeals",     "GolfBooking", false, 4, "Ưu đãi hot"),
        new WidgetSeedDef("Weather7Days", "Free",     true,  5,  "Thời tiết 7 ngày"),
        new WidgetSeedDef("News",         "News",     true,  6,  "Tin tức", 6),
        new WidgetSeedDef("CourseReview", "GolfBooking", false, 7, "Đánh giá sân golf"),
        new WidgetSeedDef("CaddieReview", "Caddie",   false, 8, "Đánh giá caddie"),
        new WidgetSeedDef("ZaloFollow",   "ZaloOA",   true,  9, "Quan tâm Zalo OA"),
        new WidgetSeedDef("Gifts",        "Loyalty",  false, 10, "Quà tặng"),
        new WidgetSeedDef("FavoriteFoods","FnB",      false, 11, "Món ăn ưa thích"),
        new WidgetSeedDef("Banner",       "Free",     true,  12, "Banner"),
    };

    // Default FeatureGrid items
    private static readonly List<FeatureGridItemSeedDef> DefaultFeatureGridItems = new()
    {
        new FeatureGridItemSeedDef("Đặt sân",    "golf",     "booking", true, 1),
        new FeatureGridItemSeedDef("Tích điểm",  "loyalty",  "loyalty", true, 2),
        new FeatureGridItemSeedDef("Tin tức",    "news",     "news",    true, 3),
        new FeatureGridItemSeedDef("Đặt caddie", "caddie",   "caddie",  true, 4),
        new FeatureGridItemSeedDef("Đặt FnB",    "fnb",      "fnb",     true, 5),
    };

    public async Task SeedAsync(DataSeedContext context)
    {
        // context.TenantId: null => host seeding (trong DB hiện tại)
        // context.TenantId: != null => tenant seeding (trong DB hiện tại)
        var tenantId = context.TenantId;

        // 1) Ensure config
        var config = await _configRepo.FirstOrDefaultAsync(x => x.TenantId == tenantId);
        if (config == null)
        {
            config = new AppHomePageConfig(
                id: Guid.NewGuid(),
                tenantId: tenantId,
                themeKey: "default"
            )
            {
                IsActive = true
            };

            await _configRepo.InsertAsync(config, autoSave: true);
        }

        // 2) Ensure widgets
        var existingWidgets = await _widgetRepo.GetListAsync(x =>
            x.AppHomePageConfigId == config.Id
        );

        var widgetByKey = existingWidgets
            .GroupBy(w => w.WidgetKey)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var def in DefaultWidgets)
        {
            if (!widgetByKey.TryGetValue(def.WidgetKey, out var widget))
            {
                widget = new AppHomePageWidget(
                    id: Guid.NewGuid(),
                    configId: config.Id,
                    tenantId: tenantId,
                    widgetKey: def.WidgetKey,
                    moduleKey: def.ModuleKey,
                    displayOrder: def.DisplayOrder
                )
                {
                    IsEnabled = def.IsEnabled,
                    Title = def.Title,
                    Limit = def.Limit
                };

                await _widgetRepo.InsertAsync(widget, autoSave: true);
                widgetByKey[def.WidgetKey] = widget;
            }
        }

        // 3) Ensure FeatureGrid items
        if (widgetByKey.TryGetValue("FeatureGrid", out var featureGridWidget))
        {
            var hasAnyItem = await _itemRepo.AnyAsync(x => x.AppHomePageWidgetId == featureGridWidget.Id);
            if (!hasAnyItem)
            {
                var items = DefaultFeatureGridItems.Select(i =>
                    new AppHomePageWidgetItem(
                        id: Guid.NewGuid(),
                        widgetId: featureGridWidget.Id,
                        tenantId: tenantId,
                        text: i.Text,
                        displayOrder: i.DisplayOrder
                    )
                    {
                        Icon = i.Icon,
                        Action = i.Action,
                        IsEnabled = i.IsEnabled
                    }
                ).ToList();

                await _itemRepo.InsertManyAsync(items, autoSave: true);
            }
        }
    }
}