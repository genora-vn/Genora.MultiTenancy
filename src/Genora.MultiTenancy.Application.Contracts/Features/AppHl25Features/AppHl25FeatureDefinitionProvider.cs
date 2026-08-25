using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppHl25Features;

public class AppHl25FeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppHl25Features.GroupName,
            L("FeatureGroup:Hl25")
        );

        group.AddFeature(
            AppHl25Features.Management,
            defaultValue: "false",
            displayName: L("Feature:Hl25"),
            description: L("Feature:Hl25Desc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
