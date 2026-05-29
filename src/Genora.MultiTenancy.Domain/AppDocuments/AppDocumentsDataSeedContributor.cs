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

public class AppDocumentsDataSeedContributor : IDataSeedContributor, ITransientDependency
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
        // Host-shared, only seed once at host level.
        if (context.TenantId != null) return;

        var existing = (await _sectionRepo.GetListAsync())
            .ToDictionary(x => x.Slug, x => x, StringComparer.OrdinalIgnoreCase);

        var seeds = BuildSeeds();

        foreach (var seed in seeds)
        {
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
                continue;
            }

            var section = new DocumentSection(_guidGenerator.Create(), seed.Name, seed.Slug)
            {
                Icon = seed.Icon,
                DisplayOrder = seed.DisplayOrder,
                FeatureName = seed.FeatureName,
                TenantPermissionName = seed.TenantPermissionName,
                HostPermissionName = seed.HostPermissionName,
                Status = (byte)DocumentStatus.Published
            };

            await _sectionRepo.InsertAsync(section, autoSave: true);

            var page = new DocumentPage(
                _guidGenerator.Create(),
                section.Id,
                "Giới thiệu",
                "gioi-thieu")
            {
                ContentHtml = $"<h2>{System.Net.WebUtility.HtmlEncode(seed.Name)}</h2>"
                    + "<p>Nội dung đang được biên soạn. Quản trị viên Host có thể chỉnh sửa nội dung tại trang quản lý tài liệu.</p>",
                DisplayOrder = 1,
                Status = (byte)DocumentStatus.Published
            };

            await _pageRepo.InsertAsync(page, autoSave: true);
        }
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
            Name = "Khách hàng & Đặt chỗ",
            Icon = "fa fa-address-book",
            DisplayOrder = 30,
            FeatureName = FeatBookings,
            TenantPermissionName = PermAppCustomers,
            HostPermissionName = PermHostAppCustomers
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
}
