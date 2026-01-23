using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AppServices.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails.Templates;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Microsoft.Extensions.DependencyInjection;
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
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MultiTenancyApplicationModule>();
        });

        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendExceptionsDetailsToClients = true;
            options.SendStackTraceToClients = true;
        });

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MultiTenancyApplicationModule>();
        });

        Configure<AbpTextTemplatingOptions>(options =>
        {
            options.DefinitionProviders.Add<AppEmailTemplateDefinitionProvider>();
        });

        Configure<AbpSettingOptions>(options =>
        {
            options.DefinitionProviders.Add<AppEmailSettingDefinitionProvider>();
        });

        context.Services.AddEntityCache<AppSetting, AppSettingDto, Guid>();
        context.Services.AddTransient<IZaloZbsClient, ZaloZbsClient>();
    }
}
