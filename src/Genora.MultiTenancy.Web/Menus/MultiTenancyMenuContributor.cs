using Genora.MultiTenancy.Features;
using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.UI.Navigation;

namespace Genora.MultiTenancy.Web.Menus;

public class MultiTenancyMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    public async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main) return;

        var l = context.GetLocalizer<MultiTenancyResource>();
        var feature = context.ServiceProvider.GetRequiredService<IFeatureChecker>();
        var perms = context.ServiceProvider.GetRequiredService<IPermissionChecker>();
        var tenant = context.ServiceProvider.GetRequiredService<ICurrentTenant>();

        // Lấy nhóm "Quản trị" (Administration) sẵn có của ABP
        var administration = context.Menu.GetAdministration();

        // Home
        context.Menu.AddItem(
            new ApplicationMenuItem(
                MultiTenancyMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );

        if (tenant.IsAvailable) // TENANT
        {
            if (await feature.IsEnabledAsync(AppSettingFeatures.Management) && await perms.IsGrantedAsync(MultiTenancyPermissions.AppSettings.Default))
            {
                context.Menu.AddItem(
                    new ApplicationMenuItem("MiniAppSetting", l["Menu:MiniAppSetting"], icon: "fa fa-mobile")
                        .AddItem(
                            new ApplicationMenuItem("AppSettings", l["Menu:AppSettings"], "/AppSettings")
                                .RequirePermissions(MultiTenancyPermissions.AppSettings.Default)
                        )
                );
            }
        }
        else // HOST
        {
            administration.AddItem(
                    new ApplicationMenuItem(
                        name: "AuditLogs",
                        displayName: l["Menu:AuditLogs"],
                        url: "/Admin/AuditLogs",
                        icon: "fa fa-clipboard-list",
                        order: 60
                    ).RequirePermissions(AuditLogPermissions.View)
                );

            if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppSettings.Default))
            {
                context.Menu.AddItem(
                    new ApplicationMenuItem("AppSettingsHost", l["Menu:MiniAppSetting"], icon: "fa fa-mobile")
                        .AddItem(
                            new ApplicationMenuItem(
                                name: "AuditLogs",
                                displayName: l["Menu:AppSettings"],
                                url: "/AppSettings",
                                icon: "fa fa-cogs",
                                order: 1
                                ).RequirePermissions(MultiTenancyPermissions.HostAppSettings.Default)
                        )
                );
            }
        }
    }
}
