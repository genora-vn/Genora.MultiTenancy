using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Features.AppCalendarSlots;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppGolfCourses;
using Genora.MultiTenancy.Features.AppMembershipTiers;
using Genora.MultiTenancy.Features.AppNewsFeatures;
using Genora.MultiTenancy.Features.AppPromotionTypes;
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

            var canSeeMembershipTiers =
                await feature.IsEnabledAsync(AppMembershipTierFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppMembershipTiers.Default);

            var canSeeCustomers =
                await feature.IsEnabledAsync(AppCustomerFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppCustomers.Default);

            var canSeeCalendarSlots =
                await feature.IsEnabledAsync(AppCalendarSlotFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppCalendarSlots.Default);

            var canSeeNews =
               await feature.IsEnabledAsync(AppNewsFeatures.Management) &&
               await perms.IsGrantedAsync(MultiTenancyPermissions.AppNews.Default);

            var canSeeBookings =
              await feature.IsEnabledAsync(AppBookingFeatures.Management) &&
              await perms.IsGrantedAsync(MultiTenancyPermissions.AppBookings.Default);
            var canSeePromotionType = await feature.IsEnabledAsync(AppPromotionTypeFeature.Management) && await perms.IsGrantedAsync(MultiTenancyPermissions.AppPromotionType.Default);
            // Nếu không có quyền gì thì khỏi add menu
            if (canSeeAppSettings || canSeeCustomerTypes || canSeeGolfCourses || canSeeMembershipTiers || canSeeCustomers || canSeeCalendarSlots || canSeeNews || canSeeBookings || canSeePromotionType)
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
                            name: "AppSettings",
                            displayName: l["Menu:AppSettings"],
                            url: "/AppSettings",
                            icon: "fa fa-cogs"
                        ).RequirePermissions(MultiTenancyPermissions.AppSettings.Default)
                    );
                }

                if (canSeeCustomerTypes)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppCustomerTypes",
                            displayName: l["Menu:AppCustomerTypes"],
                            url: "/AppCustomerTypes",
                            icon: "fa fa-users"
                        )
                        .RequirePermissions(MultiTenancyPermissions.AppCustomerTypes.Default)
                    );
                }

                if (canSeeGolfCourses)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppGolfCourses",
                            displayName: l["Menu:AppGolfCourses"],
                            url: "/AppGolfCourses",
                            icon: "fa fa-flag"
                        )
                        .RequirePermissions(MultiTenancyPermissions.AppGolfCourses.Default)
                    );
                }

                if (canSeeMembershipTiers)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppMembershipTiers",
                            displayName: l["Menu:AppMembershipTiers"],
                            url: "/AppMembershipTiers",
                            icon: "fa fa-medal"
                        )
                        .RequirePermissions(MultiTenancyPermissions.AppMembershipTiers.Default)
                    );
                }

                if (canSeeCustomers)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppCustomers",
                            displayName: l["Menu:AppCustomers"],
                            url: "/AppCustomers",
                            icon: "fa fa-user"
                        ).RequirePermissions(MultiTenancyPermissions.AppCustomers.Default)
                    );
                }

                if (canSeeCalendarSlots)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                             name: "AppCalendarSlots",
                            displayName: l["Menu:AppCalendarSlots"],
                            url: "/AppCalendarSlots",
                            icon: "fa fa-calendar"
                        ).RequirePermissions(MultiTenancyPermissions.AppCalendarSlots.Default)
                    );
                }

                if (canSeeNews)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                             name: "AppNews",
                            displayName: l["Menu:AppNews"],
                            url: "/AppNews",
                            icon: "fa fa-newspaper-o"
                        ).RequirePermissions(MultiTenancyPermissions.AppNews.Default)
                    );
                }

                if (canSeeBookings)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            "AppBookings",
                            l["Menu:AppBookings"],
                            icon: "fa fa-calendar-plus-o",
                            url: "/AppBookings"
                        ).RequirePermissions(MultiTenancyPermissions.AppBookings.Default)
                    );
                }
                if (canSeePromotionType)
                {
                    miniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            "AppPromotionTypes",
                            l["Menu:AppPromotionTypes"],
                            icon: "fa fa-calendar-plus-o",
                            url: "/AppPromotionTypes"
                        ).RequirePermissions(MultiTenancyPermissions.AppPromotionType.Default)
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

            var hostCanMembershipTiers =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppMembershipTiers.Default);

            var hostCanCustomers =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppCustomers.Default);

            var hostCanCalendarSlots =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppCalendarSlots.Default);

            var hostCanNews =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppNews.Default);

            var hostCanBookings =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppBookings.Default);

            var hostCanZaloAuths =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppZaloAuths.Default);
            var hostCanZaloLogs =
               await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppZaloLogs.Default);
            var hostPromotionType = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppPromotionType.Default);
            if (hostCanAppSettings || hostCanCustomerTypes || hostCanGolfCourses || hostCanMembershipTiers || hostCanCustomers || hostCanCalendarSlots || hostCanNews || hostCanBookings || hostCanZaloAuths || hostCanZaloLogs || hostPromotionType)
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

                if (hostCanMembershipTiers)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppMembershipTiersHost",
                            displayName: l["Menu:AppMembershipTiers"],
                            url: "/AppMembershipTiers",
                            icon: "fa fa-medal"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppMembershipTiers.Default)
                    );
                }

                if (hostCanCustomers)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppCustomersHost",
                            displayName: l["Menu:AppCustomers"],
                            url: "/AppCustomers",
                            icon: "fa fa-user"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppCustomers.Default)
                    );
                }

                if (hostCanCalendarSlots)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppCalendarSlotsHost",
                            displayName: l["Menu:AppCalendarSlots"],
                            url: "/AppCalendarSlots",
                            icon: "fa fa-calendar"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppCalendarSlots.Default)
                    );
                }

                if (hostCanNews)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppNewsHost",
                            displayName: l["Menu:AppNews"],
                            url: "/AppNews",
                            icon: "fa fa-newspaper-o"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppNews.Default)
                    );
                }

                if (hostCanBookings)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppBookingsHost",
                            displayName: l["Menu:AppBookings"],
                            url: "/AppBookings",
                            icon: "fa fa-calendar-plus-o"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppBookings.Default)
                    );
                }

                if (hostCanZaloAuths || hostCanZaloLogs)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem("Zalo", l["Menu:AppZalo"], icon: "fa fa-shield-alt")
                            .AddItem(new ApplicationMenuItem("AppZaloAuths", l["Menu:AppZaloAuths"], "/AppZaloAuths")
                                .RequirePermissions(MultiTenancyPermissions.HostAppZaloAuths.Default))
                            .AddItem(new ApplicationMenuItem("AppZaloLogs", l["Menu:AppZaloLogs"], "/AppZaloLogs")
                                .RequirePermissions(MultiTenancyPermissions.HostAppZaloLogs.Default))
                    );
                }
                if (hostPromotionType)
                {
                    hostMiniAppMenu.AddItem(
                        new ApplicationMenuItem(
                            name: "AppPromotionTypeHost",
                            displayName: l["Menu:AppPromotionTypes"],
                            url: "/AppPromotionTypes",
                            icon: "fa fa-calendar-plus-o"
                        ).RequirePermissions(MultiTenancyPermissions.HostAppPromotionType.Default)
                    );
                }

                context.Menu.AddItem(hostMiniAppMenu);
            }
        }
    }
}
