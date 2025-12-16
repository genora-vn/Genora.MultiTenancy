using Genora.MultiTenancy.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Genora.MultiTenancy.Features.AppCustomers;

public class AppCustomerFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            AppCustomerFeatures.GroupName,
            L("FeatureGroup:MiniAppCustomer"));

        group.AddFeature(
            AppCustomerFeatures.Management,
            defaultValue: "false",
            displayName: L("Feature:MiniAppCustomer"),
            description: L("Feature:MiniAppCustomerDesc"),
            valueType: new ToggleStringValueType()
        );
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}