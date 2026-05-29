using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Features.AppCalendarSlots;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Features.AppFnbFeatures;
using Genora.MultiTenancy.Features.AppProshopFeatures;
using Genora.MultiTenancy.Features.AppGolfCourses;
using Genora.MultiTenancy.Features.AppHomePages;
using Genora.MultiTenancy.Features.AppMembershipTiers;
using Genora.MultiTenancy.Features.AppNewsFeatures;
using Genora.MultiTenancy.Features.AppPromotionTypes;
using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Features.AppSpecialDates;
using Genora.MultiTenancy.Features.AppPaymentConfigurationFeatures;
using Genora.MultiTenancy.Features.AppZaloAuths;
using Genora.MultiTenancy.Features.AppZaloLogs;
using Genora.MultiTenancy.Features.SalonBeauty;
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

        // ===== Documents (User Guide) — visible to anyone logged in =====
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "Documents",
                l["Menu:Documents"],
                "/Documents",
                icon: "fa fa-book",
                order: 5
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

            var canSeePromotionPolicy =
                await feature.IsEnabledAsync(Genora.MultiTenancy.Features.AppPromotionPolicies.AppPromotionPolicyFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppPromotionPolicies.Default);

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

            var canSeePaymentConfigurations =
                await feature.IsEnabledAsync(AppPaymentConfigurationFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppPaymentConfigurations.Default);

            var canSeeSalonBeauty =
                await feature.IsEnabledAsync(SalonBeautyFeatures.Management) &&
                (
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyCustomers.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyServices.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyStylists.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyBookings.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyLocations.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyDeposits.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default)
                );

            // ===== Home Page Config (Theme + Widgets) =====
            var canSeeHomePageConfigs =
                await feature.IsEnabledAsync(AppHomePageFeatures.Management) &&
                await perms.IsGrantedAsync(MultiTenancyPermissions.AppHomePageConfigs.Default);

            var canSeeFnb =
                await feature.IsEnabledAsync(AppFnbFeatures.Management) &&
                (
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbCategories.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbItems.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbOrders.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbKitchenBoard.Default)
                );

            if (canSeeFnb)
            {
                var groupFnb = new ApplicationMenuItem(
                    name: "MenuGroup.FnB",
                    displayName: l["MenuGroup:FnB"],
                    icon: "fa fa-cutlery",
                    order: 45
                );

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbCategories.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbCategories",
                            displayName: l["Menu:AppFnbCategories"],
                            url: "/AppFnbCategories",
                            icon: "fa fa-folder-open",
                            order: 1
                        ).RequirePermissions(MultiTenancyPermissions.AppFnbCategories.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbItems.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbItems",
                            displayName: l["Menu:AppFnbItems"],
                            url: "/AppFnbItems",
                            icon: "fa fa-coffee",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.AppFnbItems.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbOrders.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbOrders",
                            displayName: l["Menu:AppFnbOrders"],
                            url: "/AppFnbOrders",
                            icon: "fa fa-receipt",
                            order: 3
                        ).RequirePermissions(MultiTenancyPermissions.AppFnbOrders.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppFnbKitchenBoard.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbKitchenBoard",
                            displayName: l["Menu:AppFnbKitchenBoard"],
                            url: "/AppFnbOrders/Kitchen",
                            icon: "fa fa-th-large",
                            order: 4
                        ).RequirePermissions(MultiTenancyPermissions.AppFnbKitchenBoard.Default)
                    );
                }

                context.Menu.AddItem(groupFnb);
            }

            // ── PROSHOP ──────────────────────────────────────────────────────
            var canSeeProshop =
                await feature.IsEnabledAsync(AppProshopFeatures.Management) &&
                (
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppProCategories.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppProItems.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppProOrders.Default) ||
                    await perms.IsGrantedAsync(MultiTenancyPermissions.AppProOrdersBoard.Default)
                );

            if (canSeeProshop)
            {
                var groupPro = new ApplicationMenuItem(
                    name: "MenuGroup.Proshop",
                    displayName: l["MenuGroup:Proshop"],
                    icon: "fa fa-shopping-bag",
                    order: 46
                );

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppProCategories.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProCategories",
                            displayName: l["Menu:AppProCategories"],
                            url: "/AppProCategories",
                            icon: "fa fa-folder-open",
                            order: 1
                        ).RequirePermissions(MultiTenancyPermissions.AppProCategories.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppProItems.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProItems",
                            displayName: l["Menu:AppProItems"],
                            url: "/AppProItems",
                            icon: "fa fa-tag",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.AppProItems.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppProOrders.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProOrders",
                            displayName: l["Menu:AppProOrders"],
                            url: "/AppProOrders",
                            icon: "fa fa-receipt",
                            order: 3
                        ).RequirePermissions(MultiTenancyPermissions.AppProOrders.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.AppProOrdersBoard.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProOrdersBoard",
                            displayName: l["Menu:AppProOrdersBoard"],
                            url: "/AppProOrders/Board",
                            icon: "fa fa-columns",
                            order: 4
                        ).RequirePermissions(MultiTenancyPermissions.AppProOrdersBoard.Default)
                    );
                }

                context.Menu.AddItem(groupPro);
            }

            // ── SALON BEAUTY ──────────────────────────────────────────────────
            if (canSeeSalonBeauty)
            {
                var groupSalonBeauty = new ApplicationMenuItem(
                    name: "MenuGroup.SalonBeauty",
                    displayName: l["MenuGroup:SalonBeauty"],
                    icon: "fa fa-spa",
                    order: 47
                );

                //if (await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyCustomers.Default))
                //{
                //    groupSalonBeauty.AddItem(
                //        new ApplicationMenuItem(
                //            name: "SalonBeautyCustomers",
                //            displayName: l["Menu:SalonBeautyCustomers"],
                //            url: "/SalonBeautyCustomers",
                //            icon: "fa fa-user",
                //            order: 1
                //        ).RequirePermissions(MultiTenancyPermissions.SalonBeautyCustomers.Default)
                //    );
                //}

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyServiceCategories",
                            displayName: l["Menu:SalonBeautyServiceCategories"],
                            url: "/SalonBeautyServiceCategories",
                            icon: "fa fa-folder-open",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.SalonBeautyServiceCategories.Default)
                    );
                }


                if (await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyServices.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyServices",
                            displayName: l["Menu:SalonBeautyServices"],
                            url: "/SalonBeautyServices",
                            icon: "fa fa-scissors",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.SalonBeautyServices.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyStylists.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyStylists",
                            displayName: l["Menu:SalonBeautyStylists"],
                            url: "/SalonBeautyStylists",
                            icon: "fa fa-id-card",
                            order: 3
                        ).RequirePermissions(MultiTenancyPermissions.SalonBeautyStylists.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyDeposits.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyDeposits",
                            displayName: l["Menu:SalonBeautyDeposits"],
                            url: "/SalonBeautyDeposits",
                            icon: "fa fa-wallet",
                            order: 5
                        ).RequirePermissions(MultiTenancyPermissions.SalonBeautyDeposits.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyLoyaltyConfig",
                            displayName: l["Menu:SalonBeautyLoyaltyConfig"],
                            url: "/SalonBeautyLoyaltyConfig",
                            icon: "fa fa-cog",
                            order: 6
                        ).RequirePermissions(MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default)
                    );
                }

                //if (await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyBookings.Default))
                //{
                //    groupSalonBeauty.AddItem(
                //        new ApplicationMenuItem(
                //            name: "SalonBeautyBookings",
                //            displayName: l["Menu:SalonBeautyBookings"],
                //            url: "/SalonBeautyBookings",
                //            icon: "fa fa-calendar-check",
                //            order: 4
                //        ).RequirePermissions(MultiTenancyPermissions.SalonBeautyBookings.Default)
                //    );
                //}

                context.Menu.AddItem(groupSalonBeauty);
            }

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

            // Cấu hình thanh toán
            if (canSeePaymentConfigurations)
            {
                groupMiniAppSetup.AddItem(
                    new ApplicationMenuItem(
                        name: "AppPaymentConfigurations",
                        displayName: l["Menu:AppPaymentConfigurations"],
                        url: "/AppPaymentConfigurations",
                        icon: "fa fa-credit-card",
                        order: 5
                    ).RequirePermissions(MultiTenancyPermissions.AppPaymentConfigurations.Default)
                );
            }

            // Cấu hình Trang chủ (Coming soon)
            if (canSeeHomePageConfigs)
            {
                groupMiniAppSetup.AddItem(
                    new ApplicationMenuItem(
                        name: "AppHomePageConfigs",
                        displayName: l["Menu:HomePageConfig"],
                        url: "/AppHomePageConfigs",
                        icon: "fa fa-th-large",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.AppHomePageConfigs.Default)
                );
            }
            else
            {
                groupMiniAppSetup.AddItem(
                    ComingSoon(
                        name: "AppHomePageConfigComingSoon",
                        displayName: $"{l["Menu:HomePageConfig"]} {l["Menu:ComingSoon"]}",
                        icon: "fa fa-th-large",
                        order: 2
                    )
                );
            }

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
                name: canSeeSalonBeauty == false ? "MenuGroup.GolfAndTeeTimes" : l["MenuGroup:SalonBeautyAndTeeTimes"],
                displayName: canSeeSalonBeauty == false ? l["MenuGroup:GolfAndTeeTimes"] : l["MenuGroup:SalonBeautyAndTeeTimes"],
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

            if (canSeeSalonBeauty && await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyLocations.Default))
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "SalonBeautyLocations",
                        displayName: l["Menu:SalonBeautyLocations"],
                        url: "/SalonBeautyLocations",
                        icon: "fa fa-store",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.SalonBeautyLocations.Default)
                );
            }

            if (canSeeSalonBeauty && await perms.IsGrantedAsync(MultiTenancyPermissions.SalonBeautyTimeSlots.Default))
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "SalonBeautyTimeSlots",
                        displayName: l["Menu:SalonBeautyTimeSlots"],
                        url: "/SalonBeautyTimeSlots",
                        icon: "fa fa-clock-o",
                        order: 6
                    ).RequirePermissions(MultiTenancyPermissions.SalonBeautyTimeSlots.Default)
                );
            }

            if (canSeePromotionPolicy)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppPromotionPolicies",
                        displayName: l["Menu:AppPromotionPolicies"],
                        url: "/AppPromotionPolicies",
                        icon: "fa fa-shield-halved",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.AppPromotionPolicies.Default)
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

            if(canSeeSalonBeauty)
            {
                groupCustomerBooking.AddItem(
                    new ApplicationMenuItem(
                        name: "SalonBeautyCustomers",
                        displayName: l["Menu:SalonBeautyCustomers"],
                        url: "/SalonBeautyCustomers",
                        icon: "fa fa-user",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.SalonBeautyCustomers.Default)
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

            if(canSeeSalonBeauty) 
            {
                groupCustomerBooking.AddItem(
                    new ApplicationMenuItem(
                        name: "SalonBeautyBookings",
                        displayName: l["Menu:SalonBeautyBookings"],
                        url: "/SalonBeautyBookings",
                        icon: "fa fa-calendar-check",
                        order: 4
                    ).RequirePermissions(MultiTenancyPermissions.SalonBeautyBookings.Default)
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
            var canSeeSettingManagement =
    await perms.IsGrantedAsync("SettingManagement.Settings") ||
    await perms.IsGrantedAsync("SettingManagement.Emailing") ||
    await perms.IsGrantedAsync("SettingManagement.TimeZone");

            if (canSeeSettingManagement)
            {
                var upgradeSettings = new ApplicationMenuItem(
                    name: "System.UpgradeSettings",
                    displayName: l["Menu:SystemUpgradeSettings"],
                    url: "/SettingManagement",
                    icon: "fa fa-wrench",
                    order: 1
                );

                // 1) Gửi email (ABP default)
                upgradeSettings.AddItem(
                    new ApplicationMenuItem(
                        name: "System.UpgradeSettings.Emailing",
                        displayName: l["Menu:SystemUpgradeSettings.Emailing"],
                        url: "/SettingManagement",
                        icon: "fa fa-envelope",
                        order: 1
                    )
                );

                // 2) Label / shortcut: ZNS/ZBS (Zalo)
                upgradeSettings.AddItem(
                    new ApplicationMenuItem(
                        name: "System.UpgradeSettings.ZaloZns",
                        displayName: l["Menu:SystemUpgradeSettings.ZaloZns"],
                        url: "/UpgradeSettings/ZaloZns",
                        icon: "fa fa-comments",
                        order: 2
                    )
                );

                // 3) Label / shortcut: Email templates
                upgradeSettings.AddItem(
                    new ApplicationMenuItem(
                        name: "System.UpgradeSettings.EmailTemplates",
                        displayName: l["Menu:SystemUpgradeSettings.EmailTemplates"],
                        url: "/UpgradeSettings/EmailTemplates",
                        icon: "fa fa-file-text-o",
                        order: 3
                    )
                );

                administration.AddItem(upgradeSettings);
            }

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
            var hostCanPromotionPolicy = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppPromotionPolicies.Default);
            var hostCanZaloAuths = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppZaloAuths.Default);
            var hostCanZaloLogs = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppZaloLogs.Default);
            var hostCanEmails = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppEmails.Default);
            var hostCanSpecialDates = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppSpecialDates.Default);
            var hostCanPaymentConfigurations = await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppPaymentConfigurations.Default);

            var hostCanSeeSalonBeauty =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyCustomers.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyServiceCategories.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyServices.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyStylists.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyBookings.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyLocations.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyDeposits.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default);

            var hostCanSeeFnb =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbCategories.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbItems.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbOrders.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbKitchenBoard.Default);

            if (hostCanSeeFnb)
            {
                var groupFnb = new ApplicationMenuItem(
                    name: "MenuGroup.FnB",
                    displayName: l["MenuGroup:FnB"],
                    icon: "fa fa-cutlery",
                    order: 45
                );

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbCategories.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbCategoriesHost",
                            displayName: l["Menu:AppFnbCategories"],
                            url: "/AppFnbCategories",
                            icon: "fa fa-folder-open",
                            order: 1
                        ).RequirePermissions(MultiTenancyPermissions.HostAppFnbCategories.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbItems.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbItemsHost",
                            displayName: l["Menu:AppFnbItems"],
                            url: "/AppFnbItems",
                            icon: "fa fa-coffee",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.HostAppFnbItems.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbOrders.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbOrdersHost",
                            displayName: l["Menu:AppFnbOrders"],
                            url: "/AppFnbOrders",
                            icon: "fa fa-receipt",
                            order: 3
                        ).RequirePermissions(MultiTenancyPermissions.HostAppFnbOrders.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppFnbKitchenBoard.Default))
                {
                    groupFnb.AddItem(
                        new ApplicationMenuItem(
                            name: "AppFnbKitchenBoardHost",
                            displayName: l["Menu:AppFnbKitchenBoard"],
                            url: "/AppFnbOrders/Kitchen",
                            icon: "fa fa-th-large",
                            order: 4
                        ).RequirePermissions(MultiTenancyPermissions.HostAppFnbKitchenBoard.Default)
                    );
                }

                context.Menu.AddItem(groupFnb);
            }

            // ── PROSHOP (HOST) ────────────────────────────────────────────────
            var hostCanSeeProshop =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProCategories.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProItems.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProOrders.Default) ||
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProOrdersBoard.Default);

            if (hostCanSeeProshop)
            {
                var groupPro = new ApplicationMenuItem(
                    name: "MenuGroup.Proshop",
                    displayName: l["MenuGroup:Proshop"],
                    icon: "fa fa-shopping-bag",
                    order: 46
                );

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProCategories.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProCategoriesHost",
                            displayName: l["Menu:AppProCategories"],
                            url: "/AppProCategories",
                            icon: "fa fa-folder-open",
                            order: 1
                        ).RequirePermissions(MultiTenancyPermissions.HostAppProCategories.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProItems.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProItemsHost",
                            displayName: l["Menu:AppProItems"],
                            url: "/AppProItems",
                            icon: "fa fa-tag",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.HostAppProItems.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProOrders.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProOrdersHost",
                            displayName: l["Menu:AppProOrders"],
                            url: "/AppProOrders",
                            icon: "fa fa-receipt",
                            order: 3
                        ).RequirePermissions(MultiTenancyPermissions.HostAppProOrders.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppProOrdersBoard.Default))
                {
                    groupPro.AddItem(
                        new ApplicationMenuItem(
                            name: "AppProOrdersBoardHost",
                            displayName: l["Menu:AppProOrdersBoard"],
                            url: "/AppProOrders/Board",
                            icon: "fa fa-columns",
                            order: 4
                        ).RequirePermissions(MultiTenancyPermissions.HostAppProOrdersBoard.Default)
                    );
                }

                context.Menu.AddItem(groupPro);
            }

            // ── SALON BEAUTY (HOST) ────────────────────────────────────────────────
            if (hostCanSeeSalonBeauty)
            {
                var groupSalonBeauty = new ApplicationMenuItem(
                    name: "MenuGroup.SalonBeauty",
                    displayName: l["MenuGroup:SalonBeauty"],
                    icon: "fa fa-spa",
                    order: 47
                );

                //if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyCustomers.Default))
                //{
                //    groupSalonBeauty.AddItem(
                //        new ApplicationMenuItem(
                //            name: "SalonBeautyCustomersHost",
                //            displayName: l["Menu:SalonBeautyCustomers"],
                //            url: "/SalonBeautyCustomers",
                //            icon: "fa fa-user",
                //            order: 1
                //        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyCustomers.Default)
                //    );
                //}

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyServiceCategories.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyServiceCategories",
                            displayName: l["Menu:SalonBeautyServiceCategories"],
                            url: "/SalonBeautyServiceCategories",
                            icon: "fa fa-folder-open",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyServiceCategories.Default)
                    );
                }


                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyServices.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyServicesHost",
                            displayName: l["Menu:SalonBeautyServices"],
                            url: "/SalonBeautyServices",
                            icon: "fa fa-scissors",
                            order: 2
                        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyServices.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyStylists.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyStylistsHost",
                            displayName: l["Menu:SalonBeautyStylists"],
                            url: "/SalonBeautyStylists",
                            icon: "fa fa-id-card",
                            order: 3
                        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyStylists.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyDeposits.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyDeposits",
                            displayName: l["Menu:SalonBeautyDeposits"],
                            url: "/SalonBeautyDeposits",
                            icon: "fa fa-wallet",
                            order: 5
                        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyDeposits.Default)
                    );
                }

                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default))
                {
                    groupSalonBeauty.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyLoyaltyConfig",
                            displayName: l["Menu:SalonBeautyLoyaltyConfig"],
                            url: "/SalonBeautyLoyaltyConfig",
                            icon: "fa fa-cog",
                            order: 6
                        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default)
                    );
                }

                //if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyBookings.Default))
                //{
                //    groupSalonBeauty.AddItem(
                //        new ApplicationMenuItem(
                //            name: "SalonBeautyBookingsHost",
                //            displayName: l["Menu:SalonBeautyBookings"],
                //            url: "/SalonBeautyBookings",
                //            icon: "fa fa-calendar-check",
                //            order: 4
                //        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyBookings.Default)
                //    );
                //}

                context.Menu.AddItem(groupSalonBeauty);
            }

            // ===== Home Page Config (Theme + Widgets) - HOST =====
            var hostCanSeeHomePageConfigs =
                await perms.IsGrantedAsync(MultiTenancyPermissions.HostAppHomePageConfigs.Default);

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

            if (hostCanPaymentConfigurations)
            {
                groupMiniAppSetup.AddItem(
                    new ApplicationMenuItem(
                        name: "AppPaymentConfigurationsHost",
                        displayName: l["Menu:AppPaymentConfigurations"],
                        url: "/AppPaymentConfigurations",
                        icon: "fa fa-credit-card",
                        order: 5
                    ).RequirePermissions(MultiTenancyPermissions.HostAppPaymentConfigurations.Default)
                );
            }

            if (hostCanSeeHomePageConfigs)
            {
                groupMiniAppSetup.AddItem(
                    new ApplicationMenuItem(
                        name: "AppHomePageConfigHost",
                        displayName: l["Menu:HomePageConfig"],
                        url: "/AppHomePageConfigs",
                        icon: "fa fa-th-large",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.HostAppHomePageConfigs.Default)
                );
            }
            else
            {
                groupMiniAppSetup.AddItem(
                    ComingSoon(
                        name: "AppHomePageConfigHostComingSoon",
                        displayName: $"{l["Menu:HomePageConfig"]} {l["Menu:ComingSoon"]}",
                        icon: "fa fa-th-large",
                        order: 2
                    )
                );
            }

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
            //var groupGolfAndTeeTimes = new ApplicationMenuItem(
            //    name: "MenuGroup.GolfAndTeeTimes",
            //    displayName: l["MenuGroup:GolfAndTeeTimes"],
            //    icon: "fa fa-flag",
            //    order: 20
            //);
            var groupGolfAndTeeTimes = new ApplicationMenuItem(
               name: hostCanSeeSalonBeauty == false ? "MenuGroup.GolfAndTeeTimes" : l["MenuGroup:SalonBeautyAndTeeTimes"],
               displayName: hostCanSeeSalonBeauty == false ? l["MenuGroup:GolfAndTeeTimes"] : l["MenuGroup:SalonBeautyAndTeeTimes"],
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

            if (hostCanSeeSalonBeauty && await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyLocations.Default))
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "SalonBeautyLocationsHost",
                        displayName: l["Menu:SalonBeautyLocations"],
                        url: "/SalonBeautyLocations",
                        icon: "fa fa-store",
                        order: 1
                    ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyLocations.Default)
                );
            }

            if (hostCanSeeSalonBeauty && await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default))
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "SalonBeautyTimeSlotsHost",
                        displayName: l["Menu:SalonBeautyTimeSlots"],
                        url: "/SalonBeautyTimeSlots",
                        icon: "fa fa-clock-o",
                        order: 6
                    ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default)
                );
            }

            if (hostCanPromotionPolicy)
            {
                groupGolfAndTeeTimes.AddItem(
                    new ApplicationMenuItem(
                        name: "AppPromotionPoliciesHost",
                        displayName: l["Menu:AppPromotionPolicies"],
                        url: "/AppPromotionPolicies",
                        icon: "fa fa-shield-halved",
                        order: 2
                    ).RequirePermissions(MultiTenancyPermissions.HostAppPromotionPolicies.Default)
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
            if(hostCanSeeSalonBeauty)
            {
                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyCustomers.Default))
                {
                    groupCustomerBooking.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyCustomersHost",
                            displayName: l["Menu:SalonBeautyCustomers"],
                            url: "/SalonBeautyCustomers",
                            icon: "fa fa-user",
                            order: 1
                        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyCustomers.Default)
                    );
                }
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

            if (hostCanSeeSalonBeauty)
            {
                if (await perms.IsGrantedAsync(MultiTenancyPermissions.HostSalonBeautyBookings.Default))
                {
                    groupCustomerBooking.AddItem(
                        new ApplicationMenuItem(
                            name: "SalonBeautyBookingsHost",
                            displayName: l["Menu:SalonBeautyBookings"],
                            url: "/SalonBeautyBookings",
                            icon: "fa fa-calendar-check",
                            order: 4
                        ).RequirePermissions(MultiTenancyPermissions.HostSalonBeautyBookings.Default)
                    );
                }
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

            var hostCanSeeSettingManagement =
    await perms.IsGrantedAsync("SettingManagement.Settings") ||
    await perms.IsGrantedAsync("SettingManagement.Emailing") ||
    await perms.IsGrantedAsync("SettingManagement.TimeZone");

            if (hostCanSeeSettingManagement)
            {
                var upgradeSettings = new ApplicationMenuItem(
                    name: "System.UpgradeSettings",
                    displayName: l["Menu:SystemUpgradeSettings"],
                    url: "/SettingManagement",
                    icon: "fa fa-wrench",
                    order: 1
                );

                // 1) Gửi email (ABP default)
                upgradeSettings.AddItem(
                    new ApplicationMenuItem(
                        name: "System.UpgradeSettings.Emailing",
                        displayName: l["Menu:SystemUpgradeSettings.Emailing"],
                        url: "/SettingManagement",
                        icon: "fa fa-envelope",
                        order: 1
                    )
                );

                // 2) Label / shortcut: ZNS/ZBS (Zalo)
                upgradeSettings.AddItem(
                    new ApplicationMenuItem(
                        name: "System.UpgradeSettings.ZaloZns",
                        displayName: l["Menu:SystemUpgradeSettings.ZaloZns"],
                        url: "/UpgradeSettings/ZaloZns",
                        icon: "fa fa-comments",
                        order: 2
                    )
                );

                // 3) Label / shortcut: Email templates
                upgradeSettings.AddItem(
                    new ApplicationMenuItem(
                        name: "System.UpgradeSettings.EmailTemplates",
                        displayName: l["Menu:SystemUpgradeSettings.EmailTemplates"],
                        url: "/UpgradeSettings/EmailTemplates",
                        icon: "fa fa-file-text-o",
                        order: 3
                    )
                );

                administration.AddItem(upgradeSettings);
            }

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
