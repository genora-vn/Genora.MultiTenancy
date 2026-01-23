using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppZaloLogs;

public class AppZaloLogFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(AppZaloLogFeatures.GroupName, L("FeatureGroup:MiniAppZaloLog"));

        group.AddFeature(
            AppZaloLogFeatures.Management,
            defaultValue: "false",// mặc định TẮT
            displayName: L("Feature:MiniAppZaloLog"),
            description: L("Feature:MiniAppZaloLogDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}