using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.Domain.Entities.Caching;
using System;
using Genora.MultiTenancy.Books;
using Genora.MultiTenancy.Apps.AppSettings;

namespace Genora.MultiTenancy;

[DependsOn(
    typeof(MultiTenancyDomainModule),
    typeof(MultiTenancyApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class MultiTenancyApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MultiTenancyApplicationModule>();
        });

        context.Services.AddEntityCache<Book, BookDto, Guid>();
        context.Services.AddEntityCache<AppSetting, AppSettingDto, Guid>();
    }
}
