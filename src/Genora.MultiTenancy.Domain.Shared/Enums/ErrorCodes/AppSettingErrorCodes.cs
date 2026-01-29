namespace Genora.MultiTenancy.Enums.ErrorCodes;
public static class AppSettingErrorCodes
{
    public const string Prefix = "AppSetting:";

    public const string SettingKeyRequired = Prefix + "SettingKeyRequired";
    public const string DescriptionRequired = Prefix + "DescriptionRequired";

    public const string ImageRequired = Prefix + "ImageRequired";
    public const string SettingValueRequired = Prefix + "SettingValueRequired";

    public const string ImageOrKeepValueRequired = Prefix + "ImageOrKeepValueRequired";
}