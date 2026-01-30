using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails.Templates;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AutoMapper;
using Volo.Abp.Domain.Entities.Caching;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.TenantManagement;
using Volo.Abp.TextTemplating;
using Volo.Abp.TextTemplating.Scriban;
using Volo.Abp.VirtualFileSystem;

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
        });

        Configure<ZaloZbsOptions>(configuration.GetSection("Zalo:Zbs"));

        context.Services.AddEntityCache<AppSetting, AppSettingDto, Guid>();
        context.Services.AddTransient<IZaloZbsClient, ZaloZbsClient>();
        context.Services.AddTransient<IZaloZbsTemplateResolver, ZaloZbsTemplateResolver>();
    }
}
