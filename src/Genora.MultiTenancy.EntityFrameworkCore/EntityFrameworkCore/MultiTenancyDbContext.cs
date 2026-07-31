using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.Diagnostics;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppBookingStatusHistories;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomerMemberships;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppEmails;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrderActivity;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.DomainModels.AppProCategories;
using Genora.MultiTenancy.DomainModels.AppProItems;
using Genora.MultiTenancy.DomainModels.AppProOrderActivity;
using Genora.MultiTenancy.DomainModels.AppProOrders;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppHomePageConfigs;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppPaymentConfigurations;
using Genora.MultiTenancy.DomainModels.AppPromotionPolicies;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.DomainModels.AppDocuments;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.DomainModels.AppHlApiLogs;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Genora.MultiTenancy.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class MultiTenancyDbContext :
    AbpDbContext<MultiTenancyDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    private readonly SerilogCommandInterceptor _sqlInterceptor;
    private readonly IHostEnvironment _env;

    public DbSet<AppSetting> AppSettings { get; set; }

    // Mini App
    public DbSet<CustomerType> CustomerType { get; set; }
    public DbSet<GolfCourse> GolfCourse { get; set; }
    public DbSet<MembershipTier> MembershipTier { get; set; }
    public DbSet<Customer> Customer { get; set; }
    public DbSet<CalendarSlot> CalendarSlot { get; set; }
    public DbSet<CalendarSlotPrice> CalendarSlotPrice { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<NewsRelated> NewsRelateds { get; set; }
    public DbSet<CustomerMembership> CustomerMembership { get; set; }
    public DbSet<Booking> Booking { get; set; }
    public DbSet<BookingPlayer> BookingPlayer { get; set; }
    public DbSet<BookingStatusHistory> BookingStatusHistory { get; set; }
    public DbSet<ZaloAuth> ZaloAuths { get; set; }
    public DbSet<ZaloLog> ZaloLogs { get; set; }  // nếu có
    public DbSet<SpecialDate> SpecialDates { get; set; }  // nếu có
    public DbSet<Email> AppEmails { get; set; }
    public DbSet<AppHomePageConfig> AppHomePageConfigs { get; set; }
    public DbSet<AppHomePageWidget> AppHomePageWidgets { get; set; }
    public DbSet<AppHomePageWidgetItem> AppHomePageWidgetItems { get; set; }

    // FnB
    public DbSet<FnbCategory> AppFnbCategories { get; set; }
    public DbSet<FnbItem> AppFnbItems { get; set; }
    public DbSet<FnbOrder> AppFnbOrders { get; set; }
    public DbSet<FnbOrderItem> AppFnbOrderItems { get; set; }
    public DbSet<FnbOrderActivity> AppFnbOrderActivities { get; set; }

    // Proshop
    public DbSet<ProCategory> AppProCategories { get; set; }
    public DbSet<ProItem> AppProItems { get; set; }
    public DbSet<ProOrder> AppProOrders { get; set; }
    public DbSet<ProOrderItem> AppProOrderItems { get; set; }
    public DbSet<ProOrderActivity> AppProOrderActivities { get; set; }

    // Payment
    public DbSet<PaymentConfiguration> AppPaymentConfigurations { get; set; }

    // Salon Beauty
    public DbSet<SalonBeautyCustomer> SalonBeautyCustomer { get; set; }
    public DbSet<SalonBeautyServiceCategory> SalonBeautyServiceCategory { get; set; }
    public DbSet<SalonBeautyService> SalonBeautyService { get; set; }
    public DbSet<SalonBeautyStylist> SalonBeautyStylist { get; set; }
    public DbSet<SalonBeautyBooking> SalonBeautyBooking { get; set; }
    public DbSet<SalonBeautyBookingService> SalonBeautyBookingService { get; set; }
    public DbSet<SalonBeautyCustomerLoyaltyBalance> SalonBeautyCustomerLoyaltyBalance { get; set; }
    public DbSet<SalonBeautyCustomerLoyaltyTransaction> SalonBeautyCustomerLoyaltyTransaction { get; set; }
    public DbSet<SalonBeautyLocation> SalonBeautyLocation { get; set; }
    public DbSet<SalonBeautyTimeSlot> SalonBeautyTimeSlot { get; set; }
    public DbSet<SalonBeautyDepositTransaction> SalonBeautyDepositTransaction { get; set; }
    public DbSet<SalonBeautyLoyaltyBonusTier> SalonBeautyLoyaltyBonusTier { get; set; }

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<OptionExtend> OptionExtends { get; set; }
    public DbSet<PromotionType> PromotionTypes { get; set; }
    public DbSet<PromotionPolicy> PromotionPolicies { get; set; }

    // Documentation
    public DbSet<DocumentSection> AppDocumentSections { get; set; }
    public DbSet<DocumentPage> AppDocumentPages { get; set; }

    // Caddie
    public DbSet<AppCaddie> AppCaddies { get; set; }
    public DbSet<AppLanguage> AppLanguages { get; set; }
    public DbSet<AppCaddieLanguage> AppCaddieLanguages { get; set; }
    public DbSet<AppCaddieVoiceRegion> AppCaddieVoiceRegions { get; set; }
    public DbSet<AppCaddieSkill> AppCaddieSkills { get; set; }
    public DbSet<AppCaddieSchedule> AppCaddieSchedules { get; set; }
    public DbSet<AppCaddieBooking> AppCaddieBookings { get; set; }
    public DbSet<AppCaddieBookingDetail> AppCaddieBookingDetails { get; set; }
    public DbSet<AppCaddieScheduleTemplate> AppCaddieScheduleTemplates { get; set; }
    public DbSet<AppCaddieRating> AppCaddieRatings { get; set; }
    public DbSet<AppCaddieRatingDetail> AppCaddieRatingDetails { get; set; }

    // Hoa Linh
    public DbSet<HlOrder> AppHlOrders { get; set; }
    public DbSet<HlOrderItem> AppHlOrderItems { get; set; }
    public DbSet<HlGiftExchange> AppHlGiftExchanges { get; set; }
    public DbSet<HlApiLog> AppHlApiLogs { get; set; }
    public DbSet<HlPointBatch> AppHlPointBatches { get; set; }
    public DbSet<HlPointTransaction> AppHlPointTransactions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public MultiTenancyDbContext(
       DbContextOptions<MultiTenancyDbContext> options,
       SerilogCommandInterceptor sqlInterceptor,
       IHostEnvironment env)
       : base(options)
    {
        _sqlInterceptor = sqlInterceptor;
        _env = env;
    }

    public MultiTenancyDbContext(DbContextOptions<MultiTenancyDbContext> options)
        : base(options)
    {
    }

    // Bật log/diagnostic & interceptor
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Có interceptor thì gắn; nếu null (design-time) thì bỏ qua
        // Tạm tắt SerilogCommandInterceptor để giảm tải log SQL
        //if (_sqlInterceptor is not null)
        //    optionsBuilder.AddInterceptors(_sqlInterceptor);

        // Chỉ DEV mới bật logging nhạy cảm; nếu _env null (design-time) thì không bật
        if (_env?.IsDevelopment() == true)
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.Entity<AppSetting>(b =>
        {
            b.ToTable(MultiTenancyConsts.DbTablePrefix + "Settings", MultiTenancyConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SettingKey).IsRequired().HasMaxLength(100);
        });

        // Mini App domain module
        builder.ConfigureMiniAppModule();
        builder.ConfigureFnbModule();
        builder.ConfigureProshopModule();
        builder.ConfigurePaymentModule();
        builder.ConfigureSalonBeautyModule();
        builder.ConfigureCaddieModule();
        builder.ConfigureHoaLinhModule();

        builder.Entity<ZaloAuth>(b =>
        {
            b.ToTable("AppZaloAuth");
            b.ConfigureByConvention();
            b.Property(x => x.AppId).IsRequired().HasMaxLength(50);
            b.Property(x => x.OaId).HasMaxLength(50);
            b.Property(x => x.State).HasMaxLength(100);
            b.Property(x => x.CodeChallenge).HasMaxLength(200);
            b.Property(x => x.CodeVerifier).HasMaxLength(200);
            b.HasIndex(x => new { x.AppId, x.State });
        });

        builder.Entity<ZaloLog>(b =>
        {
            b.ToTable("AppZaloLog");
            b.ConfigureByConvention();

            b.Property(x => x.Action).HasMaxLength(128);
            b.Property(x => x.Endpoint).HasMaxLength(512);

            // ✅ Index chính phục vụ list theo scope + sort/filter theo thời gian
            b.HasIndex(x => new { x.TenantId, x.CreationTime });
        });

        // ===== AppCustomers =====
        builder.Entity<Customer>(b =>
        {
            b.ToTable("AppCustomers");
            b.ConfigureByConvention();

            b.Property(x => x.VgaCode).HasMaxLength(20);
            b.Property(x => x.Address).HasMaxLength(500);
            b.Property(x => x.Email).HasMaxLength(100);

            b.Property(x => x.IsFollower);
            b.Property(x => x.IsSensitive);

            b.Property(x => x.BonusPoint).HasColumnType("decimal(18,2)");

            b.HasOne(x => x.MembershipTier)
             .WithMany()
             .HasForeignKey(x => x.MembershipTierId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.MembershipTierId);
            b.Property(x => x.ProvinceCode).HasMaxLength(20);
            b.HasIndex(x => x.ProvinceCode);
            // ✅ Unique theo (TenantId, CustomerCode) nhưng CHỈ áp dụng cho record đang active
            b.HasIndex(x => new { x.TenantId, x.CustomerCode })
             .IsUnique()
             .HasDatabaseName("IX_AppCustomers_TenantId_CustomerCode")
             .HasFilter("[IsActive] = 1 AND [CustomerCode] IS NOT NULL");
        });

        // ===== AppGolfCourses =====
        builder.Entity<GolfCourse>(b =>
        {
            b.ToTable("AppGolfCourses");
            b.ConfigureByConvention();

            b.Property(x => x.FrameTimes).HasMaxLength(50);
            b.Property(x => x.NumberHoles);
            b.Property(x => x.Utilities).HasMaxLength(20);

            b.Property(x => x.CancellationPolicyHours)
                .HasColumnType("smallint")
                .IsRequired(false);

            b.Property(x => x.PromotionTypeIds)
                .HasMaxLength(1000)
                .IsRequired(false);

            b.Property(x => x.IsMemberSupported)
                .HasDefaultValue(false)
                .IsRequired();

            b.Property(x => x.MaxMemberGuest)
                .IsRequired(false);
        });

        // ===== AppPromotionPolicies =====
        builder.Entity<PromotionPolicy>(b =>
        {
            b.ToTable("AppPromotionPolicies");
            b.ConfigureByConvention();

            b.Property(x => x.PolicyTitle).HasMaxLength(255);
            b.Property(x => x.CancellationPolicyHours).IsRequired(false);
            b.Property(x => x.CancellationPolicyHoursWeekend).IsRequired(false);
            b.Property(x => x.CancellationPolicyContent).HasColumnType("nvarchar(max)");

            b.HasOne(x => x.GolfCourse)
             .WithMany()
             .HasForeignKey(x => x.GolfCourseId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.PromotionType)
             .WithMany()
             .HasForeignKey(x => x.PromotionTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.GolfCourseId, x.PromotionTypeId })
             .IsUnique()
             .HasDatabaseName("IX_AppPromotionPolicies_Tenant_GolfCourse_Promotion");
        });

        // ===== AppBookingPlayers =====
        builder.Entity<BookingPlayer>(b =>
        {
            b.ToTable("AppBookingPlayers");
            b.ConfigureByConvention();

            b.Property(x => x.PricePerPlayer).HasColumnType("decimal(18,2)");
            b.Property(x => x.VgaCode).HasMaxLength(50);
            b.Property(x => x.CaddieName).HasMaxLength(255);
        });

        // ===== AppBookings =====
        builder.Entity<Booking>(b =>
        {
            b.ToTable("AppBookings");
            b.ConfigureByConvention();

            b.Property(x => x.NumberHole).HasMaxLength(20);

            b.Property(x => x.TotalCaddieFee).HasColumnType("decimal(18,2)");

            b.Property(x => x.Utility).HasMaxLength(20).HasColumnName("Ultility");

            b.Property(x => x.IsExportInvoice);

            b.Property(x => x.CompanyName).HasMaxLength(200);
            b.Property(x => x.TaxCode).HasMaxLength(50);
            b.Property(x => x.CompanyAddress).HasMaxLength(500);
            b.Property(x => x.InvoiceEmail).HasMaxLength(256);
        });

        builder.Entity<MembershipTier>(b =>
        {
            b.ToTable("AppMembershipTiers");
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);

            b.Property(x => x.MinTotalSpending).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.Code);
        });

        builder.Entity<CustomerMembership>(b =>
        {
            b.ToTable("AppCustomerMemberships");
            b.ConfigureByConvention();

            b.HasOne(x => x.Customer)
             .WithMany(x => x.Memberships)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.MembershipTier)
             .WithMany(x => x.CustomerMemberships)
             .HasForeignKey(x => x.MembershipTierId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.CustomerId, x.IsCurrent });
        });

        // ===== AppCalendarSlotPrices =====
        builder.Entity<CalendarSlotPrice>(b =>
        {
            b.ToTable("AppCalendarSlotPrices");
            b.ConfigureByConvention();

            // Price theo số hố
            b.Property(x => x.Price9).HasColumnType("decimal(18,2)").IsRequired(false);
            b.Property(x => x.Price18).HasColumnType("decimal(18,2)").IsRequired();     // non-null
            b.Property(x => x.Price27).HasColumnType("decimal(18,2)").IsRequired(false);
            b.Property(x => x.Price36).HasColumnType("decimal(18,2)").IsRequired(false);

            b.HasIndex(x => new { x.CalendarSlotId, x.CustomerTypeId }).IsUnique();
        });

        // ===== AppSpecialDates =====
        builder.Entity<SpecialDate>(b =>
        {
            b.ToTable("AppSpecialDates");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);

            b.Property(x => x.WeekdaysMask).IsRequired(false);

            // Tránh tạo trùng config (TenantId + GolfCourseId + Name)
            b.HasIndex(x => new { x.TenantId, x.GolfCourseId, x.Name }).IsUnique();
        });

        // ===== AppEmails =====
        builder.Entity<Email>(b =>
        {
            b.ConfigureByConvention();
            b.ToTable("AppEmails");

            b.Property(x => x.TemplateName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Subject).IsRequired().HasMaxLength(512);
            b.Property(x => x.ToEmails).IsRequired().HasMaxLength(2048);
            b.Property(x => x.CcEmails).HasMaxLength(2048);
            b.Property(x => x.BccEmails).HasMaxLength(2048);

            b.Property(x => x.BookingCode).HasMaxLength(128);
            b.Property(x => x.LastError).HasMaxLength(4000);

            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.BookingId });
            b.HasIndex(x => new { x.TenantId, x.BookingCode });
        });

        builder.Entity<News>(b =>
        {
            b.ToTable("AppNews");
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(255);
            b.Property(x => x.ShortDescription).IsRequired().HasMaxLength(1000);
            b.Property(x => x.ThumbnailUrl).HasMaxLength(500);

            b.HasIndex(x => new { x.TenantId, x.CreationTime });
        });

        builder.Entity<NewsRelated>(b =>
        {
            b.ToTable("AppNewsRelateds");
            b.ConfigureByConvention();

            b.Property(x => x.NewsId)
                .IsRequired();

            b.Property(x => x.RelatedNewsId)
                .IsRequired();

            b.HasOne<News>()
                .WithMany(x => x.RelatedNewsLinks)
                .HasForeignKey(x => x.NewsId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<News>()
                .WithMany()
                .HasForeignKey(x => x.RelatedNewsId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dùng cho GetAsync detail: lấy danh sách related theo NewsId
            b.HasIndex(x => new
            {
                x.TenantId,
                x.NewsId
            })
            .HasDatabaseName("IX_AppNewsRelateds_Tenant_NewsId");

            // Dùng khi cần reverse lookup hoặc kiểm tra related
            b.HasIndex(x => new
            {
                x.TenantId,
                x.RelatedNewsId
            })
            .HasDatabaseName("IX_AppNewsRelateds_Tenant_RelatedNewsId");

            // Chống chọn trùng tin liên quan trong cùng bài
            b.HasIndex(x => new
            {
                x.TenantId,
                x.NewsId,
                x.RelatedNewsId
            })
            .IsUnique()
            .HasDatabaseName("IX_AppNewsRelateds_Tenant_News_Related");
        });

        builder.Entity<AppHomePageConfig>(b =>
        {
            b.ToTable("AppHomePageConfigs");
            b.ConfigureByConvention();

            b.Property(x => x.ThemeKey).IsRequired().HasMaxLength(64);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => x.TenantId);

            b.HasMany(x => x.Widgets)
             .WithOne()
             .HasForeignKey(x => x.AppHomePageConfigId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AppHomePageWidget>(b =>
        {
            b.ToTable("AppHomePageWidgets");
            b.ConfigureByConvention();

            b.Property(x => x.WidgetKey).IsRequired().HasMaxLength(64);
            b.Property(x => x.ModuleKey).IsRequired().HasMaxLength(64);

            b.Property(x => x.Title).HasMaxLength(256);
            b.Property(x => x.ConfigJson).HasColumnType("nvarchar(max)");

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.AppHomePageConfigId, x.DisplayOrder });
            b.HasIndex(x => new { x.AppHomePageConfigId, x.WidgetKey }).IsUnique(false);

            b.HasMany(x => x.Items)
             .WithOne()
             .HasForeignKey(x => x.AppHomePageWidgetId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AppHomePageWidgetItem>(b =>
        {
            b.ToTable("AppHomePageWidgetItems");
            b.ConfigureByConvention();

            b.Property(x => x.Text).IsRequired().HasMaxLength(128);
            b.Property(x => x.Icon).HasMaxLength(128);
            b.Property(x => x.Action).HasMaxLength(512);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.AppHomePageWidgetId, x.DisplayOrder });
        });

        builder.Entity<FnbOrderActivity>(b =>
        {
            b.ToTable("AppFnbOrderActivities");
            b.ConfigureByConvention();

            b.Property(x => x.ActionType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(1000);

            b.HasIndex(x => new { x.OrderId, x.ActionTime });
        });

        // ===== Documentation =====
        builder.Entity<DocumentSection>(b =>
        {
            b.ToTable("AppDocumentSections");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Slug).IsRequired().HasMaxLength(200);
            b.Property(x => x.Icon).HasMaxLength(100);
            b.Property(x => x.FeatureName).HasMaxLength(200);
            b.Property(x => x.TenantPermissionName).HasMaxLength(200);
            b.Property(x => x.HostPermissionName).HasMaxLength(200);

            b.HasIndex(x => x.Slug).IsUnique();
            b.HasIndex(x => x.DisplayOrder);

            b.HasMany(x => x.Pages)
             .WithOne(x => x.Section)
             .HasForeignKey(x => x.SectionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DocumentPage>(b =>
        {
            b.ToTable("AppDocumentPages");
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(255);
            b.Property(x => x.Slug).IsRequired().HasMaxLength(200);
            b.Property(x => x.ContentHtml).HasColumnType("nvarchar(max)");
            b.Property(x => x.FeatureName).HasMaxLength(200);
            b.Property(x => x.TenantPermissionName).HasMaxLength(200);
            b.Property(x => x.HostPermissionName).HasMaxLength(200);

            b.HasIndex(x => new { x.SectionId, x.Slug }).IsUnique();
            b.HasIndex(x => new { x.SectionId, x.DisplayOrder });
        });
    }
}
