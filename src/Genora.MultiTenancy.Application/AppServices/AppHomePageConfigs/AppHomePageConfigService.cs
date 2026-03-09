using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Genora.MultiTenancy.DomainModels.AppHomePageConfigs;
using Genora.MultiTenancy.Features.AppHomePages;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppHomePageConfigs;

[Authorize]
public class AppHomePageConfigService
    : FeatureProtectedCrudAppService<
        AppHomePageWidget,
        HomePageWidgetDto,
        Guid,
        GetHomePageWidgetListInput,
        UpdateHomePageWidgetDto>,
      IAppHomePageConfigService
{
    protected override string FeatureName => AppHomePageFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHomePageConfigs.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHomePageConfigs.Default;

    private readonly IRepository<AppHomePageConfig, Guid> _configRepo;
    private readonly IRepository<AppHomePageWidgetItem, Guid> _itemRepo;

    public AppHomePageConfigService(
        IRepository<AppHomePageWidget, Guid> widgetRepo,
        IRepository<AppHomePageConfig, Guid> configRepo,
        IRepository<AppHomePageWidgetItem, Guid> itemRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(widgetRepo, currentTenant, featureChecker)
    {
        _configRepo = configRepo;
        _itemRepo = itemRepo;

        // Policy đúng key
        GetPolicyName = MultiTenancyPermissions.AppHomePageConfigs.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHomePageConfigs.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHomePageConfigs.Edit;
        UpdatePolicyName = MultiTenancyPermissions.AppHomePageConfigs.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHomePageConfigs.Edit;
    }

    // =========================
    // Ensure config (Host default + tenant clone)
    // =========================
    private async Task<AppHomePageConfig> EnsureConfigAsync()
    {
        var tenantId = CurrentTenant.Id; // null => host

        var cfg = await _configRepo.FirstOrDefaultAsync(x => x.TenantId == tenantId);
        if (cfg != null) return cfg;

        // tenant chưa có -> clone host
        if (tenantId.HasValue)
        {
            var hostCfg = await _configRepo.FirstOrDefaultAsync(x => x.TenantId == null);
            if (hostCfg == null)
            {
                hostCfg = new AppHomePageConfig(GuidGenerator.Create(), null, themeKey: "default");
                await _configRepo.InsertAsync(hostCfg, autoSave: true);
            }

            cfg = new AppHomePageConfig(GuidGenerator.Create(), tenantId, hostCfg.ThemeKey);
            await _configRepo.InsertAsync(cfg, autoSave: true);

            var hostWidgets = await Repository.GetListAsync(w => w.AppHomePageConfigId == hostCfg.Id);

            foreach (var hw in hostWidgets.OrderBy(x => x.DisplayOrder))
            {
                var w = new AppHomePageWidget(GuidGenerator.Create(), cfg.Id, tenantId, hw.WidgetKey, hw.ModuleKey, hw.DisplayOrder)
                {
                    IsEnabled = hw.IsEnabled,
                    Title = hw.Title,
                    Limit = hw.Limit,
                    ConfigJson = hw.ConfigJson
                };
                await Repository.InsertAsync(w, autoSave: true);

                var hostItems = await _itemRepo.GetListAsync(i => i.AppHomePageWidgetId == hw.Id);
                foreach (var hi in hostItems.OrderBy(x => x.DisplayOrder))
                {
                    var it = new AppHomePageWidgetItem(GuidGenerator.Create(), w.Id, tenantId, hi.Text, hi.DisplayOrder)
                    {
                        Icon = hi.Icon,
                        Action = hi.Action,
                        IsEnabled = hi.IsEnabled
                    };
                    await _itemRepo.InsertAsync(it, autoSave: true);
                }
            }

            return cfg;
        }

        // Nếu host chưa có -> tạo mới
        cfg = new AppHomePageConfig(GuidGenerator.Create(), null, themeKey: "default");
        await _configRepo.InsertAsync(cfg, autoSave: true);
        return cfg;
    }

    // =========================
    // Proxy methods for Admin UI
    // =========================
    public async Task<HomePageWidgetDto> GetWidgetAsync(Guid id)
        => await GetAsync(id);

    public async Task<HomePageWidgetDto> UpdateWidgetByIdAsync(Guid id, UpdateHomePageWidgetDto input)
        => await UpdateAsync(id, input);

    public async Task<PagedResultDto<HomePageWidgetListItemDto>> GetWidgetListAsync(GetHomePageWidgetListInput input)
    {
        await CheckGetListPolicyAsync();

        var cfg = await EnsureConfigAsync();

        var q = await Repository.GetQueryableAsync();
        q = q.Where(x => x.AppHomePageConfigId == cfg.Id);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var f = input.Filter.Trim();
            q = q.Where(x => x.WidgetKey.Contains(f)
                          || x.ModuleKey.Contains(f)
                          || (x.Title != null && x.Title.Contains(f)));
        }

        var total = await AsyncExecuter.CountAsync(q);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(AppHomePageWidget.DisplayOrder) + " asc"
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            q.OrderBy(sorting)
             .Skip(input.SkipCount)
             .Take(input.MaxResultCount)
        );

        return new PagedResultDto<HomePageWidgetListItemDto>(
            total,
            ObjectMapper.Map<List<AppHomePageWidget>, List<HomePageWidgetListItemDto>>(items)
        );
    }

    public async Task UpdateWidgetAsync(UpdateWidgetRequestDto input)
    {
        await CheckUpdatePolicyAsync();

        var cfg = await EnsureConfigAsync();

        var w = await Repository.GetAsync(input.Id);
        if (w.AppHomePageConfigId != cfg.Id)
            throw new BusinessException("AppHomePageConfig:WidgetNotInScope");

        if (input.IsEnabled.HasValue) w.IsEnabled = input.IsEnabled.Value;
        if (input.DisplayOrder.HasValue) w.DisplayOrder = input.DisplayOrder.Value;

        if (input.ModuleKey != null) w.ModuleKey = input.ModuleKey.Trim();
        if (input.Title != null) w.Title = string.IsNullOrWhiteSpace(input.Title) ? null : input.Title.Trim();
        if (input.Limit.HasValue) w.Limit = input.Limit.Value;
        if (input.ConfigJson != null) w.ConfigJson = string.IsNullOrWhiteSpace(input.ConfigJson) ? null : input.ConfigJson;

        await Repository.UpdateAsync(w, autoSave: true);
    }

    public async Task UpdateWidgetOrderAsync(UpdateWidgetOrderDto input)
    {
        await CheckUpdatePolicyAsync();

        var cfg = await EnsureConfigAsync();

        if (input.OrderedIds == null || input.OrderedIds.Count == 0)
            return;

        var widgets = await Repository.GetListAsync(x => x.AppHomePageConfigId == cfg.Id);
        var map = widgets.ToDictionary(x => x.Id, x => x);

        for (var i = 0; i < input.OrderedIds.Count; i++)
        {
            var id = input.OrderedIds[i];
            if (!map.TryGetValue(id, out var w)) continue;
            w.DisplayOrder = (i + 1) * 10;
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public async Task<FeatureGridDto> GetFeatureGridAsync(Guid widgetId)
    {
        await CheckGetPolicyAsync();

        var cfg = await EnsureConfigAsync();

        var w = await Repository.GetAsync(widgetId);
        if (w.AppHomePageConfigId != cfg.Id)
            throw new BusinessException("AppHomePageConfig:WidgetNotInScope");

        if (!string.Equals(w.WidgetKey, "FeatureGrid", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("AppHomePageConfig:NotFeatureGrid");

        var items = await _itemRepo.GetListAsync(x => x.AppHomePageWidgetId == widgetId);

        return new FeatureGridDto
        {
            Title = w.Title,
            Limit = w.Limit,
            Items = items
                .OrderBy(x => x.DisplayOrder)
                .Select(ObjectMapper.Map<AppHomePageWidgetItem, HomePageWidgetItemDto>)
                .ToList()
        };
    }

    public async Task UpdateFeatureGridAsync(Guid widgetId, UpdateFeatureGridDto input)
    {
        await CheckUpdatePolicyAsync();

        var cfg = await EnsureConfigAsync();

        var w = await Repository.GetAsync(widgetId);
        if (w.AppHomePageConfigId != cfg.Id)
            throw new BusinessException("AppHomePageConfig:WidgetNotInScope");

        if (!string.Equals(w.WidgetKey, "FeatureGrid", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("AppHomePageConfig:NotFeatureGrid");

        w.Title = string.IsNullOrWhiteSpace(input.Title) ? null : input.Title.Trim();
        w.Limit = input.Limit;
        await Repository.UpdateAsync(w, autoSave: true);

        var tenantId = CurrentTenant.Id;
        var existing = await _itemRepo.GetListAsync(x => x.AppHomePageWidgetId == widgetId);
        var map = existing.ToDictionary(x => x.Id, x => x);

        var keep = new HashSet<Guid>();

        foreach (var it in input.Items ?? new List<UpdateFeatureGridItemDto>())
        {
            var text = (it.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;

            if (it.Id == Guid.Empty || !map.TryGetValue(it.Id, out var entity))
            {
                var created = new AppHomePageWidgetItem(GuidGenerator.Create(), widgetId, tenantId, text, it.DisplayOrder)
                {
                    Icon = string.IsNullOrWhiteSpace(it.Icon) ? null : it.Icon.Trim(),
                    Action = string.IsNullOrWhiteSpace(it.Action) ? null : it.Action.Trim(),
                    IsEnabled = it.IsEnabled
                };
                await _itemRepo.InsertAsync(created, autoSave: true);
                keep.Add(created.Id);
            }
            else
            {
                entity.DisplayOrder = it.DisplayOrder;
                entity.Text = text;
                entity.Icon = string.IsNullOrWhiteSpace(it.Icon) ? null : it.Icon.Trim();
                entity.Action = string.IsNullOrWhiteSpace(it.Action) ? null : it.Action.Trim();
                entity.IsEnabled = it.IsEnabled;

                await _itemRepo.UpdateAsync(entity, autoSave: true);
                keep.Add(entity.Id);
            }
        }

        var toDelete = existing.Where(x => !keep.Contains(x.Id)).ToList();
        if (toDelete.Count > 0)
            await _itemRepo.DeleteManyAsync(toDelete, autoSave: true);
    }

    public override async Task<HomePageWidgetDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var cfg = await EnsureConfigAsync();

        var w = await Repository.GetAsync(id);
        if (w.AppHomePageConfigId != cfg.Id)
            throw new BusinessException("AppHomePageConfig:WidgetNotInScope");

        var dto = ObjectMapper.Map<AppHomePageWidget, HomePageWidgetDto>(w);

        var items = await _itemRepo.GetListAsync(x => x.AppHomePageWidgetId == id);
        dto.Items = items
            .OrderBy(x => x.DisplayOrder)
            .Select(ObjectMapper.Map<AppHomePageWidgetItem, HomePageWidgetItemDto>)
            .ToList();

        return dto;
    }

    public override async Task<HomePageWidgetDto> UpdateAsync(Guid id, UpdateHomePageWidgetDto input)
    {
        await CheckUpdatePolicyAsync();

        var cfg = await EnsureConfigAsync();

        var w = await Repository.GetAsync(id);
        if (w.AppHomePageConfigId != cfg.Id)
            throw new BusinessException("AppHomePageConfig:WidgetNotInScope");

        w.ModuleKey = string.IsNullOrWhiteSpace(input.ModuleKey) ? w.ModuleKey : input.ModuleKey.Trim();
        w.IsEnabled = input.IsEnabled;
        w.Title = string.IsNullOrWhiteSpace(input.Title) ? null : input.Title.Trim();
        w.Limit = input.Limit;
        w.ConfigJson = string.IsNullOrWhiteSpace(input.ConfigJson) ? null : input.ConfigJson;

        await Repository.UpdateAsync(w, autoSave: true);
        return ObjectMapper.Map<AppHomePageWidget, HomePageWidgetDto>(w);
    }
}