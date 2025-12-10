using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppCustomerTypes;

public class AppCustomerTypeFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppCustomerTypeFeatures.GroupName,
            L("FeatureGroup:MiniAppCustomerType")
        );

        group.AddFeature(
            AppCustomerTypeFeatures.Management,
            defaultValue: "false", // mặc định TẮT
            displayName: L("Feature:MiniAppCustomerType"),
            description: L("Feature:MiniAppCustomerTypeDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}