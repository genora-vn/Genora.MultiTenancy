using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppPaymentConfigurationFeatures;

public class AppPaymentConfigurationFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppPaymentConfigurationFeatures.GroupName,
            L("FeatureGroup:MiniAppPaymentConfiguration"));

        group.AddFeature(
            AppPaymentConfigurationFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppPaymentConfiguration"),
            description: L("Feature:MiniAppPaymentConfigurationDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
