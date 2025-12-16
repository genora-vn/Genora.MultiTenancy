using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppBookingFeatures;

public class AppBookingFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppBookingFeatures.GroupName,
            L("FeatureGroup:MiniAppBooking"));

        group.AddFeature(
            AppBookingFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppBooking"),
            description: L("Feature:MiniAppBookingDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}