using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppHlgFeatures;

public class AppHlgFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppHlgFeatures.GroupName,
            L("FeatureGroup:Hlg")
        );

        group.AddFeature(
            AppHlgFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:Hlg"),
            description: L("Feature:HlgDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
