using Genora.MultiTenancy.DomainModels.AppDocuments;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<DocumentSection, Guid> _sectionRepo;
    private readonly IRepository<DocumentPage, Guid> _pageRepo;
    private readonly IGuidGenerator _guidGenerator;

    // Permission constants are duplicated here as plain strings because Domain layer cannot
    // reference Application.Contracts. Keep these in sync with MultiTenancyPermissions.cs.
    private const string PermAppSettings = "MultiTenancy.AppSettings";
    private const string PermHostAppSettings = "MultiTenancy.HostAppSettings";
    private const string PermAppGolfCourses = "MultiTenancy.AppGolfCourses";
    private const string PermHostAppGolfCourses = "MultiTenancy.HostAppGolfCourses";
    private const string PermSalonBeautyBookings = "MultiTenancy.SalonBeautyBookings";
    private const string PermHostSalonBeautyBookings = "MultiTenancy.HostSalonBeautyBookings";
    private const string PermAppProOrders = "MultiTenancy.AppProOrders";
    private const string PermHostAppProOrders = "MultiTenancy.HostAppProOrders";
    private const string PermAppFnbOrders = "MultiTenancy.AppFnbOrders";
    private const string PermHostAppFnbOrders = "MultiTenancy.HostAppFnbOrders";
    private const string PermAppCustomers = "MultiTenancy.AppCustomers";
    private const string PermHostAppCustomers = "MultiTenancy.HostAppCustomers";
    private const string PermAppMembershipTiers = "MultiTenancy.AppMembershipTiers";
    private const string PermHostAppMembershipTiers = "MultiTenancy.HostAppMembershipTiers";
    private const string PermAppNews = "MultiTenancy.AppNews";
    private const string PermHostAppNews = "MultiTenancy.HostAppNews";
    private const string PermAppPaymentConfigurations = "MultiTenancy.AppPaymentConfigurations";
    private const string PermHostAppPaymentConfigurations = "MultiTenancy.HostAppPaymentConfigurations";
    private const string PermAppHomePageConfigs = "MultiTenancy.AppHomePageConfigs";
    private const string PermHostAppHomePageConfigs = "MultiTenancy.HostAppHomePageConfigs";
    private const string PermAppZaloAuths = "MultiTenancy.AppZaloAuths";
    private const string PermHostAppZaloAuths = "MultiTenancy.HostAppZaloAuths";
    private const string PermAppPromotionPolicies = "MultiTenancy.AppPromotionPolicies";
    private const string PermHostAppPromotionPolicies = "MultiTenancy.HostAppPromotionPolicies";
    private const string PermAppCustomerTypes = "MultiTenancy.AppCustomerTypes";
    private const string PermHostAppCustomerTypes = "MultiTenancy.HostAppCustomerTypes";
    private const string PermAppPromotionTypes = "MultiTenancy.AppPromotionType";
    private const string PermHostAppPromotionTypes = "MultiTenancy.HostAppPromotionType";
    private const string PermAppCalendarSlots = "MultiTenancy.AppCalendarSlots";
    private const string PermHostAppCalendarSlots = "MultiTenancy.HostAppCalendarSlots";
    private const string PermAppSpecialDates = "MultiTenancy.AppSpecialDates";
    private const string PermHostAppSpecialDates = "MultiTenancy.HostAppSpecialDates";
    private const string PermAppBookings = "MultiTenancy.AppBookings";
    private const string PermHostAppBookings = "MultiTenancy.HostAppBookings";
    private const string PermSalonBeautyCustomers = "MultiTenancy.SalonBeautyCustomers";
    private const string PermHostSalonBeautyCustomers = "MultiTenancy.HostSalonBeautyCustomers";
    private const string PermSalonBeautyLocations = "MultiTenancy.SalonBeautyLocations";
    private const string PermHostSalonBeautyLocations = "MultiTenancy.HostSalonBeautyLocations";
    private const string PermSalonBeautyTimeSlots = "MultiTenancy.SalonBeautyTimeSlots";
    private const string PermHostSalonBeautyTimeSlots = "MultiTenancy.HostSalonBeautyTimeSlots";
    private const string PermSalonBeautyServiceCategories = "MultiTenancy.SalonBeautyServiceCategories";
    private const string PermHostSalonBeautyServiceCategories = "MultiTenancy.HostSalonBeautyServiceCategories";
    private const string PermSalonBeautyServices = "MultiTenancy.SalonBeautyServices";
    private const string PermHostSalonBeautyServices = "MultiTenancy.HostSalonBeautyServices";
    private const string PermSalonBeautyStylists = "MultiTenancy.SalonBeautyStylists";
    private const string PermHostSalonBeautyStylists = "MultiTenancy.HostSalonBeautyStylists";
    private const string PermSalonBeautyDeposits = "MultiTenancy.SalonBeautyDeposits";
    private const string PermHostSalonBeautyDeposits = "MultiTenancy.HostSalonBeautyDeposits";
    private const string PermSalonBeautyLoyaltyConfig = "MultiTenancy.SalonBeautyLoyaltyConfig";
    private const string PermHostSalonBeautyLoyaltyConfig = "MultiTenancy.HostSalonBeautyLoyaltyConfig";
    private const string PermAppFnbCategories = "MultiTenancy.AppFnbCategories";
    private const string PermHostAppFnbCategories = "MultiTenancy.HostAppFnbCategories";
    private const string PermAppFnbItems = "MultiTenancy.AppFnbItems";
    private const string PermHostAppFnbItems = "MultiTenancy.HostAppFnbItems";
    private const string PermAppFnbKitchenBoard = "MultiTenancy.AppFnbKitchenBoard";
    private const string PermHostAppFnbKitchenBoard = "MultiTenancy.HostAppFnbKitchenBoard";
    private const string PermAppProCategories = "MultiTenancy.AppProCategories";
    private const string PermHostAppProCategories = "MultiTenancy.HostAppProCategories";
    private const string PermAppProItems = "MultiTenancy.AppProItems";
    private const string PermHostAppProItems = "MultiTenancy.HostAppProItems";
    private const string PermAppProOrdersBoard = "MultiTenancy.AppProOrdersBoard";
    private const string PermHostAppProOrdersBoard = "MultiTenancy.HostAppProOrdersBoard";
    private const string PermAppEmails = "MultiTenancy.AppEmails";
    private const string PermHostAppEmails = "MultiTenancy.HostAppEmails";

    // Feature constants — keep in sync with Application.Contracts/Features/*/*Features.cs
    private const string FeatAppSettings = "MiniAppSetting.Management";
    private const string FeatGolfCourse = "MiniAppGolfCourse.Management";
    private const string FeatSalonBeauty = "SalonBeauty.Management";
    private const string FeatProshop = "MiniAppProshop.Management";
    private const string FeatFnb = "MiniAppFnb.Management";
    private const string FeatBookings = "MiniAppBookings.Management";
    private const string FeatMembershipTier = "MiniAppMembershipTier.Management";
    private const string FeatNews = "MiniAppNews.Management";

    public AppDocumentsDataSeedContributor(
        IRepository<DocumentSection, Guid> sectionRepo,
        IRepository<DocumentPage, Guid> pageRepo,
        IGuidGenerator guidGenerator)
    {
        _sectionRepo = sectionRepo;
        _pageRepo = pageRepo;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // ──────────────────────────────────────────────────────────────────────
        // TẠMTẮT: AppDocuments data seed bị comment để tránh ghi đè dữ liệu đã
        // được user chỉnh sửa qua CMS. Bật lại khi cần seed lần đầu.
        // ──────────────────────────────────────────────────────────────────────
        return;

        /*
        // Host-shared, only seed once at host level.
        if (context.TenantId != null) return;

        var existing = (await _sectionRepo.GetListAsync())
            .ToDictionary(x => x.Slug, x => x, StringComparer.OrdinalIgnoreCase);

        var existingPages = (await _pageRepo.GetListAsync())
            .GroupBy(p => p.SectionId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(p => p.Slug, p => p, StringComparer.OrdinalIgnoreCase));

        var seeds = BuildSeeds();

        foreach (var seed in seeds)
        {
            DocumentSection section;

            if (existing.TryGetValue(seed.Slug, out var current))
            {
                // Upsert metadata only — keep user-edited Name/Icon/Order/Status/Content untouched.
                var changed = false;
                if (current.FeatureName != seed.FeatureName)
                {
                    current.FeatureName = seed.FeatureName;
                    changed = true;
                }
                if (current.TenantPermissionName != seed.TenantPermissionName)
                {
                    current.TenantPermissionName = seed.TenantPermissionName;
                    changed = true;
                }
                if (current.HostPermissionName != seed.HostPermissionName)
                {
                    current.HostPermissionName = seed.HostPermissionName;
                    changed = true;
                }

                if (changed)
                {
                    await _sectionRepo.UpdateAsync(current, autoSave: true);
                }

                section = current;
            }
            else
            {
                section = new DocumentSection(_guidGenerator.Create(), seed.Name, seed.Slug)
                {
                    Icon = seed.Icon,
                    DisplayOrder = seed.DisplayOrder,
                    FeatureName = seed.FeatureName,
                    TenantPermissionName = seed.TenantPermissionName,
                    HostPermissionName = seed.HostPermissionName,
                    Status = (byte)DocumentStatus.Published
                };

                await _sectionRepo.InsertAsync(section, autoSave: true);
            }

            // Seed pages for this section
            var sectionPages = existingPages.TryGetValue(section.Id, out var sp) ? sp : new Dictionary<string, DocumentPage>(StringComparer.OrdinalIgnoreCase);
            var pageSeeds = GetPagesForSection(seed.Slug);

            foreach (var pageSeed in pageSeeds)
            {
                if (sectionPages.TryGetValue(pageSeed.Slug, out var existingPage))
                {
                    // Upsert metadata only — preserve user-edited Title/Content/Status
                    var pageChanged = false;
                    if (existingPage.FeatureName != pageSeed.FeatureName)
                    {
                        existingPage.FeatureName = pageSeed.FeatureName;
                        pageChanged = true;
                    }
                    if (existingPage.TenantPermissionName != pageSeed.TenantPermissionName)
                    {
                        existingPage.TenantPermissionName = pageSeed.TenantPermissionName;
                        pageChanged = true;
                    }
                    if (existingPage.HostPermissionName != pageSeed.HostPermissionName)
                    {
                        existingPage.HostPermissionName = pageSeed.HostPermissionName;
                        pageChanged = true;
                    }

                    if (pageChanged)
                    {
                        await _pageRepo.UpdateAsync(existingPage, autoSave: true);
                    }
                    continue;
                }

                var page = new DocumentPage(
                    _guidGenerator.Create(),
                    section.Id,
                    pageSeed.Title,
                    pageSeed.Slug)
                {
                    ContentHtml = pageSeed.ContentHtml,
                    DisplayOrder = pageSeed.DisplayOrder,
                    Status = (byte)DocumentStatus.Published,
                    FeatureName = pageSeed.FeatureName,
                    TenantPermissionName = pageSeed.TenantPermissionName,
                    HostPermissionName = pageSeed.HostPermissionName
                };

                await _pageRepo.InsertAsync(page, autoSave: true);
            }
        }
        */
    }

    private static List<DocumentSeed> BuildSeeds() => new()
    {
        new DocumentSeed
        {
            Slug = "mini-app-setup",
            Name = "Cài đặt Mini App",
            Icon = "fa fa-sliders",
            DisplayOrder = 10,
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppSettings,
            HostPermissionName = PermHostAppSettings
        },
        new DocumentSeed
        {
            Slug = "golf-tee-times",
            Name = "Sân golf & Giờ chơi",
            Icon = "fa fa-flag",
            DisplayOrder = 20,
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppGolfCourses,
            HostPermissionName = PermHostAppGolfCourses
        },
        new DocumentSeed
        {
            Slug = "salon-location-schedule",
            Name = "Cơ sở & Lịch làm việc",
            Icon = "fa fa-building",
            DisplayOrder = 22,
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyLocations,
            HostPermissionName = PermHostSalonBeautyLocations
        },
        new DocumentSeed
        {
            Slug = "salon-beauty",
            Name = "Salon Beauty",
            Icon = "fa fa-spa",
            DisplayOrder = 25,
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyBookings,
            HostPermissionName = PermHostSalonBeautyBookings
        },
        new DocumentSeed
        {
            Slug = "proshop",
            Name = "Proshop",
            Icon = "fa fa-shopping-bag",
            DisplayOrder = 26,
            FeatureName = FeatProshop,
            TenantPermissionName = PermAppProOrders,
            HostPermissionName = PermHostAppProOrders
        },
        new DocumentSeed
        {
            Slug = "fnb",
            Name = "F&B",
            Icon = "fa fa-cutlery",
            DisplayOrder = 27,
            FeatureName = FeatFnb,
            TenantPermissionName = PermAppFnbOrders,
            HostPermissionName = PermHostAppFnbOrders
        },
        new DocumentSeed
        {
            Slug = "customer-booking",
            Name = "Khách hàng & Đặt chỗ (Golf)",
            Icon = "fa fa-address-book",
            DisplayOrder = 30,
            FeatureName = FeatBookings,
            TenantPermissionName = PermAppCustomers,
            HostPermissionName = PermHostAppCustomers
        },
        new DocumentSeed
        {
            Slug = "customer-booking-salon",
            Name = "Khách hàng & Đặt chỗ (Salon)",
            Icon = "fa fa-calendar-check-o",
            DisplayOrder = 32,
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyCustomers,
            HostPermissionName = PermHostSalonBeautyCustomers
        },
        new DocumentSeed
        {
            Slug = "loyalty",
            Name = "Khách hàng trung thành",
            Icon = "fa fa-gem",
            DisplayOrder = 40,
            FeatureName = FeatMembershipTier,
            TenantPermissionName = PermAppMembershipTiers,
            HostPermissionName = PermHostAppMembershipTiers
        },
        new DocumentSeed
        {
            Slug = "news",
            Name = "Tin tức",
            Icon = "fa fa-newspaper-o",
            DisplayOrder = 50,
            FeatureName = FeatNews,
            TenantPermissionName = PermAppNews,
            HostPermissionName = PermHostAppNews
        },
        new DocumentSeed
        {
            Slug = "system-admin",
            Name = "Quản trị hệ thống",
            Icon = "fa fa-cog",
            DisplayOrder = 60,
            // No feature/permission gate — always visible to anyone logged in.
            FeatureName = null,
            TenantPermissionName = null,
            HostPermissionName = null
        }
    };

    private sealed class DocumentSeed
    {
        public string Slug { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public string? FeatureName { get; set; }
        public string? TenantPermissionName { get; set; }
        public string? HostPermissionName { get; set; }
    }

    private sealed class PageSeed
    {
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public string ContentHtml { get; set; } = null!;
        public string? FeatureName { get; set; }
        public string? TenantPermissionName { get; set; }
        public string? HostPermissionName { get; set; }
    }
}
