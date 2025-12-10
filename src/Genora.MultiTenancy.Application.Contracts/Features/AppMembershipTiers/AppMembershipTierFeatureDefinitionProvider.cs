using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppMembershipTiers;

public class AppMembershipTierFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppMembershipTierFeatures.GroupName,
            L("FeatureGroup:MiniAppMembershipTier"));

        group.AddFeature(
            AppMembershipTierFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppMembershipTier"),
            description: L("Feature:MiniAppMembershipTierDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}