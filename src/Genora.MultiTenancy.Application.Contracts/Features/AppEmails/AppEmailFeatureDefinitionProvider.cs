using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppEmails;

public class AppEmailFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppEmailFeatures.GroupName,
            L("FeatureGroup:MiniAppEmail")
        );

        group.AddFeature(
            AppEmailFeatures.Management,
            defaultValue: "false", // mặc định TẮT
            displayName: L("Feature:MiniAppEmail"),
            description: L("Feature:MiniAppEmailDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}