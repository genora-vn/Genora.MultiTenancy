using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppProshopFeatures;

public class AppProshopFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppProshopFeatures.GroupName,
            L("FeatureGroup:MiniAppProshop")
        );

        group.AddFeature(
            AppProshopFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppProshop"),
            description: L("Feature:MiniAppProshopDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
