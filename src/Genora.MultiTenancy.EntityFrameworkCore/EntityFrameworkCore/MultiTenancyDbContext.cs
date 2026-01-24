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
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
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
    public DbSet<CustomerMembership> CustomerMembership { get; set; }
    public DbSet<Booking> Booking { get; set; }
    public DbSet<BookingPlayer> BookingPlayer { get; set; }
    public DbSet<BookingStatusHistory> BookingStatusHistory { get; set; }
    public DbSet<ZaloAuth> ZaloAuths { get; set; }
    public DbSet<ZaloLog> ZaloLogs { get; set; }  // nếu có
    public DbSet<SpecialDate> SpecialDates { get; set; }  // nếu có
    public DbSet<Email> AppEmails { get; set; }

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
        // để trống – OnConfiguring sẽ check null
    }

    // ⬅️ Bật log/diagnostic & interceptor
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Có interceptor thì gắn; nếu null (design-time) thì bỏ qua
        if (_sqlInterceptor is not null)
            optionsBuilder.AddInterceptors(_sqlInterceptor);

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
        });

        // ===== AppGolfCourses =====
        builder.Entity<GolfCourse>(b =>
        {
            b.ToTable("AppGolfCourses");
            b.ConfigureByConvention();

            b.Property(x => x.FrameTimes).HasMaxLength(50);
            b.Property(x => x.NumberHoles);
            b.Property(x => x.Utilities).HasMaxLength(20);
        });

        // ===== AppBookingPlayers =====
        builder.Entity<BookingPlayer>(b =>
        {
            b.ToTable("AppBookingPlayers");
            b.ConfigureByConvention();

            b.Property(x => x.PricePerPlayer).HasColumnType("decimal(18,2)");
            b.Property(x => x.VgaCode).HasMaxLength(50);
        });

        // ===== AppBookings =====
        builder.Entity<Booking>(b =>
        {
            b.ToTable("AppBookings");
            b.ConfigureByConvention();

            b.Property(x => x.NumberHole).HasMaxLength(20);

            b.Property(x => x.Utility).HasMaxLength(20).HasColumnName("Ultility");

            b.Property(x => x.IsExportInvoice);

            // ✅ Invoice fields (new)
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

            // Nếu muốn đảm bảo unique theo (CalendarSlotId, CustomerTypeId)
            b.HasIndex(x => new { x.CalendarSlotId, x.CustomerTypeId }).IsUnique();
        });

        // ===== AppSpecialDates =====
        builder.Entity<SpecialDate>(b =>
        {
            b.ToTable("AppSpecialDates");
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);

            // tránh tạo trùng config (TenantId + GolfCourseId + Name)
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
    }
}
