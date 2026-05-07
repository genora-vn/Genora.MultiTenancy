using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.SalonBeauty;

public class SalonBeautyFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            SalonBeautyFeatures.GroupName,
            L("Feature:SalonBeautyManagementGroup"));

        group.AddFeature(
            SalonBeautyFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:SalonBeautyManagement"),
            description: L("Feature:SalonBeautyManagementDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MultiTenancyResource>(name);
    }
}
