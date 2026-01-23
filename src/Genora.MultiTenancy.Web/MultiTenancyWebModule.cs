using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.AuditLogs;
using Genora.MultiTenancy.EntityFrameworkCore;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.TenantManagement;
using Genora.MultiTenancy.Web.HealthChecks;
using Genora.MultiTenancy.Web.Menus;
using Genora.MultiTenancy.Web.Middlewares;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundJobs.Hangfire;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Emailing;              // ✅ ADD: IEmailSender
using Volo.Abp.FeatureManagement;
using Volo.Abp.Hangfire;
using Volo.Abp.Identity.Web;
using Volo.Abp.Localization;
using Volo.Abp.MailKit;              // ✅ ADD: AbpMailKitModule (SMTP sender)
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Genora.MultiTenancy.Web;

[DependsOn(
    typeof(MultiTenancyHttpApiModule),
    typeof(MultiTenancyApplicationModule),
    typeof(MultiTenancyEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpFeatureManagementWebModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpMailKitModule), //Bật SMTP EmailSender (thay NullEmailSender)
    typeof(AbpHangfireModule),
    typeof(AbpBackgroundJobsHangfireModule)
)]
public class MultiTenancyWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(MultiTenancyResource),
                typeof(MultiTenancyDomainModule).Assembly,
                typeof(MultiTenancyDomainSharedModule).Assembly,
                typeof(MultiTenancyApplicationModule).Assembly,
                typeof(MultiTenancyApplicationContractsModule).Assembly,
                typeof(MultiTenancyWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("MultiTenancy");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                var certPath = Path.Combine(hostingEnvironment.ContentRootPath, "openiddict.pfx");

                serverBuilder.AddProductionEncryptionAndSigningCertificate(
                    certPath,
                    configuration["AuthServer:CertificatePassPhrase"]!,
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable
                );

                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.AddCors(options =>
        {
            options.AddPolicy("ZaloPolicy", builder =>
            {
                builder
                    .WithOrigins("https://h5.zdn.vn")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB tổng request
        });

        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });

        // 1) Cấu hình ngôn ngữ ABP
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("vi", "vi", "Tiếng Việt"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.DefaultResourceType = typeof(MultiTenancyResource);
        });

        // 2) Cấu hình culture cho pipeline ASP.NET Core
        Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("vi"),
                new CultureInfo("en")
            };

            options.DefaultRequestCulture = new RequestCulture("vi");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        // đọc section từ appsettings.json
        Configure<AuditLogCleanupOptions>(configuration.GetSection("AuditLogCleanup"));

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });

            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            });
        }

        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true;
            options.IsEnabledForAnonymousUsers = false;
            options.IsEnabledForGetRequests = false;
            options.ApplicationName = "Genora";
        });

        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = true;
        });

        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = true;
        });

        context.Services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(configuration.GetConnectionString("Default"));
        });

        context.Services.AddHangfireServer();

        // ✅ Replace auditing store
        context.Services.Replace(ServiceDescriptor.Transient<IAuditingStore, HostRedirectAuditingStore>());

        // Register DI Zalo Service
        context.Services.AddHttpClient();
        context.Services.AddTransient<IZaloOAuthClient, ZaloOAuthClient>();
        context.Services.AddTransient<IZaloTokenProvider, ZaloTokenProvider>();
        context.Services.AddTransient<IZaloApiClient, ZaloApiClient>();

        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureHealthChecks(context);
        ConfigureAuthentication(context);
        ConfigureAutoMapper(context);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);

        Configure<AbpTenantResolveOptions>(options =>
        {
            options.TenantResolvers.Clear();
            options.TenantResolvers.Add(new HostTenantResolveContributor());
        });

        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = true;
        });

        Configure<AbpAspNetCoreMultiTenancyOptions>(o =>
        {
            o.TenantKey = "tenant";
        });

        Configure<AbpTenantResolveOptions>(o =>
        {
            o.TenantResolvers.Add(new DomainTenantResolveContributor("{0}.local")); // test1.local -> "test1"
            o.TenantResolvers.Add(new HeaderTenantResolveContributor());
            o.TenantResolvers.Add(new QueryStringTenantResolveContributor());
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddMultiTenancyHealthChecks();
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle => { bundle.AddFiles("/global-styles.css"); }
            );

            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle => { bundle.AddFiles("/global-scripts.js"); }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureAutoMapper(ServiceConfigurationContext context)
    {
        // (giữ nguyên logic, chỉ gộp để tránh cấu hình lặp)
        context.Services.AddAutoMapperObjectMapper<MultiTenancyWebModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MultiTenancyWebModule>(validate: false);
            options.AddMaps<MultiTenancyApplicationModule>(validate: false);
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MultiTenancyWebModule>();

            if (hostingEnvironment.IsDevelopment())
            {
                options.FileSets.ReplaceEmbeddedByPhysical<MultiTenancyDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Genora.MultiTenancy.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<MultiTenancyDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Genora.MultiTenancy.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<MultiTenancyApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Genora.MultiTenancy.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<MultiTenancyApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Genora.MultiTenancy.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<MultiTenancyHttpApiModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}Genora.MultiTenancy.HttpApi"));
                options.FileSets.ReplaceEmbeddedByPhysical<MultiTenancyWebModule>(hostingEnvironment.ContentRootPath);
            }
        });
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new MultiTenancyMenuContributor());
        });

        Configure<AbpToolbarOptions>(options =>
        {
            options.Contributors.Add(new MultiTenancyToolbarContributor());
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(MultiTenancyApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "MultiTenancy API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        // ✅ FIX: log đúng type + đảm bảo dùng IEmailSender thật (MailKit) thay vì NullEmailSender
        var sp = context.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<MultiTenancyWebModule>>();
        var sender = sp.GetRequiredService<IEmailSender>();
        logger.LogWarning("IEmailSender implementation = {Type}", sender.GetType().FullName);

        var jobManager = sp.GetRequiredService<IBackgroundJobManager>();
        logger.LogWarning("IBackgroundJobManager implementation = {Type}", jobManager.GetType().FullName);

        app.UseCors("ZaloPolicy");

        var opts = context.ServiceProvider.GetRequiredService<IOptions<AuditLogCleanupOptions>>().Value;
        if (opts.Enabled)
        {
            context.AddBackgroundWorkerAsync<AuditLogCleanupWorker>();
        }

        app.UseHangfireDashboard("/hangfire");

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseRouting();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseMiddleware<TenantAutoMigrateMiddleware>();
        app.UseMiddleware<LogEnrichmentMiddleware>();

        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MultiTenancy API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
