using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppCalendarSlots;
public class AppCalendarSlotFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(AppCalendarSlotFeatures.GroupName, L("FeatureGroup:MiniAppCalendarSlot"));

        group.AddFeature(
            AppCalendarSlotFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppCalendarSlot"),
            description: L("Feature:MiniAppCalendarSlotDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}