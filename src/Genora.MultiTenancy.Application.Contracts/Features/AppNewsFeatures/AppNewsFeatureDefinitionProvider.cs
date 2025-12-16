using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppNewsFeatures;
public class AppNewsFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppNewsFeatures.GroupName,
            L("FeatureGroup:MiniAppNews")
        );

        group.AddFeature(
            AppNewsFeatures.Management,
            defaultValue: "false", // mặc định tắt
            displayName: L("Feature:MiniAppNews"),
            description: L("Feature:MiniAppNewsDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}