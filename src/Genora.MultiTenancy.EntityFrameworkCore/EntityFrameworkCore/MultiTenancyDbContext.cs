using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.Diagnostics;
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
    }
}
