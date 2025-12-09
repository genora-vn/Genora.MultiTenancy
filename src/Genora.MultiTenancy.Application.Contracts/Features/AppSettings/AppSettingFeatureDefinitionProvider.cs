using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppSettings;

public class AppSettingFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(AppSettingFeatures.GroupName, L("FeatureGroup:MiniAppSetting"));

        group.AddFeature(
            AppSettingFeatures.Management,
            defaultValue: "false",// mặc định TẮT
            displayName: L("Feature:MiniAppSetting"),
            description: L("Feature:MiniAppSettingDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}