using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppZaloAuths;

public class AppZaloAuthFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(AppZaloAuthFeatures.GroupName, L("FeatureGroup:MiniAppZaloAuth"));

        group.AddFeature(
            AppZaloAuthFeatures.Management,
            defaultValue: "false",// mặc định TẮT
            displayName: L("Feature:MiniAppZaloAuth"),
            description: L("Feature:MiniAppZaloAuthDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}