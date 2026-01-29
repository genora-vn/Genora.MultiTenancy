using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Features.AppCalendarSlots;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Features.AppGolfCourses;
using Genora.MultiTenancy.Features.AppMembershipTiers;
using Genora.MultiTenancy.Features.AppNewsFeatures;
using Genora.MultiTenancy.Features.AppPromotionTypes;
using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Features.AppSpecialDates;
using Genora.MultiTenancy.Features.AppZaloAuths;
using Genora.MultiTenancy.Features.AppZaloLogs;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
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

    private static void ApplyNativeTooltips(ApplicationMenuItem item)
    {
        if (!item.CustomData.ContainsKey("data-genora-tooltip"))
        {
            item.CustomData["data-genora-tooltip"] = item.DisplayName?.ToString();
        }

        foreach (var child in item.Items)
        {
            ApplyNativeTooltips(child);
        }
    }

    private static ApplicationMenuItem ComingSoon(
    string name,
    string displayName,
    string icon,
    int order = 0)
    {
        var item = new ApplicationMenuItem(
            name: name,
            displayName: displayName,
            url: "#",
            icon: icon,
            order: order
        );

        // UI disabled theo theme
        item.CssClass = "disabled genora-coming-soon";
        item.CustomData["aria-disabled"] = "true";
        item.CustomData["tabindex"] = "-1";
        item.Url = "#";

        // Tạo tooltip xem nội dung ẩn
        item.CustomData["data-genora-tooltip"] = "Chưa phát triển";

        return item;
    }

    public async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main) return;

        var l = context.GetLocalizer<MultiTenancyResource>();
        var feature = context.ServiceProvider.GetRequiredService<IFeatureChecker>();
        var perms = context.ServiceProvider.GetRequiredService<IPermissionChecker>();
        var tenant = context.ServiceProvider.GetRequiredService<ICurrentTenant>();

        // ===== Home =====
        context.Menu.AddItem(
            new ApplicationMenuItem(
                MultiTenancyMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );

        // ===== Permissions/Features (Tenant vs Host) =====
        if (tenant.IsAvailable) // ================= TENANT =================
        {
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

            var canSeePromotionType =
                await feature.IsEnabledAsync(AppPromotionTypeFeature.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppPromotionType.Default);

            var canSeeZaloAuths =
                await feature.IsEnabledAsync(AppZaloAuthFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppZaloAuths.Default);

            var canSeeZaloLogs =
                await feature.IsEnabledAsync(AppZaloLogFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppZaloLogs.Default);

            var canSeeSpecialDates =
                  await feature.IsEnabledAsync(AppSpecialDateFeatures.Management) &&
                  await perms.IsGrantedAsync(MultiTenancyPermissions.AppSpecialDates.Default);

            var canSeeEmails =
                await feature.IsEnabledAsync(AppEmailFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppEmails.Default);

            // =========================================================
            // 1) Cài đặt Mini App
            // =========================================================
            var groupMiniAppSetup = new ApplicationMenuItem(
                name: "MenuGroup.MiniAppSetup",
                displayName: l["MenuGroup:MiniAppSetup"],
                icon: "fa fa-sliders",
                order: 10
            );

            // Cấu hình Mini App (AppSettings)
            if (canSeeAppSettings)
            {
                groupMiniAppSetup.AddItem(
                    new ApplicationMenuItem(
                        name: "AppSettings",
                        displayName: l["Menu:AppSettings"],
                        url: "/AppSettings",
                        icon: "fa fa-cogs",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.AppSettings.Default)
                );
            }

            // Cấu hình Trang chủ (Coming soon)
            groupMiniAppSetup.AddItem(
                ComingSoon(
                    name: "HomePageConfig",
                    displayName: $"{l["Menu:HomePageConfig"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-th-large",
                    order: 2
                )
            );

            // Tích hợp Zalo OA (cấp 1) -> cấp 2: Xác thực, Nhật ký
            var zaloIntegration = new ApplicationMenuItem(
                name: "ZaloIntegration",
                displayName: l["Menu:ZaloIntegration"],
                icon: "fa fa-comments",
                order: 3
            );

            if (canSeeZaloAuths)
            {
                zaloIntegration.AddItem(
                    new ApplicationMenuItem(
                        name: "AppZaloAuths",
                        displayName: l["Menu:AppZaloAuths"],
                        url: "/AppZaloAuths",
                        icon: "fa fa-key",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.AppZaloAuths.Default)
                );
            }

            if (canSeeZaloLogs)
            {
                zaloIntegration.AddItem(
                    new ApplicationMenuItem(
                        name: "AppZaloLogs",
                        displayName: l["Menu:AppZaloLogs"],
                        url: "/AppZaloLogs",
                        icon: "fa fa-list-alt",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.AppZaloLogs.Default)
                );
            }

            // Nếu ít nhất 1 submenu thấy được thì add
            if (canSeeZaloAuths || canSeeZaloLogs)
            {
                groupMiniAppSetup.AddItem(zaloIntegration);
            }
            else
            {
                // Nếu chưa có quyền/feature thì vẫn hiển thị “Tích hợp Zalo OA” dạng coming soon, không click được
                groupMiniAppSetup.AddItem(
                    ComingSoon(
                        name: "ZaloIntegrationComingSoon",
                        displayName: $"{l["Menu:ZaloIntegration"]} {l["Menu:ComingSoon"]}",
                        icon: "fa fa-comments",
                        order: 3
                    )
                );
            }

            context.Menu.AddItem(groupMiniAppSetup);

            // =========================================================
            // 2) Sân golf & Giờ chơi
            // =========================================================
            var groupGolfAndTeeTimes = new ApplicationMenuItem(
                name: "MenuGroup.GolfAndTeeTimes",
                displayName: l["MenuGroup:GolfAndTeeTimes"],
                icon: "fa fa-flag",
                order: 20
            );

            if (canSeeGolfCourses)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppGolfCourses",
                        displayName: l["Menu:AppGolfCourses"],
                        url: "/AppGolfCourses",
                        icon: "fa fa-flag",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.AppGolfCourses.Default)
                );
            }

            if (canSeeCustomerTypes)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppCustomerTypes",
                        displayName: l["Menu:AppCustomerTypes"],
                        url: "/AppCustomerTypes",
                        icon: "fa fa-users",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.AppCustomerTypes.Default)
                );
            }

            if (canSeePromotionType)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppPromotionTypes",
                        displayName: l["Menu:AppPromotionTypes"],
                        url: "/AppPromotionTypes",
                        icon: "fa fa-tags",
                        order: 3
                    ).RequirePermissions(MultiTenancyPermissions.AppPromotionType.Default)
                );
            }

            if (canSeeCalendarSlots)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppCalendarSlots",
                        displayName: l["Menu:AppCalendarSlots"],
                        url: "/AppCalendarSlots",
                        icon: "fa fa-calendar",
                        order: 4
                    ).RequirePermissions(MultiTenancyPermissions.AppCalendarSlots.Default)
                );
            }
            
            if (canSeeSpecialDates)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppSpecialDates",
                        displayName: l["Menu:AppSpecialDates"],
                        url: "/AppSpecialDates",
                        icon: "fa fa-calendar-plus-o",
                        order: 5
                    ).RequirePermissions(MultiTenancyPermissions.AppSpecialDates.Default)
                );
            }

            context.Menu.AddItem(groupGolfAndTeeTimes);

            // =========================================================
            // 3) Khách hàng & Đặt chỗ
            // =========================================================
            var groupCustomerBooking = new ApplicationMenuItem(
                name: "MenuGroup.CustomerBooking",
                displayName: l["MenuGroup:CustomerBooking"],
                icon: "fa fa-address-book",
                order: 30
            );

            if (canSeeCustomers)
            {
                groupCustomerBooking.AddItem(
                    new ApplicationMenuItem(
                        name: "AppCustomers",
                        displayName: l["Menu:AppCustomers"],
                        url: "/AppCustomers",
                        icon: "fa fa-user",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.AppCustomers.Default)
                );
            }

            // Mã giảm giá (Coming soon)
            groupCustomerBooking.AddItem(
                ComingSoon(
                    name: "Coupons",
                    displayName: $"{l["Menu:Coupons"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-ticket",
                    order: 2
                )
            );

            if (canSeeBookings)
            {
                groupCustomerBooking.AddItem(
                    new ApplicationMenuItem(
                        name: "AppBookings",
                        displayName: l["Menu:AppBookings"],
                        url: "/AppBookings",
                        icon: "fa fa-calendar-check",
                        order: 3
                    ).RequirePermissions(MultiTenancyPermissions.AppBookings.Default)
                );
            }

            context.Menu.AddItem(groupCustomerBooking);

            // =========================================================
            // 4) Khách hàng trung thành
            // =========================================================
            var groupLoyalty = new ApplicationMenuItem(
                name: "MenuGroup.Loyalty",
                displayName: l["MenuGroup:Loyalty"],
                icon: "fa fa-gem",
                order: 40
            );

            if (canSeeMembershipTiers)
            {
                groupLoyalty.AddItem(
                    new ApplicationMenuItem(
                        name: "AppMembershipTiers",
                        displayName: l["Menu:AppMembershipTiers"],
                        url: "/AppMembershipTiers",
                        icon: "fa fa-medal",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.AppMembershipTiers.Default)
                );
            }

            groupLoyalty.AddItem(
                ComingSoon(
                    name: "GiftGroups",
                    displayName: $"{l["Menu:GiftGroups"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-layer-group",
                    order: 2
                )
            );

            groupLoyalty.AddItem(
                ComingSoon(
                    name: "Gifts",
                    displayName: $"{l["Menu:Gifts"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-gift",
                    order: 3
                )
            );

            groupLoyalty.AddItem(
                ComingSoon(
                    name: "RewardHistory",
                    displayName: $"{l["Menu:RewardHistory"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-history",
                    order: 4
                )
            );

            context.Menu.AddItem(groupLoyalty);

            // =========================================================
            // 5) Tin tức
            // =========================================================
            var groupNews = new ApplicationMenuItem(
                name: "MenuGroup.News",
                displayName: l["MenuGroup:News"],
                icon: "fa fa-newspaper-o",
                order: 50
            );

            if (canSeeNews)
            {
                groupNews.AddItem(
                    new ApplicationMenuItem(
                        name: "AppNews",
                        displayName: l["Menu:AppNews"],
                        url: "/AppNews",
                        icon: "fa fa-newspaper-o",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.AppNews.Default)
                );
            }

            context.Menu.AddItem(groupNews);

            // =========================================================
            // 6) Quản trị hệ thống (tận dụng Administration menu của ABP)
            // =========================================================
            var administration = context.Menu.GetAdministration();
            administration.DisplayName = l["MenuGroup:SystemAdmin"];
            administration.Icon = "fa fa-cog";
            administration.Order = 60;
            administration.Items.Clear();

            // Quản lý danh tính -> Vai trò / Người dùng
            var identityGroup = new ApplicationMenuItem(
                name: "System.Identity",
                displayName: l["Menu:SystemIdentity"],
                icon: "fa fa-user-shield",
                order: 1
            );

            identityGroup.AddItem(
                new ApplicationMenuItem(
                    name: "System.Roles",
                    displayName: l["Menu:Roles"],
                    url: "/Identity/Roles",
                    icon: "fa fa-shield",
                    order: 1
                ).RequirePermissions(IdentityPermissions.Roles.Default)
            );

            identityGroup.AddItem(
                new ApplicationMenuItem(
                    name: "System.Users",
                    displayName: l["Menu:Users"],
                    url: "/Identity/Users",
                    icon: "fa fa-users",
                    order: 2
                ).RequirePermissions(IdentityPermissions.Users.Default)
            );

            administration.AddItem(identityGroup);

            // Cài đặt nâng cấp (Setting Management)
            administration.AddItem(
                new ApplicationMenuItem(
                    name: "System.UpgradeSettings",
                    displayName: l["Menu:SystemUpgradeSettings"],
                    url: "/SettingManagement",
                    icon: "fa fa-wrench",
                    order: 2
                )
                .RequirePermissions("SettingManagement.Settings")
            );

            // Nhật ký hệ thống -> Nhật ký truy cập (coming soon) + Lịch sử gửi email (AppEmails)
            var systemLogs = new ApplicationMenuItem(
                name: "System.Logs",
                displayName: l["Menu:SystemLogs"],
                icon: "fa fa-list",
                order: 3
            );

            systemLogs.AddItem(
                ComingSoon(
                    name: "System.AccessLogs",
                    displayName: $"{l["Menu:SystemAccessLogs"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-file-text-o",
                    order: 1
                )
            );

            if (canSeeEmails)
            {
                systemLogs.AddItem(
                    new ApplicationMenuItem(
                        name: "AppEmails",
                        displayName: l["Menu:AppEmails"],
                        url: "/AppEmails",
                        icon: "fa fa-envelope",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.AppEmails.Default)
                );
            }

            administration.AddItem(systemLogs);
        }
        else // ================= HOST =================
        {
            // Host: cấu trúc menu giống Tenant nhưng check quyền theo Host*
            var hostCanAppSettings = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppSettings.Default);
            var hostCanCustomerTypes = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppCustomerTypes.Default);
            var hostCanGolfCourses = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppGolfCourses.Default);
            var hostCanMembershipTiers = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppMembershipTiers.Default);
            var hostCanCustomers = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppCustomers.Default);
            var hostCanCalendarSlots = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppCalendarSlots.Default);
            var hostCanNews = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppNews.Default);
            var hostCanBookings = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppBookings.Default);
            var hostCanPromotionType = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppPromotionType.Default);
            var hostCanZaloAuths = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppZaloAuths.Default);
            var hostCanZaloLogs = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppZaloLogs.Default);
            var hostCanEmails = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppEmails.Default);
            var hostCanSpecialDates = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppSpecialDates.Default);

            // 1) Cài đặt Mini App
            var groupMiniAppSetup = new ApplicationMenuItem(
                name: "MenuGroup.MiniAppSetup",
                displayName: l["MenuGroup:MiniAppSetup"],
                icon: "fa fa-sliders",
                order: 10
            );

            if (hostCanAppSettings)
            {
                groupMiniAppSetup.AddItem(
                    new ApplicationMenuItem(
                        name: "AppSettingsHost",
                        displayName: l["Menu:AppSettings"],
                        url: "/AppSettings",
                        icon: "fa fa-cogs",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.HostAppSettings.Default)
                );
            }

            groupMiniAppSetup.AddItem(
                ComingSoon(
                    name: "HomePageConfig",
                    displayName: $"{l["Menu:HomePageConfig"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-th-large",
                    order: 2
                )
            );

            var zaloIntegration = new ApplicationMenuItem(
                name: "ZaloIntegration",
                displayName: l["Menu:ZaloIntegration"],
                icon: "fa fa-comments",
                order: 3
            );

            if (hostCanZaloAuths)
            {
                zaloIntegration.AddItem(
                    new ApplicationMenuItem(
                        name: "AppZaloAuthsHost",
                        displayName: l["Menu:AppZaloAuths"],
                        url: "/AppZaloAuths",
                        icon: "fa fa-key",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.HostAppZaloAuths.Default)
                );
            }

            if (hostCanZaloLogs)
            {
                zaloIntegration.AddItem(
                    new ApplicationMenuItem(
                        name: "AppZaloLogsHost",
                        displayName: l["Menu:AppZaloLogs"],
                        url: "/AppZaloLogs",
                        icon: "fa fa-list-alt",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.HostAppZaloLogs.Default)
                );
            }

            if (hostCanZaloAuths || hostCanZaloLogs)
            {
                groupMiniAppSetup.AddItem(zaloIntegration);
            }
            else
            {
                groupMiniAppSetup.AddItem(
                    ComingSoon(
                        name: "ZaloIntegrationComingSoon",
                        displayName: $"{l["Menu:ZaloIntegration"]} {l["Menu:ComingSoon"]}",
                        icon: "fa fa-comments",
                        order: 3
                    )
                );
            }

            context.Menu.AddItem(groupMiniAppSetup);

            // 2) Sân golf & Giờ chơi
            var groupGolfAndTeeTimes = new ApplicationMenuItem(
                name: "MenuGroup.GolfAndTeeTimes",
                displayName: l["MenuGroup:GolfAndTeeTimes"],
                icon: "fa fa-flag",
                order: 20
            );

            if (hostCanGolfCourses)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppGolfCoursesHost",
                        displayName: l["Menu:AppGolfCourses"],
                        url: "/AppGolfCourses",
                        icon: "fa fa-flag",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.HostAppGolfCourses.Default)
                );
            }

            if (hostCanCustomerTypes)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppCustomerTypesHost",
                        displayName: l["Menu:AppCustomerTypes"],
                        url: "/AppCustomerTypes",
                        icon: "fa fa-users",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.HostAppCustomerTypes.Default)
                );
            }

            if (hostCanPromotionType)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppPromotionTypesHost",
                        displayName: l["Menu:AppPromotionTypes"],
                        url: "/AppPromotionTypes",
                        icon: "fa fa-tags",
                        order: 3
                    ).RequirePermissions(MultiTenancyPermissions.HostAppPromotionType.Default)
                );
            }

            if (hostCanCalendarSlots)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppCalendarSlotsHost",
                        displayName: l["Menu:AppCalendarSlots"],
                        url: "/AppCalendarSlots",
                        icon: "fa fa-calendar",
                        order: 4
                    ).RequirePermissions(MultiTenancyPermissions.HostAppCalendarSlots.Default)
                );
            }

            if (hostCanSpecialDates)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppSpecialDateHost",
                        displayName: l["Menu:AppSpecialDates"],
                        url: "/AppSpecialDates",
                        icon: "fa fa-calendar-plus-o",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.HostAppSpecialDates.Default)
                );
            }

            context.Menu.AddItem(groupGolfAndTeeTimes);

            // 3) Khách hàng & Đặt chỗ
            var groupCustomerBooking = new ApplicationMenuItem(
                name: "MenuGroup.CustomerBooking",
                displayName: l["MenuGroup:CustomerBooking"],
                icon: "fa fa-address-book",
                order: 30
            );

            if (hostCanCustomers)
            {
                groupCustomerBooking.AddItem(
                    new ApplicationMenuItem(
                        name: "AppCustomersHost",
                        displayName: l["Menu:AppCustomers"],
                        url: "/AppCustomers",
                        icon: "fa fa-user",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.HostAppCustomers.Default)
                );
            }

            groupCustomerBooking.AddItem(
                ComingSoon(
                    name: "Coupons",
                    displayName: $"{l["Menu:Coupons"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-ticket",
                    order: 2
                )
            );

            if (hostCanBookings)
            {
                groupCustomerBooking.AddItem(
                    new ApplicationMenuItem(
                        name: "AppBookingsHost",
                        displayName: l["Menu:AppBookings"],
                        url: "/AppBookings",
                        icon: "fa fa-calendar-check",
                        order: 3
                    ).RequirePermissions(MultiTenancyPermissions.HostAppBookings.Default)
                );
            }

            context.Menu.AddItem(groupCustomerBooking);

            // 4) Khách hàng trung thành
            var groupLoyalty = new ApplicationMenuItem(
                name: "MenuGroup.Loyalty",
                displayName: l["MenuGroup:Loyalty"],
                icon: "fa fa-gem",
                order: 40
            );

            if (hostCanMembershipTiers)
            {
                groupLoyalty.AddItem(
                    new ApplicationMenuItem(
                        name: "AppMembershipTiersHost",
                        displayName: l["Menu:AppMembershipTiers"],
                        url: "/AppMembershipTiers",
                        icon: "fa fa-medal",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.HostAppMembershipTiers.Default)
                );
            }

            groupLoyalty.AddItem(
                ComingSoon(
                    name: "GiftGroups",
                    displayName: $"{l["Menu:GiftGroups"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-layer-group",
                    order: 2
                )
            );

            groupLoyalty.AddItem(
                ComingSoon(
                    name: "Gifts",
                    displayName: $"{l["Menu:Gifts"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-gift",
                    order: 3
                )
            );

            groupLoyalty.AddItem(
                ComingSoon(
                    name: "RewardHistory",
                    displayName: $"{l["Menu:RewardHistory"]} {l["Menu:ComingSoon"]}",
                    icon: "fa fa-history",
                    order: 4
                )
            );

            context.Menu.AddItem(groupLoyalty);

            // 5) Tin tức
            var groupNews = new ApplicationMenuItem(
                name: "MenuGroup.News",
                displayName: l["MenuGroup:News"],
                icon: "fa fa-newspaper-o",
                order: 50
            );

            if (hostCanNews)
            {
                groupNews.AddItem(
                    new ApplicationMenuItem(
                        name: "AppNewsHost",
                        displayName: l["Menu:AppNews"],
                        url: "/AppNews",
                        icon: "fa fa-newspaper-o",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.HostAppNews.Default)
                );
            }

            context.Menu.AddItem(groupNews);

            // 6) Quản trị hệ thống
            var administration = context.Menu.GetAdministration();
            administration.DisplayName = l["MenuGroup:SystemAdmin"];
            administration.Icon = "fa fa-cog";
            administration.Order = 60;
            administration.Items.Clear();

            var identityGroup = new ApplicationMenuItem(
                name: "System.Identity",
                displayName: l["Menu:SystemIdentity"],
                icon: "fa fa-user-shield",
                order: 1
            );

            var tenantGroup = new ApplicationMenuItem(
                name: "System.TenantManagement",
                displayName: l["Menu:TenantManagement"],
                icon: "fa fa-building",
                order: 2
            );

            tenantGroup.AddItem(
                new ApplicationMenuItem(
                    name: "System.Tenants",
                    displayName: l["Menu:Tenants"],
                    url: "/TenantManagement/Tenants",
                    icon: "fa fa-building-o",
                    order: 1
                ).RequirePermissions(TenantManagementPermissions.Tenants.Default)
            );

            administration.AddItem(tenantGroup);

            identityGroup.AddItem(
                new ApplicationMenuItem(
                    name: "System.Roles",
                    displayName: l["Menu:Roles"],
                    url: "/Identity/Roles",
                    icon: "fa fa-shield",
                    order: 1
                ).RequirePermissions(IdentityPermissions.Roles.Default)
            );

            identityGroup.AddItem(
                new ApplicationMenuItem(
                    name: "System.Users",
                    displayName: l["Menu:Users"],
                    url: "/Identity/Users",
                    icon: "fa fa-users",
                    order: 2
                ).RequirePermissions(IdentityPermissions.Users.Default)
            );

            administration.AddItem(identityGroup);

            administration.AddItem(
                new ApplicationMenuItem(
                    name: "System.UpgradeSettings",
                    displayName: l["Menu:SystemUpgradeSettings"],
                    url: "/SettingManagement",
                    icon: "fa fa-wrench",
                    order: 3
                ).RequirePermissions("SettingManagement.Settings"));

            var systemLogs = new ApplicationMenuItem(
                name: "System.Logs",
                displayName: l["Menu:SystemLogs"],
                icon: "fa fa-list",
                order: 4
            );

            systemLogs.AddItem(
                //ComingSoon(
                //    name: "System.AccessLogs",
                //    displayName: $"{l["Menu:SystemAccessLogs"]} {l["Menu:ComingSoon"]}",
                //    icon: "fa fa-file-text-o",
                //    order: 1
                //)
                new ApplicationMenuItem(
                    name: "AuditLogs",
                    displayName: l["Menu:AuditLogs"],
                    url: "/Admin/AuditLogs",
                    icon: "fa fa-clipboard-list",
                    order: 60
                ).RequirePermissions(AuditLogPermissions.View)
            );

            if (hostCanEmails)
            {
                systemLogs.AddItem(
                    new ApplicationMenuItem(
                        name: "AppEmailsHost",
                        displayName: l["Menu:AppEmails"],
                        url: "/AppEmails",
                        icon: "fa fa-envelope",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.HostAppEmails.Default)
                );
            }

            administration.AddItem(systemLogs);
        }

        foreach (var rootItem in context.Menu.Items)
        {
            ApplyNativeTooltips(rootItem);
        }
    }
}
