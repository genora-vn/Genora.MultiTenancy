using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppFnbFeatures;
public class AppFnbFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppFnbFeatures.GroupName,
            L("FeatureGroup:MiniAppFnb")
        );

        group.AddFeature(
            AppFnbFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppFnb"),
            description: L("Feature:MiniAppFnbDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}