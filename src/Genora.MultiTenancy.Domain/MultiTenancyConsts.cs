using Volo.Abp.Identity;

namespace Genora.MultiTenancy;

public static class MultiTenancyConsts
{
    public const string DbTablePrefix = "App";
    public const string? DbSchema = null;
    public const bool IsEnabled = true;
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = IdentityDataSeedContributor.AdminPasswordDefaultValue;
}
