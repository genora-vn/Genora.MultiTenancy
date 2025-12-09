using Genora.MultiTenancy.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Genora.MultiTenancy.EntityFrameworkCore;

[DependsOn(
    typeof(MultiTenancyDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
    )]
public class MultiTenancyEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {

        MultiTenancyEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //context.Services.AddAbpDbContext<MultiTenancyDbContext>(options =>
        //{
        //        /* Remove "includeAllEntities: true" to create
        //         * default repositories only for aggregate roots */
        //    options.AddDefaultRepositories(includeAllEntities: true);
        //    options.AddRepository<Tenant, CustomTenantRepository>();
        //});

        //if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        //{
        //    return;
        //}

        //Configure<AbpDbContextOptions>(options =>
        //{
        //    /* The main point to change your DBMS.
        //     * See also MultiTenancyDbContextFactory for EF Core tooling. */

        //    options.UseSqlServer();

        //});

        context.Services.AddAbpDbContext<MultiTenancyDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
        context.Services.AddSingleton<SerilogCommandInterceptor>();

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer(sql =>
            {
                sql.CommandTimeout(180);                 // lệnh (migrate) có thể lâu
                //sql.EnableRetryOnFailure(
                //    maxRetryCount: 10,
                //    maxRetryDelay: TimeSpan.FromSeconds(5),
                //    errorNumbersToAdd: null
                //);
            });
        });

    }
}
