using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails.Templates;
using Genora.MultiTenancy.AppServices.AppPayments;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppDtos.UrBox;
using Genora.MultiTenancy.AppServices.UrBox;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AutoMapper;
using Volo.Abp.Domain.Entities.Caching;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.TenantManagement;
using Volo.Abp.TextTemplating;
using Volo.Abp.TextTemplating.Scriban;
using Volo.Abp.VirtualFileSystem;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;
using Genora.MultiTenancy.AppServices.SalonBeauty;
using Genora.MultiTenancy.AppServices.SalonBeauties;

namespace Genora.MultiTenancy;

[DependsOn(
    typeof(MultiTenancyDomainModule),
    typeof(MultiTenancyApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpTextTemplatingScribanModule)
    )]
public class MultiTenancyApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MultiTenancyApplicationModule>();
        });

        // Hiển thị modal lỗi cho client
        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendExceptionsDetailsToClients = false;
            options.SendStackTraceToClients = false;
        });

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MultiTenancyApplicationModule>("Genora.MultiTenancy");
        });

        Configure<AbpTextTemplatingOptions>(options =>
        {
            options.DefinitionProviders.Add<AppEmailTemplateDefinitionProvider>();
        });

        Configure<AbpSettingOptions>(options =>
        {
            options.DefinitionProviders.Add<AppEmailSettingDefinitionProvider>();
            options.DefinitionProviders.Add<ZaloSettingDefinitionProvider>();
            options.DefinitionProviders.Add<ZaloPaymentSettingDefinitionProvider>();
        });

        //Configure<ZaloZbsOptions>(configuration.GetSection("Zalo:Zbs"));

        context.Services.AddEntityCache<AppSetting, AppSettingDto, Guid>();
        context.Services.AddTransient<IZaloZbsClient, ZaloZbsClient>();
        context.Services.AddTransient<IZaloZbsTemplateResolver, ZaloZbsTemplateResolver>();
        context.Services.AddTransient<IZaloZbsToggleProvider, ZaloZbsToggleProvider>();
        context.Services.AddTransient<IZaloRuntimeConfigProvider, ZaloRuntimeConfigProvider>();
        context.Services.AddTransient<IZaloTokenProvider, ZaloTokenProvider>();
        context.Services.AddTransient<IZaloApiClient, ZaloApiClient>();
        context.Services.AddTransient<IZaloOAuthClient, ZaloOAuthClient>();
        context.Services.AddScoped<IMiniAppZaloNewsService, MiniAppZaloNewsService>();

        // Salon Beauty services
        context.Services.AddScoped<ISalonBeautyCustomerAppService, SalonBeautyCustomerAppService>();
        context.Services.AddScoped<ISalonBeautyServiceCategoryAppService, SalonBeautyServiceCategoryAppService>();
        context.Services.AddScoped<ISalonBeautyServiceAppService, SalonBeautyServiceAppService>();
        context.Services.AddScoped<ISalonBeautyStylistAppService, SalonBeautyStylistAppService>();
        context.Services.AddScoped<ISalonBeautyBookingAppService, SalonBeautyBookingAppService>();
        context.Services.AddScoped<ISalonBeautyLoyaltyAppService, SalonBeautyLoyaltyAppService>();

        // VietQR API client — timeout 5s, tránh block prepare-order quá lâu
        context.Services.AddHttpClient("VietQR", client =>
        {
            client.BaseAddress = new Uri("https://api.vietqr.io");
            client.Timeout     = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Hoa Linh DMS API client
        context.Services.AddHttpClient("HoaLinhDms", client =>
        {
            var config = context.Services.GetConfiguration();
            var baseUrl = config["HoaLinhApi:BaseUrl"] ?? "https://dmsapi.hoalinh.io.vn";
            var apiKey = config["HoaLinhApi:ApiKey"] ?? "";
            var timeout = int.TryParse(config["HoaLinhApi:TimeoutSeconds"], out var t) ? t : 30;

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        });

        context.Services.AddScoped<IHlApiClientService, HlApiClientService>();
        context.Services.AddScoped<IHlDataAccessService, HlDataAccessService>();
        context.Services.AddScoped<IHlPaymentService, HlPaymentService>();
        context.Services.AddScoped<IHlCustomerAppService, HlCustomerAppService>();
        context.Services.AddScoped<IHlPointAppService, HlPointAppService>();
        Configure<Genora.MultiTenancy.HoaLinh.HlPointExpireOptions>(
            configuration.GetSection(Genora.MultiTenancy.HoaLinh.HlPointExpireOptions.SectionName));
        Configure<Genora.MultiTenancy.HoaLinh.HlLoyaltyOptions>(
            configuration.GetSection(Genora.MultiTenancy.HoaLinh.HlLoyaltyOptions.SectionName));

        // ── UrBox (kho quà eVoucher) ─────────────────────────────────────────
        var urBoxSection = configuration.GetSection(UrBoxSettings.SectionName);
        Configure<UrBoxSettings>(urBoxSection);

        var urBoxBaseUrl = configuration[$"{UrBoxSettings.SectionName}:UrBoxApiUrl"] ?? "https://sandapi.urbox.dev";
        var urBoxTimeout = int.TryParse(configuration[$"{UrBoxSettings.SectionName}:TimeoutSeconds"], out var ubt) ? ubt : 30;
        context.Services.AddHttpClient("UrBox", client =>
        {
            client.BaseAddress = new Uri(urBoxBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(urBoxTimeout > 0 ? urBoxTimeout : 30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        context.Services.AddScoped<IUrBoxService, UrBoxService>();
    }
}
