using Genora.MultiTenancy.Features;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppGolfCourses;
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

        if (tenant.IsAvailable) // ===== TENANT =====
        {
            // Kiểm tra từng feature + permission
            var canSeeAppSettings =
                await feature.IsEnabledAsync(AppSettingFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppSettings.Default);

            var canSeeCustomerTypes =
                await feature.IsEnabledAsync(AppCustomerTypeFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppCustomerTypes.Default);

            var canSeeGolfCourses =
                await feature.IsEnabledAsync(AppGolfCourseFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppGolfCourses.Default);

            // Nếu không có quyền gì thì khỏi add menu
            if (canSeeAppSettings || canSeeCustomerTypes || canSeeGolfCourses)
            {
                var miniAppMenu = new ApplicationMenuItem(
                    "MiniAppSetting",
                    l["Menu:MiniAppSetting"],
                    icon: "fa fa-mobile",
                    order: 20
                );

                if (canSeeAppSettings)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            "AppSettings",
                            l["Menu:AppSettings"],
                            "/AppSettings"
                        ).RequirePermissions(MultiTenancyPermissions.AppSettings.Default)
                    );
                }

                if (canSeeCustomerTypes)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            "AppCustomerTypes",
                            l["Menu:AppCustomerTypes"],
                            "/AppCustomerTypes"
                        ).RequirePermissions(MultiTenancyPermissions.AppCustomerTypes.Default)
                    );
                }

                if (canSeeGolfCourses)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            "AppGolfCourses",
                            l["Menu:AppGolfCourses"],
                            "/AppGolfCourses"
                        ).RequirePermissions(MultiTenancyPermissions.AppGolfCourses.Default)
                    );
                }

                context.Menu.AddItem(miniAppMenu);
            }
        }
        else // ===== HOST =====
        {
            // Audit logs
            administration.AddItem(
                new ApplicationMenuItem(
                    name: "AuditLogs",
                    displayName: l["Menu:AuditLogs"],
                    url: "/Admin/AuditLogs",
                    icon: "fa fa-clipboard-list",
                    order: 60
                ).RequirePermissions(AuditLogPermissions.View)
            );

            var hostCanAppSettings =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppSettings.Default);

            var hostCanCustomerTypes =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppCustomerTypes.Default);

            var hostCanGolfCourses =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppGolfCourses.Default);

            if (hostCanAppSettings || hostCanCustomerTypes || hostCanGolfCourses)
            {
                var hostMiniAppMenu = new ApplicationMenuItem(
                    "MiniAppSettingHost",
                    l["Menu:MiniAppSetting"],
                    icon: "fa fa-mobile",
                    order: 20
                );

                if (hostCanAppSettings)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppSettingsHost",
                            displayName: l["Menu:AppSettings"],
                            url: "/AppSettings",
                            icon: "fa fa-cogs"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppSettings.Default)
                    );
                }

                if (hostCanCustomerTypes)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppCustomerTypesHost",
                            displayName: l["Menu:AppCustomerTypes"],
                            url: "/AppCustomerTypes",
                            icon: "fa fa-users"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppCustomerTypes.Default)
                    );
                }

                if (hostCanGolfCourses)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppGolfCoursesHost",
                            displayName: l["Menu:AppGolfCourses"],
                            url: "/AppGolfCourses",
                            icon: "fa fa-flag"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppGolfCourses.Default)
                    );
                }

                context.Menu.AddItem(hostMiniAppMenu);
            }
        }
    }
}
