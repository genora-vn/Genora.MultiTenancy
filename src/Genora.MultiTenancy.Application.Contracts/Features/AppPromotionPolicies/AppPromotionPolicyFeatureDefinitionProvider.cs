using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppPromotionPolicies;

public class AppPromotionPolicyFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppPromotionPolicyFeatures.GroupName,
            L("FeatureGroup:MiniAppPromotionPolicy"));

        group.AddFeature(
            AppPromotionPolicyFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppPromotionPolicy"),
            description: L("Feature:MiniAppPromotionPolicyDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
