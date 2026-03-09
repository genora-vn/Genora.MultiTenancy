using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Genora.MultiTenancy.DomainModels.AppHomePageConfigs;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppHomePageConfigs;

[AllowAnonymous]
public class MiniAppHomePageConfigService : ApplicationService, IMiniAppHomePageConfigService
{
    private readonly IRepository<AppHomePageConfig, Guid> _configRepo;
    private readonly IRepository<AppHomePageWidget, Guid> _widgetRepo;
    private readonly IRepository<AppHomePageWidgetItem, Guid> _itemRepo;
    private readonly ICurrentTenant _currentTenant;

    public MiniAppHomePageConfigService(
        IRepository<AppHomePageConfig, Guid> configRepo,
        IRepository<AppHomePageWidget, Guid> widgetRepo,
        IRepository<AppHomePageWidgetItem, Guid> itemRepo,
        ICurrentTenant currentTenant)
    {
        _configRepo = configRepo;
        _widgetRepo = widgetRepo;
        _itemRepo = itemRepo;
        _currentTenant = currentTenant;
    }

    private async Task<AppHomePageConfig> EnsureConfigAsync()
    {
        var tenantId = _currentTenant.Id;

        var cfg = await _configRepo.FirstOrDefaultAsync(x => x.TenantId == tenantId);
        if (cfg != null) return cfg;

        // Clone từ host nếu tenant chưa có
        if (tenantId.HasValue)
        {
            var hostCfg = await _configRepo.FirstOrDefaultAsync(x => x.TenantId == null)
                          ?? await _configRepo.InsertAsync(new AppHomePageConfig(GuidGenerator.Create(), null, "default"), autoSave: true);

            cfg = new AppHomePageConfig(GuidGenerator.Create(), tenantId, hostCfg.ThemeKey);
            await _configRepo.InsertAsync(cfg, autoSave: true);

            var hostWidgets = await _widgetRepo.GetListAsync(w => w.AppHomePageConfigId == hostCfg.Id);
            foreach (var hw in hostWidgets.OrderBy(x => x.DisplayOrder))
            {
                var w = new AppHomePageWidget(GuidGenerator.Create(), cfg.Id, tenantId, hw.WidgetKey, hw.ModuleKey, hw.DisplayOrder)
                {
                    IsEnabled = hw.IsEnabled,
                    Title = hw.Title,
                    Limit = hw.Limit,
                    ConfigJson = hw.ConfigJson
                };
                await _widgetRepo.InsertAsync(w, autoSave: true);

                var hostItems = await _itemRepo.GetListAsync(i => i.AppHomePageWidgetId == hw.Id);
                foreach (var hi in hostItems.OrderBy(x => x.DisplayOrder))
                {
                    await _itemRepo.InsertAsync(new AppHomePageWidgetItem(GuidGenerator.Create(), w.Id, tenantId, hi.Text, hi.DisplayOrder)
                    {
                        Icon = hi.Icon,
                        Action = hi.Action,
                        IsEnabled = hi.IsEnabled
                    }, autoSave: true);
                }
            }

            return cfg;
        }

        cfg = new AppHomePageConfig(GuidGenerator.Create(), null, "default");
        await _configRepo.InsertAsync(cfg, autoSave: true);
        return cfg;
    }

    public async Task<MiniAppHomePageConfigDto> GetHomePageConfigAsync()
    {
        var cfg = await EnsureConfigAsync();

        var widgets = await _widgetRepo.GetListAsync(x => x.AppHomePageConfigId == cfg.Id);

        return new MiniAppHomePageConfigDto
        {
            ThemeKey = cfg.ThemeKey,
            Widgets = widgets
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new MiniAppHomePageWidgetDto
                {
                    Id = x.Id,
                    WidgetKey = x.WidgetKey,
                    ModuleKey = x.ModuleKey,
                    IsEnabled = x.IsEnabled,
                    DisplayOrder = x.DisplayOrder,
                    Title = x.Title,
                    Limit = x.Limit,
                    ConfigJson = x.ConfigJson
                })
                .ToList()
        };
    }

    public async Task<FeatureGridDto> GetFeatureGridAsync(Guid widgetId)
    {
        var cfg = await EnsureConfigAsync();

        var w = await _widgetRepo.GetAsync(widgetId);
        if (w.AppHomePageConfigId != cfg.Id) return new FeatureGridDto();

        var items = await _itemRepo.GetListAsync(x => x.AppHomePageWidgetId == widgetId);

        return new FeatureGridDto
        {
            Title = w.Title,
            Limit = w.Limit,
            Items = items.OrderBy(x => x.DisplayOrder)
                .Select(x => new HomePageWidgetItemDto
                {
                    Id = x.Id,
                    DisplayOrder = x.DisplayOrder,
                    Text = x.Text,
                    Icon = x.Icon,
                    Action = x.Action,
                    IsEnabled = x.IsEnabled
                }).ToList()
        };
    }
}