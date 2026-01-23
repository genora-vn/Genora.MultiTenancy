using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppSpecialDates;

public class AppSpecialDateFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(AppSpecialDateFeatures.GroupName, L("FeatureGroup:MiniAppSpecialDate"));

        group.AddFeature(
            AppSpecialDateFeatures.Management,
            defaultValue: "false",// mặc định TẮT
            displayName: L("Feature:MiniAppSpecialDate"),
            description: L("Feature:MiniAppSpecialDateDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}