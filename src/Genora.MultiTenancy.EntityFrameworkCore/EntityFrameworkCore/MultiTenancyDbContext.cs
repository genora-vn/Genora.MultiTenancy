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
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppNews;
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

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

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
            b.HasIndex(x => x.CreationTime);
        });
    }
}
