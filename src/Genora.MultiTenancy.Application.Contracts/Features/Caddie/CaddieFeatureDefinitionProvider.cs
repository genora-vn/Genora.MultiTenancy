using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.Caddie;

public class CaddieFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            CaddieFeatures.GroupName,
            L("Feature:CaddieManagementGroup"));

        group.AddFeature(
            CaddieFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:CaddieManagement"),
            description: L("Feature:CaddieManagementDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MultiTenancyResource>(name);
    }
}
